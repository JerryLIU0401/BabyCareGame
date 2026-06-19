using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using CardModel;
using Manager;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine.UI;

/// <summary>
/// 管理 AR 圖像辨識、圖庫輪替、模型生成與卡牌使用事件。
/// </summary>
public class ImageTrackingController : MonoBehaviour
{
#if UNITY_EDITOR
    // Editor 的 XR Simulation 追蹤更新是非同步流程，多等幾幀可讓 SimulatedTrackedImage 停止被底層讀取後再卸載場景。
    private const int EditorSceneExitDelayFrames = 8;
    // 重新進入掃描場景時先等待新 Camera 與 XR Origin 穩定，避免 Simulation provider 沿用上一輪已銷毀的 Camera。
    private const int EditorSceneEntryDelayFrames = 3;
    // Camera 就緒等待需要有上限，避免場景綁定異常時 Coroutine 永遠停住而沒有錯誤訊息。
    private const int EditorCameraReadyMaxFrames = 60;
#endif

    [Header("AR Components")]
    [SerializeField] private ARSession arSession;
    public ARTrackedImageManager trackedImageManager;
    public XRReferenceImageLibrary[] imageLibraries;
    
    public GameObject[] prefabs;
    private Dictionary<string, GameObject> prefabDictionary = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> spawnedPrefabs = new Dictionary<string, GameObject>();
    private Dictionary<string, TrackingState> trackingStates = new Dictionary<string, TrackingState>();
    private Vector3 scaleFactor = new Vector3(0.1f, 0.1f, 0.1f); // 可調整的縮放因子
    
    //暫存當前卡牌資訊，讓使用按鈕可以運作
    private string currentImageName = "";
    
    [Header("圖庫切換設定")]
    [SerializeField] private float checkInterval = 2f;
    // Reset 後保留緩衝時間，讓 XR provider 有機會完成舊 Trackable 的清理。
    [SerializeField] private float resetStabilizeSeconds = 0.75f;
    // 重新啟動追蹤後延後恢復自動輪替，避免剛 reset 完就立刻再次切圖庫。
    [SerializeField] private float autoSwitchResumeDelay = 1f;
    // 圖庫切換本身也要分幀處理，避免同一幀停止、換圖庫、啟動造成底層狀態競爭。
    [SerializeField] private float librarySwitchDelay = 0.2f;
    [Header("實機快速圖庫切換設定")]
    // 實機平台不受 XR Simulation 銷毀時序限制，因此可縮短每個圖庫停留時間，降低玩家等待目標卡被輪到的時間。
    [SerializeField] private float mobileCheckInterval = 0.6f;
    // 實機切換圖庫仍保留極短緩衝，避免同一幀停用與啟用 ARTrackedImageManager 造成 provider 狀態競爭。
    [SerializeField] private float mobileLibrarySwitchDelay = 0.05f;
    // 實機切換後只需要短冷卻，讓自動輪巡能快速進入下一個 library。
    [SerializeField] private float mobileAutoSwitchResumeDelay = 0.15f;
    // 實機重新偵測時仍保留短暫 Session 穩定時間，避免舊 Trackable 立即回流但不拖慢 Android/iOS 測試。
    [SerializeField] private float mobileResetStabilizeSeconds = 0.12f;
    
    private int currentLibraryIndex = 0;
    private float timeSinceLastDetection = 0f;
    private float autoSwitchCooldownTimer = 0f;
    // 這些狀態用來區分「目前正在追蹤」與「本輪已建立模型」，避免追蹤暫斷時又恢復自動切圖庫。
    private bool hasTrackedImage = false;
    private bool hasDetectedCard = false;
    private bool isResettingDetection = false;
    private bool isSwitchingLibrary = false;
    private bool isAutoLibrarySwitchingEnabled = true;
    private bool isTrackingEventSubscribed = false;
    // 初始化與離場狀態分開保存，避免場景卸載和重新進場時同一套 AR 物件被重複啟停。
    private bool hasInitializedTracking = false;
    private bool isSceneExitInProgress = false;
    private Coroutine initializationRoutine;

    public Text searchText;
    
    //使用按鈕顯示
    [SerializeField] private GameObject useBtnGameObject;
    // 重新偵測按鈕可由 Inspector 指定；未指定時仍可直接把 RetryDetectionBtn 綁到 UI OnClick。
    [SerializeField] private GameObject retryDetectionBtnGameObject;
    
    //註冊事件
    public event Action<ModelInfo> OnUseCardAction;

