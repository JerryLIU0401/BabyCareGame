using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using CardModel;
using Manager;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine.UI;


public class ImageTrackingController : MonoBehaviour
{
    [Header("AR Components")]
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
    
    private int currentLibraryIndex = 0;
    private float timeSinceLastDetection = 0f;
    private bool hasTrackedImage = false;

    public Text searchText;
    
    //使用按鈕顯示
    [SerializeField] private GameObject useBtnGameObject;
    
    //註冊事件
    public event Action<ModelInfo> OnUseCardAction;
    
    private void Awake()
    {
        foreach (var prefab in prefabs)
        {
            prefabDictionary[prefab.name] = prefab;
            Debug.Log($"初始化預製件: {prefab.name}");
        }
    }

    private void Start()
    {
        if (trackedImageManager == null) trackedImageManager = GetComponent<ARTrackedImageManager>();
        
        if (trackedImageManager.subsystem == null)
            Debug.LogWarning("⚠️ AR Tracked Image Manager 尚未啟動，可能是 AR Session 啟動順序問題");

        if (Camera.main == null)
            Debug.LogError("❌ Camera.main 為空，追蹤可能失敗");
        
        Camera arCamera = GetComponentInChildren<Camera>();
        if (arCamera == null)
        {
            Debug.LogError("❌ 找不到 AR Camera！請確認 Main Camera 是 XR Origin 的子物件");
        }
        else
        {
            Debug.Log("✅ AR Camera 正常存在：" + arCamera.name);
        }

        if (Camera.main == null)
        {
            Debug.LogWarning("Main Camera tag 沒有設定，請確認 Main Camera 有打 Tag。");
        }

    }

    private void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        trackedImageManager.referenceLibrary = imageLibraries[currentLibraryIndex];
    }

    private void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        StopAllCoroutines();
    }
    
    private void Update()
    {
        if (!hasTrackedImage)
        {
            timeSinceLastDetection += Time.deltaTime;
            if (timeSinceLastDetection >= checkInterval)
            {
                SwitchToNextLibrary();
                timeSinceLastDetection = 0f;
            }

            searchText.text = "辨識卡牌中";
            useBtnGameObject.SetActive(false);
        }
        else
        {
            useBtnGameObject.SetActive(true);
        }
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
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
        
        // 更新圖像：只控制顯示與隱藏
        foreach (var trackedImage in eventArgs.updated)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                hasTrackedImage = true;
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
                // useBtnGameObject.SetActive(false);
                Debug.Log($"❌ 已移除：{imageName}");
            }
        }
        
        if (hasTrackedImage && eventArgs.updated.Count > 0)
        {
            string lastTrackedName = eventArgs.updated[^1].referenceImage.name;
            searchText.text = lastTrackedName;
        }
    }

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

        if (!prefabDictionary.TryGetValue(imageName, out GameObject prefab))
        {
            Debug.LogError($"❌ 找不到對應 prefab：{imageName}");
            return;
        }

        if (!spawnedPrefabs.ContainsKey(imageName))
        {
            GameObject newPrefab = Instantiate(prefab, trackedImage.transform.position, trackedImage.transform.rotation);
            newPrefab.transform.SetParent(trackedImage.transform);
            newPrefab.transform.localScale = scaleFactor;
            spawnedPrefabs[imageName] = newPrefab;
            Debug.Log($"🆕 已生成模型：{imageName}");
            // useBtnGameObject.SetActive(true);
        }
    }

    private void UpdateModelTransform(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;
        currentImageName = imageName;
        if (!spawnedPrefabs.TryGetValue(imageName, out var model)) return;

        // 防止 trackingState 沒變也一直重複 SetActive
        if (trackingStates.TryGetValue(imageName, out var lastState) && lastState == trackedImage.trackingState)
        {
            return;
        }

        trackingStates[imageName] = trackedImage.trackingState;

        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            model.SetActive(true);
            // useBtnGameObject.SetActive(true);
            Debug.Log($"✅ {imageName} 已追蹤成功，顯示模型");
        }
        else
        {
            model.SetActive(false);
            // useBtnGameObject.SetActive(false);
            Debug.Log($"⚠️ {imageName} 追蹤中斷，隱藏模型");
        }
    }

    private void SwitchToNextLibrary()
    {
        currentLibraryIndex = (currentLibraryIndex + 1) % imageLibraries.Length;
        trackedImageManager.enabled = false;
        trackedImageManager.referenceLibrary = imageLibraries[currentLibraryIndex];
        trackedImageManager.enabled = true;

        foreach (var obj in spawnedPrefabs.Values)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedPrefabs.Clear();
        trackingStates.Clear();

        print($"🔁 自動切換圖庫：{imageLibraries[currentLibraryIndex].name}");
    }
    
    //按下確認使用並讀取的中的資訊
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
    
    
    public void CleanupARBeforeSceneChange()
    {
        prefabDictionary.Clear();
        
        // 1. 停用追蹤事件
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;

        // 2. 停止 ARSession
        ARSession session = FindAnyObjectByType<ARSession>();
        if (session != null)
        {
            session.enabled = false;
            session.Reset(); // 完全重置 session（選用）
            Debug.Log("🔁 AR Session 已停用並重置");
        }

        // 3. 清除生成的物件
        foreach (var spawned in spawnedPrefabs.Values)
        {
            if (spawned != null) Destroy(spawned);
        }
        spawnedPrefabs.Clear();

        // 4. 停用 XR Origin
        var xrOrigin = FindAnyObjectByType<XROrigin>();
        if (xrOrigin != null)
        {
            xrOrigin.gameObject.SetActive(false);
        }

        Debug.Log("🧹 已清理 AR 場景中的物件與狀態");
    }
}