    /// <summary>
    /// 取得目前平台使用的自動輪巡間隔。
    /// </summary>
    /// <returns>Editor 使用保守秒數；Android/iOS 等實機平台使用快速秒數。</returns>
    private float EffectiveCheckInterval
    {
        get
        {
#if UNITY_EDITOR
            // Editor 仍以 Simulator 穩定性為優先，避免過快切換造成模擬圖像銷毀時序問題。
            return checkInterval;
#else
            // 實機平台目標是快速掃到玩家手上的卡牌，因此改用較短的輪巡間隔。
            return mobileCheckInterval;
#endif
        }
    }

    /// <summary>
    /// 取得目前平台使用的圖庫切換等待時間。
    /// </summary>
    /// <returns>Editor 使用保守秒數；Android/iOS 等實機平台使用快速秒數。</returns>
    private float EffectiveLibrarySwitchDelay
    {
        get
        {
#if UNITY_EDITOR
            // Simulator 需要較長等待，讓舊 Trackable 有時間從非同步流程中釋放。
            return librarySwitchDelay;
#else
            // Android/iOS 可接受更短切換緩衝，降低多圖庫輪巡的總等待時間。
            return mobileLibrarySwitchDelay;
#endif
        }
    }

    /// <summary>
    /// 取得目前平台使用的自動輪巡恢復冷卻。
    /// </summary>
    /// <returns>Editor 使用保守秒數；Android/iOS 等實機平台使用快速秒數。</returns>
    private float EffectiveAutoSwitchResumeDelay
    {
        get
        {
#if UNITY_EDITOR
            // Editor 保留較長冷卻，避免 reset 或切圖庫後立刻再次操作 Simulation provider。
            return autoSwitchResumeDelay;
#else
            // 實機測試重點是縮短每張卡等待時間，因此只保留很短的恢復冷卻。
            return mobileAutoSwitchResumeDelay;
#endif
        }
    }

    /// <summary>
    /// 取得目前平台使用的重新偵測穩定時間。
    /// </summary>
    /// <returns>Editor 使用保守秒數；Android/iOS 等實機平台使用快速秒數。</returns>
    private float EffectiveResetStabilizeSeconds
    {
        get
        {
#if UNITY_EDITOR
            // Simulator 重新偵測時較容易殘留舊 Trackable，因此保留原本較長等待。
            return resetStabilizeSeconds;
#else
            // 實機 provider 清理速度較穩定，短等待即可兼顧掃描速度與狀態穩定。
            return mobileResetStabilizeSeconds;
#endif
        }
    }
    
    /// <summary>
    /// 初始化 Prefab 對照表，讓辨識到的 Reference Image 名稱可以快速找到對應模型。
    /// </summary>
    private void Awake()
    {
        EnsureARSession();
        EnsureTrackedImageManager();

#if UNITY_EDITOR
        PrepareEditorComponentsForDelayedStartup();
#endif

        foreach (var prefab in prefabs)
        {
            prefabDictionary[prefab.name] = prefab;
            Debug.Log($"初始化預製件: {prefab.name}");
        }
    }

    /// <summary>
    /// 檢查 AR 元件與相機設定，協助在場景啟動時提早發現 Inspector 綁定問題。
    /// </summary>
    private void Start()
    {
        StartTrackingInitialization();
    }

    /// <summary>
    /// 啟用圖像追蹤事件並套用目前圖庫。
    /// </summary>
    private void OnEnable()
    {
        EnsureTrackedImageManager();
        StartTrackingInitialization();
    }

    /// <summary>
    /// 停用圖像追蹤事件，避免跨場景或物件停用後持續收到 AR 回呼。
    /// </summary>
    private void OnDisable()
    {
        initializationRoutine = null;
        UnsubscribeTrackingEvent();
        StopAllCoroutines();
    }
    
    /// <summary>
    /// 控制自動圖庫輪替與按鈕顯示；成功建立模型後會停止自動切換，避免干擾 ARFoundation 的 Trackable 狀態。
    /// </summary>
    private void Update()
    {
        UpdateDetectionButtons();
        UpdateAutoSwitchCooldown();

        if (ShouldSwitchLibrary())
        {
            timeSinceLastDetection += Time.deltaTime;
            if (timeSinceLastDetection >= EffectiveCheckInterval)
            {
                SwitchToNextLibrary();
                timeSinceLastDetection = 0f;
            }

            SetSearchText("辨識卡牌中");
        }
    }

    /// <summary>
    /// 接收 ARFoundation 的圖像追蹤變更，並依追蹤狀態建立、顯示或隱藏模型。
    /// </summary>
    /// <param name="eventArgs">ARFoundation 回報的新增、更新與移除圖像集合。</param>
    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        if (isResettingDetection)
        {
            return;
        }

        hasTrackedImage = false;
        // 新增圖像：產生模型並綁定
        foreach (var trackedImage in eventArgs.added)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                hasTrackedImage = true;
                HandleTrackedImage(trackedImage);
            }
        }
        
        // 更新圖像：必要時補建模型，再依追蹤狀態控制顯示與隱藏。
        foreach (var trackedImage in eventArgs.updated)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                hasTrackedImage = true;

                // 重新掃描後 ARFoundation 可能直接回報 updated 而不是 added；
                // 若模型已被重置流程清掉，必須在 updated 階段補建模型，避免只有文字更新但畫面沒有內容。
                string imageName = trackedImage.referenceImage.name;
                if (!spawnedPrefabs.ContainsKey(imageName))
                {
                    HandleTrackedImage(trackedImage);
                }
            }

            UpdateModelTransform(trackedImage);
        }
        
        // 移除圖像：隱藏對應模型
        foreach (var trackedImage in eventArgs.removed)
        {
            string imageName = trackedImage.referenceImage.name;
            if (spawnedPrefabs.TryGetValue(imageName, out var model))
            {
                model.SetActive(false);
                // 追蹤移除時只隱藏模型，不恢復自動切圖庫；玩家可透過重新偵測按鈕明確開始下一輪掃描。
                Debug.Log($"❌ 已移除：{imageName}");
            }

            // 圖像移除後要清掉最後追蹤狀態，否則下一次相同 Tracking 狀態回來時可能被誤判成無需更新。
            trackingStates.Remove(imageName);
        }
    }

    /// <summary>
    /// 依辨識到的圖像名稱建立或復用模型。
    /// </summary>
    /// <param name="trackedImage">ARFoundation 回報的已追蹤圖像。</param>
    private void HandleTrackedImage(ARTrackedImage trackedImage)
    {
        Debug.Log($"referenceImage name: {trackedImage.referenceImage.name}");
        Debug.Log($"referenceImage guid: {trackedImage.referenceImage.guid}");
        Debug.Log($"trackingState: {trackedImage.trackingState}");
        
        string imageName = trackedImage.referenceImage.name;
        
        if (string.IsNullOrEmpty(imageName))
        {
            Debug.LogError("❌ Reference Image 名稱為空，無法用名稱對應 Prefab。請檢查 XRReferenceImageLibrary 的 Image Name 是否正確帶入。");
            return;
        }

        
        currentImageName = imageName;

        if (spawnedPrefabs.TryGetValue(imageName, out GameObject existingModel))
        {
            // 同一張卡牌再次被回報時復用既有模型，避免遊戲層重複 Instantiate。
            AttachModelToTrackedImage(existingModel, trackedImage);
            MarkCardDetected(imageName);
            Debug.Log($"♻️ 已復用模型：{imageName}");
            return;
        }

        if (!prefabDictionary.TryGetValue(imageName, out GameObject prefab))
        {
            Debug.LogError($"❌ 找不到對應 prefab：{imageName}");
            return;
        }

        GameObject newPrefab = Instantiate(prefab, trackedImage.transform);
        AttachModelToTrackedImage(newPrefab, trackedImage);
        spawnedPrefabs[imageName] = newPrefab;
        MarkCardDetected(imageName);
        Debug.Log($"🆕 已生成模型：{imageName}");
    }

    /// <summary>
    /// 依追蹤狀態更新模型顯示，不在追蹤中斷時清除已建立模型。
    /// </summary>
    /// <param name="trackedImage">ARFoundation 回報的已追蹤圖像。</param>
    private void UpdateModelTransform(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;
        currentImageName = imageName;
        if (!spawnedPrefabs.TryGetValue(imageName, out var model)) return;

        bool shouldShowModel = trackedImage.trackingState == TrackingState.Tracking;
        bool isModelVisibilityCorrect = model.activeSelf == shouldShowModel;

        // 防止 trackingState 沒變也一直重複 SetActive，但不能擋住「模型已隱藏、追蹤已恢復」的重新顯示。
        if (trackingStates.TryGetValue(imageName, out var lastState)
            && lastState == trackedImage.trackingState
            && isModelVisibilityCorrect)
        {
            return;
        }

        trackingStates[imageName] = trackedImage.trackingState;

        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            // updated 回報的 ARTrackedImage 可能是新一輪 trackable，重新掛載可避免模型留在舊父物件底下。
            AttachModelToTrackedImage(model, trackedImage);
            // 模型已存在且重新追蹤成功時，仍維持自動切圖庫暫停，避免同一張卡牌造成 Trackable 狀態震盪。
            MarkCardDetected(imageName);
            Debug.Log($"✅ {imageName} 已追蹤成功，顯示模型");
        }
        else
        {
            model.SetActive(false);
            // 追蹤中斷只隱藏模型，保留資料讓玩家可按使用或重新偵測按鈕決定下一步。
            Debug.Log($"⚠️ {imageName} 追蹤中斷，隱藏模型");
        }
    }

    /// <summary>
    /// 在尚未成功建立模型時輪替圖庫。
    /// </summary>
    private void SwitchToNextLibrary()
    {
        if (!CanSwitchImageLibrary() || isSwitchingLibrary || isResettingDetection)
        {
            return;
        }

        // 圖庫切換會碰到 ARFoundation 底層 Trackable 狀態，因此交由協程分幀處理。
        StartCoroutine(SwitchToNextLibraryRoutine());
    }
    
    /// <summary>
    /// 讓 UI 使用按鈕讀取目前模型資訊並通知卡牌管理器。
    /// </summary>
    public void UseCardBtn()
    {
        if (spawnedPrefabs.ContainsKey(currentImageName))
        {
            //嘗試獲取分數並更新玩家分數
            ModelInfo modelInfo = spawnedPrefabs[currentImageName].GetComponent<ModelInfo>();
            if (modelInfo != null)
            {
               //啟用事件
               OnUseCardAction?.Invoke(modelInfo);
            }
        }
    }

    /// <summary>
    /// 供 UI 重新偵測按鈕呼叫，清除目前模型與追蹤狀態後恢復自動圖庫輪替。
    /// </summary>
    public void RetryDetectionBtn()
    {
        if (isResettingDetection || isSwitchingLibrary)
        {
            return;
        }

        StartCoroutine(ResetDetectionRoutine());
    }
    
    /// <summary>
    /// 離開 AR 場景前清理事件、Session、生成模型與 XR Origin。
    /// </summary>
    public void CleanupARBeforeSceneChange()
    {
        PrepareARTrackingForSceneExit();
        ResetARSessionForSceneExit();
        ClearARSceneObjectsForExit();
        Debug.Log("🧹 已清理 AR 場景中的物件與狀態");
    }

    /// <summary>
    /// 離開 AR 場景前以分幀方式清理事件、追蹤管理器、Session、生成模型與 XR Origin。
    /// </summary>
    /// <returns>等待 ARFoundation 或 XR Simulation 停止使用追蹤物件的協程列舉器。</returns>
    public IEnumerator CleanupARBeforeSceneChangeRoutine()
    {
        PrepareARTrackingForSceneExit();
        ResetARSessionForSceneExit();

#if UNITY_EDITOR
        for (int frameIndex = 0; frameIndex < EditorSceneExitDelayFrames; frameIndex++)
        {
            // Session 停用後再等待，讓 XR Simulation 的非同步 discoverer loop 有時間停止讀取模擬圖像與舊 Camera。
            yield return null;
        }
#else
        // 實機平台仍保留一幀緩衝，讓 ARTrackedImageManager 與 ARSession 停用狀態先同步到底層 provider。
        yield return null;
#endif

        ClearARSceneObjectsForExit();
        Debug.Log("🧹 已分幀清理 AR 場景中的物件與狀態");
    }

    /// <summary>
    /// 停止 AR 圖像追蹤與本控制器的背景協程，讓離場流程不再收到新的追蹤回呼。
    /// </summary>
    private void PrepareARTrackingForSceneExit()
    {
        isSceneExitInProgress = true;
        hasInitializedTracking = false;
        initializationRoutine = null;
        isResettingDetection = true;
        isSwitchingLibrary = false;
        isAutoLibrarySwitchingEnabled = false;
        autoSwitchCooldownTimer = 0f;

        // 離場時必須先停止自動輪替與重置協程，避免清理途中再次切換圖庫或重啟追蹤。
        StopAllCoroutines();
        UnsubscribeTrackingEvent();

        if (trackedImageManager != null)
        {
            // 先停用追蹤管理器，讓 ARFoundation 不再產生 trackedImagesChanged 或更新 Simulation 追蹤狀態。
            trackedImageManager.enabled = false;
        }
    }

    /// <summary>
    /// 依執行平台停止 ARSession，避免 Editor XR Simulation 在場景卸載前立即 Reset 已銷毀的模擬圖像。
    /// </summary>
    private void ResetARSessionForSceneExit()
    {
        EnsureARSession();
        if (arSession == null)
        {
            return;
        }

        arSession.enabled = false;

#if UNITY_EDITOR
        // Editor 的 XR Simulation 會用非同步迴圈讀取 SimulatedTrackedImage；離場時略過 Reset，避免它在物件卸載後重新計算追蹤品質。
        Debug.Log("🔁 AR Session 已停用；Editor XR Simulation 離場時略過 Reset。");
#else
        // 實機離場仍重置 Session，避免下一次進入 AR 場景沿用平台 provider 的舊追蹤狀態。
        arSession.Reset();
        Debug.Log("🔁 AR Session 已停用並重置");
#endif
    }

    /// <summary>
    /// 清除應用層生成物件與目前偵測狀態，讓下一次進入 AR 場景從乾淨狀態開始。
    /// </summary>
    private void ClearARSceneObjectsForExit()
    {
        prefabDictionary.Clear();
        ClearSpawnedModels();
        ResetDetectionState(false);

#if UNITY_EDITOR
        // Editor XR Simulation 由場景卸載自然釋放 XR Origin，避免手動停用 Camera 後 provider 仍持有失效引用。
#else
        var xrOrigin = FindAnyObjectByType<XROrigin>();
        if (xrOrigin != null)
        {
            // 離場前關閉 XR Origin 可釋放相機與子物件，但必須放在追蹤管理器停用與等待之後，避免 Simulation 仍讀取已銷毀圖像。
            xrOrigin.gameObject.SetActive(false);
        }
#endif
    }

    /// <summary>
    /// 啟動 AR 追蹤初始化流程，讓 Session、Camera 與 Image Tracking 以可控順序恢復。
    /// </summary>
    private void StartTrackingInitialization()
    {
        if (!isActiveAndEnabled || isSceneExitInProgress || hasInitializedTracking || initializationRoutine != null)
        {
            return;
        }

        initializationRoutine = StartCoroutine(InitializeTrackingAfterSceneReadyRoutine());
    }

    /// <summary>
    /// 等待場景中的 Camera 與 AR 元件就緒後，再重新啟動 ARSession 與圖像追蹤。
    /// </summary>
    /// <returns>等待 AR 場景初始化完成的協程列舉器。</returns>
    private IEnumerator InitializeTrackingAfterSceneReadyRoutine()
    {
        isResettingDetection = true;
        isAutoLibrarySwitchingEnabled = false;
        SetSearchText("初始化辨識中");
        UpdateDetectionButtons();

        EnsureARSession();
        EnsureTrackedImageManager();
        UnsubscribeTrackingEvent();

        if (trackedImageManager == null)
        {
            Debug.LogError("❌ 找不到 ARTrackedImageManager，圖像追蹤流程無法啟動。", this);
            initializationRoutine = null;
            yield break;
        }

        // 初始化期間先停用追蹤管理器，避免 ARSession 尚未穩定時就開始處理圖像事件。
        trackedImageManager.enabled = false;

#if UNITY_EDITOR
        for (int frameIndex = 0; frameIndex < EditorSceneEntryDelayFrames; frameIndex++)
        {
            // 新場景載入後先等 Camera 與 XR Origin 完成啟用，避免 Simulation provider 沿用上一輪已銷毀的 Camera。
            yield return null;
        }

        yield return WaitForARCameraReadyRoutine();
#else
        yield return null;
#endif

        if (arSession != null)
        {
#if UNITY_EDITOR
            // 新 Camera 已存在後才重置 Session，讓 XR Simulation 重新綁定這一輪場景狀態。
            arSession.Reset();
#endif
            arSession.enabled = true;
            yield return null;
        }
        else
        {
            Debug.LogWarning("[ImageTrackingController] 找不到 ARSession，將只啟動圖像追蹤管理器。", this);
        }

        ApplyCurrentLibrary();
        trackedImageManager.enabled = true;
        yield return null;

        SubscribeTrackingEvent();
        ResetDetectionState(true);
        SetAutoSwitchCooldown(EffectiveAutoSwitchResumeDelay);
        isResettingDetection = false;
        hasInitializedTracking = true;
        initializationRoutine = null;
        SetSearchText("辨識卡牌中");
        UpdateDetectionButtons();

        LogARCameraState();
        if (trackedImageManager.subsystem == null)
        {
            Debug.LogWarning("⚠️ AR Tracked Image Manager 尚未啟動，可能是 AR Session 啟動順序問題", this);
        }

        Debug.Log("已完成 AR 圖像追蹤延遲初始化");
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor 進場時先讓 AR 元件維持停用，避免 XR Simulation 在新 Camera 就緒前自動啟動。
    /// </summary>
    private void PrepareEditorComponentsForDelayedStartup()
    {
        if (trackedImageManager != null)
        {
            // 圖像追蹤必須等 Session 與 Camera 穩定後再啟用，避免 Simulation 追蹤事件讀到舊物件。
            trackedImageManager.enabled = false;
        }

        if (arSession != null)
        {
            // 場景中的 ARSession 預設也會被設為停用；這裡保留防護，避免 Inspector 被改回啟用時又提早啟動。
            arSession.enabled = false;
        }
    }

    /// <summary>
    /// 等待目前場景的新 AR Camera 可用，避免重新啟動 Simulation 時仍抓到上一輪已銷毀的 Camera。
    /// </summary>
    /// <returns>等待 Camera 就緒或超時的協程列舉器。</returns>
    private IEnumerator WaitForARCameraReadyRoutine()
    {
        int waitedFrames = 0;
        while (!HasUsableARCamera() && waitedFrames < EditorCameraReadyMaxFrames)
        {
            waitedFrames++;
            yield return null;
        }

        if (!HasUsableARCamera())
        {
            Debug.LogWarning("[ImageTrackingController] 等待 AR Camera 就緒逾時，仍會嘗試啟動 XR Simulation。", this);
        }
    }

    /// <summary>
    /// 判斷目前場景是否已有可用 Camera，讓 XR Simulation 能重新綁定本輪場景。
    /// </summary>
    /// <returns>存在 Main Camera 或 XR Origin 子 Camera 時回傳 true。</returns>
    private bool HasUsableARCamera()
    {
        return Camera.main != null || GetComponentInChildren<Camera>(true) != null;
    }
#endif

    /// <summary>
    /// 輸出 AR Camera 狀態，協助確認 Simulation 是否使用本輪場景中的 Camera。
    /// </summary>
    private void LogARCameraState()
    {
        Camera arCamera = GetComponentInChildren<Camera>(true);
        if (arCamera == null)
        {
            Debug.LogError("❌ 找不到 AR Camera！請確認 Main Camera 是 XR Origin 的子物件", this);
        }
        else
        {
            Debug.Log("✅ AR Camera 正常存在：" + arCamera.name, arCamera);
        }

        if (Camera.main == null)
        {
            Debug.LogWarning("Main Camera tag 沒有設定，請確認 Main Camera 有打 Tag。", this);
        }
    }

    /// <summary>
    /// 判斷目前是否應繼續自動切換圖庫。
    /// </summary>
    /// <returns>尚未偵測成功且不在重置流程中時回傳 true。</returns>
    private bool ShouldSwitchLibrary()
    {
        return isAutoLibrarySwitchingEnabled
               && !hasDetectedCard
               && !hasTrackedImage
               && !isResettingDetection
               && !isSwitchingLibrary
               && autoSwitchCooldownTimer <= 0f
               && CanSwitchImageLibrary();
    }

    /// <summary>
    /// 判斷圖庫設定是否足以套用目前圖庫。
    /// </summary>
    /// <returns>圖庫陣列與追蹤管理器皆有效時回傳 true。</returns>
    private bool CanUseImageLibraries()
    {
        return trackedImageManager != null && imageLibraries != null && imageLibraries.Length > 0;
    }

    /// <summary>
    /// 判斷是否真的需要切換圖庫。
    /// </summary>
    /// <returns>至少有兩個圖庫可輪替時回傳 true。</returns>
    private bool CanSwitchImageLibrary()
    {
        return CanUseImageLibraries() && imageLibraries.Length > 1;
    }

    /// <summary>
    /// 套用目前索引指向的圖庫。
    /// </summary>
    private void ApplyCurrentLibrary()
    {
        if (!CanUseImageLibraries())
        {
            return;
        }

        // Inspector 若調整圖庫數量，先校正索引可避免場景啟動時超出範圍。
        currentLibraryIndex %= imageLibraries.Length;
        trackedImageManager.referenceLibrary = imageLibraries[currentLibraryIndex];
    }

    /// <summary>
    /// 更新自動切換冷卻時間，避免 reset 後立即再次切換圖庫。
    /// </summary>
    private void UpdateAutoSwitchCooldown()
    {
        if (autoSwitchCooldownTimer > 0f)
        {
            autoSwitchCooldownTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// 設定自動輪替冷卻，讓 ARFoundation 有時間完成剛啟動後的追蹤狀態同步。
    /// </summary>
    /// <param name="cooldownSeconds">需要延後自動切換的秒數。</param>
    private void SetAutoSwitchCooldown(float cooldownSeconds)
    {
        autoSwitchCooldownTimer = Mathf.Max(autoSwitchCooldownTimer, cooldownSeconds);
    }

    /// <summary>
    /// 確保 ARSession 參考存在，支援場景中 ARSession 元件預設停用時仍可被控制器接管。
    /// </summary>
    private void EnsureARSession()
    {
        if (arSession == null)
        {
            arSession = FindFirstObjectByType<ARSession>(FindObjectsInactive.Include);
        }
    }

    /// <summary>
    /// 確保追蹤管理器存在，支援 Inspector 未綁定時從同物件補抓元件。
    /// </summary>
    private void EnsureTrackedImageManager()
    {
        if (trackedImageManager == null)
        {
            trackedImageManager = GetComponent<ARTrackedImageManager>();
        }
    }

    /// <summary>
    /// 訂閱圖像追蹤事件，避免重複訂閱造成同一事件被處理多次。
    /// </summary>
    private void SubscribeTrackingEvent()
    {
        if (trackedImageManager == null || isTrackingEventSubscribed)
        {
            return;
        }

        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        isTrackingEventSubscribed = true;
    }

    /// <summary>
    /// 取消訂閱圖像追蹤事件，讓重置與離場流程不會收到殘留回呼。
    /// </summary>
    private void UnsubscribeTrackingEvent()
    {
        if (trackedImageManager == null || !isTrackingEventSubscribed)
        {
            return;
        }

        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        isTrackingEventSubscribed = false;
    }

    /// <summary>
    /// 將模型掛回目前追蹤圖像底下，確保復用模型時位置與縮放一致。
    /// </summary>
    /// <param name="model">要顯示或復用的模型物件。</param>
    /// <param name="trackedImage">模型應對齊的 AR 圖像。</param>
    private void AttachModelToTrackedImage(GameObject model, ARTrackedImage trackedImage)
    {
        model.transform.SetParent(trackedImage.transform, false);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = scaleFactor;
        model.SetActive(true);
    }

    /// <summary>
    /// 標記本輪已成功建立可用模型，並停止自動切換圖庫。
    /// </summary>
    /// <param name="imageName">目前辨識成功的 Reference Image 名稱。</param>
    private void MarkCardDetected(string imageName)
    {
        currentImageName = imageName;
        hasTrackedImage = true;
        hasDetectedCard = true;
        isAutoLibrarySwitchingEnabled = false;
        timeSinceLastDetection = 0f;
        SetSearchText(imageName);
        UpdateDetectionButtons();
    }

    /// <summary>
    /// 更新使用卡牌與重新偵測按鈕顯示狀態。
    /// </summary>
    private void UpdateDetectionButtons()
    {
        if (useBtnGameObject != null)
        {
            // 只要本輪已有可用模型，就允許玩家使用卡牌；模型暫時隱藏時仍保留玩家決策權。
            useBtnGameObject.SetActive(hasDetectedCard && !isResettingDetection);
        }

        if (retryDetectionBtnGameObject != null)
        {
            // 重新偵測按鈕只在已有卡牌結果時顯示，避免玩家在初始掃描中誤觸重置流程。
            retryDetectionBtnGameObject.SetActive(hasDetectedCard && !isResettingDetection);
        }
    }

    /// <summary>
    /// 設定掃描提示文字。
    /// </summary>
    /// <param name="message">要顯示給玩家的提示內容。</param>
    private void SetSearchText(string message)
    {
        if (searchText != null)
        {
            searchText.text = message;
        }
    }

    /// <summary>
    /// 重置目前偵測狀態，並依需求決定是否恢復自動輪替。
    /// </summary>
    /// <param name="enableAutoSwitching">重置後是否恢復自動切換圖庫。</param>
    private void ResetDetectionState(bool enableAutoSwitching)
    {
        currentImageName = string.Empty;
        hasTrackedImage = false;
        hasDetectedCard = false;
        isAutoLibrarySwitchingEnabled = enableAutoSwitching;
        timeSinceLastDetection = 0f;
        trackingStates.Clear();
    }

    /// <summary>
    /// 重新啟動 AR Session，確保 XR provider 不會沿用上一輪卡牌追蹤狀態。
    /// </summary>
    private void ResetARSessionState()
    {
        EnsureARSession();
        if (arSession == null)
        {
            return;
        }

        // 重新掃描時必須確保 Session 處於啟用狀態，否則 Reset 只會停在半初始化狀態。
        if (!arSession.enabled)
        {
            arSession.enabled = true;
        }

        arSession.Reset();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        ResetImageTrackingValidationState();
#endif
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>
    /// 重置 ARFoundation Editor 驗證器的 TrackableId 快取，避免 Session reset 後同一張圖被回報為 added 時誤判重複。
    /// </summary>
    private void ResetImageTrackingValidationState()
    {
        if (trackedImageManager == null || trackedImageManager.subsystem == null)
        {
            return;
        }

        // ValidationUtility 是 ARFoundation 在 Editor/Development Build 使用的內部檢查器；
        // Session.Reset 會重置 provider，但這個檢查器不會自動清空，因此需要同步重建。
        var validationField = typeof(XRImageTrackingSubsystem).GetField(
            "m_ValidationUtility",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        if (validationField == null)
        {
            Debug.LogWarning("找不到 ARFoundation 圖像追蹤驗證器，略過 Editor 驗證狀態重置。");
            return;
        }

        validationField.SetValue(trackedImageManager.subsystem, new ValidationUtility<XRTrackedImage>());
        Debug.Log("已重置 ARFoundation 圖像追蹤驗證狀態");
    }
#endif

    /// <summary>
    /// 清除已生成模型，讓重新偵測從乾淨的應用層狀態開始。
    /// </summary>
    private void ClearSpawnedModels()
    {
        foreach (var spawned in spawnedPrefabs.Values)
        {
            if (spawned != null) Destroy(spawned);
        }

        spawnedPrefabs.Clear();
    }

    /// <summary>
    /// 手動重新偵測流程：暫停事件、停用追蹤、清空模型狀態、重置 Session 後再恢復輪詢。
    /// </summary>
    /// <returns>等待 ARFoundation 完成停用與重啟的協程列舉器。</returns>
    private IEnumerator ResetDetectionRoutine()
    {
        isResettingDetection = true;
        isAutoLibrarySwitchingEnabled = false;
        autoSwitchCooldownTimer = 0f;
        SetSearchText("重新初始化辨識中");
        UpdateDetectionButtons();

        UnsubscribeTrackingEvent();

        if (trackedImageManager != null)
        {
            // 先停用追蹤管理器，讓底層有機會移除舊的 Trackable 狀態。
            trackedImageManager.enabled = false;
        }

        ClearSpawnedModels();
        ResetDetectionState(false);

        yield return null;

        // 手動重新偵測代表玩家明確要換牌，重置 Session 可降低舊 Trackable 殘留機率。
        ResetARSessionState();

        yield return new WaitForSeconds(EffectiveResetStabilizeSeconds);
        yield return null;
        yield return null;

        ApplyCurrentLibrary();

        if (trackedImageManager != null)
        {
            trackedImageManager.enabled = true;
        }

        yield return null;

        SubscribeTrackingEvent();
        ResetDetectionState(true);
        SetAutoSwitchCooldown(EffectiveAutoSwitchResumeDelay);
        isResettingDetection = false;
        SetSearchText("辨識卡牌中");
        UpdateDetectionButtons();

        Debug.Log("已重新啟動 AR 圖像偵測流程");
    }

    /// <summary>
    /// 分幀切換圖庫，避免同步停用與啟用 ARTrackedImageManager 時留下重複 TrackableId。
    /// </summary>
    /// <returns>等待 ARFoundation 完成輕量圖庫切換的協程列舉器。</returns>
    private IEnumerator SwitchToNextLibraryRoutine()
    {
        isSwitchingLibrary = true;
        isAutoLibrarySwitchingEnabled = false;
        autoSwitchCooldownTimer = 0f;
        SetSearchText("切換辨識圖庫中");

        UnsubscribeTrackingEvent();

        if (trackedImageManager != null)
        {
            // 停用追蹤後再切換圖庫，避免舊圖庫的 Trackable 與新圖庫的 added 事件混在同一輪。
            trackedImageManager.enabled = false;
        }

        ClearSpawnedModels();
        ResetDetectionState(false);

        yield return null;

        // 自動輪替只切換圖庫，不重置 ARSession；Android 實機重建相機背景紋理時會造成黑屏閃爍。
        // 若玩家需要完全清掉舊追蹤狀態，仍可透過重新偵測按鈕走硬重置流程。

        yield return new WaitForSeconds(EffectiveLibrarySwitchDelay);

        currentLibraryIndex = (currentLibraryIndex + 1) % imageLibraries.Length;
        ApplyCurrentLibrary();

        yield return null;

        if (trackedImageManager != null)
        {
            trackedImageManager.enabled = true;
        }

        yield return null;

        SubscribeTrackingEvent();
        ResetDetectionState(true);
        SetAutoSwitchCooldown(EffectiveAutoSwitchResumeDelay);
        isSwitchingLibrary = false;
        SetSearchText("辨識卡牌中");

        print($"🔁 自動切換圖庫：{imageLibraries[currentLibraryIndex].name}");
    }
}
