using System;
using System.Collections;
using System.Collections.Generic;
using Function.Select;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using UI;
namespace Manager
{
    /// <summary>
    /// 管理玩家資料、遊戲流程、教學選擇與結算觸發。
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // 時間歸零時顯示在共用過場面板上的訊息，集中管理避免多處硬編碼。
        private const string TimeUpTransitionMessage = "時間到，遊戲結算 ! ! !";

        // 回到遊戲場景後需要接續倒數，集中保存場景名稱可避免掃描流程分散硬編碼。
        private const string GameSceneName = "Game";

        // 分數勝利與時間到結算使用不同文字，讓玩家能清楚知道本次結束原因。
        private const string ScoreVictoryTransitionMessage = "遊戲結束！！！";
        
        private static GameManager instance;
        private SelectPlayerCount selectPlayerCount;
        private SelectGameUnit selectGameUnit;

        // 記錄設定面板事件是否已訂閱，避免 OnEnable 與動態初始化流程重複綁定同一批事件。
        private bool hasSubscribedSetupEvents;

        // 記錄目前場景中的計時器，讓跨場景保留的 GameManager 可以正確解除舊事件。
        private GameTimer activeGameTimer;

        // 保存進入 AR 掃描前的剩餘時間，因為 GameTimer 會隨 Game 場景卸載而被銷毀。
        private float pausedTimerRemainingSeconds;

        // 標記目前是否因掃描流程暫停倒數，避免一般重新載入 Game 場景時誤啟動計時器。
        private bool isTimerPausedForScan;

        // 標記本局正式倒數是否已開始，讓尚未完成設定或教學的流程不會被場景載入事件啟動。
        private bool hasStartedGameTimer;

        // 時間到流程只能執行一次，避免倒數與其他結算入口重複打開結算面板。
        private bool isTimeUpProcessing;

        // 分數達標時先暫存結算狀態，等 AR 流程回到 Game 場景後再播放過場與顯示結果。
        private bool hasPendingScoreVictory;

        
        //----------------------Data---------------------------
        
        [SerializeField] private List<PlayerData> playersData = new List<PlayerData>();
        [SerializeField] private List<GameData> gameData = new List<GameData>();

        //----------------------勝利條件---------------------------

        [Header("勝利分數設定")]
        // 2 人局的勝利門檻開放給 Inspector，方便現場測試時直接調整平衡。
        [SerializeField, Min(1)] private int twoPlayerVictoryScore = 22;
        // 3 人局獨立設定，避免不同玩家數共用同一數值造成遊戲時間失衡。
        [SerializeField, Min(1)] private int threePlayerVictoryScore = 19;
        // 4 人局通常回合等待較長，因此保留較低預設值並允許設計端微調。
        [SerializeField, Min(1)] private int fourPlayerVictoryScore = 15;

   
        //----------------------Action----------------------------
        // 定義事件，玩家資料生成事件
        public event Action<List<PlayerData>> OnPlayerDataGenerated; 
        
        // 定義事件，更改玩家UI
        public event Action<GameData> ChangePlayerUI;
        
        //傳遞勝利條件達成後的玩家資訊給結算畫面
        public event Action<List<PlayerData>> OnGameVictory;
        
        //---------------------UI 素材----------------------------
        //UI的顯示
        [SerializeField] private GameObject settingPanel;
        [SerializeField] private GameObject techPanel;
        [SerializeField] private Sprite[] playerSprites;
       
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                // 從 AR 或其他場景回到 Game 時，場景內的新 GameManager 不應再生成設定面板，避免覆蓋跨場景保留的遊戲狀態。
                Destroy(gameObject);
                return;
            }

            // 先建立 Singleton 身分，再生成設定 UI，確保重複載入 Game 場景時不會先產生多餘面板。
            instance = this;
            DontDestroyOnLoad(gameObject);

            // 等於零代表目前沒有建立玩家資料，也代表這是新遊戲設定流程的入口。
            if (gameData.Count == 0)
            {
                CreateSettingPanel();
            }
        }

        /// <summary>
        /// 在所有場景物件完成 Awake 後播放遊戲背景音樂。
        /// </summary>
        private void Start()
        {
            if (instance != this)
            {
                // 重複的 GameManager 會在 Awake 被銷毀，不應再觸發任何音樂切換。
                return;
            }

            if (SceneManager.GetActiveScene().name == GameSceneName)
            {
                // 進入 Game 場景後才播放局內背景音樂，開始畫面的音樂會在此被自然替換。
                AudioManager.TryPlayMusic(AudioTrack.Gameplay);
            }
        }

        /// <summary>
        /// 在目前作用中場景的 Canvas 底下建立遊戲設定面板。
        /// </summary>
        private void CreateSettingPanel()
        {
            if (settingPanel == null)
            {
                // 設定面板是新局資料初始化的入口，缺少 Prefab 時必須明確回報，避免後續流程靜默失效。
                Debug.LogError("[GameManager] settingPanel 尚未綁定，無法建立遊戲設定面板。", this);
                return;
            }

            Canvas sceneCanvas = FindActiveSceneCanvas();
            if (sceneCanvas == null)
            {
                // 只允許掛在目前場景 Canvas，避免誤掛到 DontDestroyOnLoad 的轉場 Canvas 後被轉場物件一併銷毀。
                Debug.LogError("[GameManager] 找不到目前場景的 Canvas，無法建立遊戲設定面板。", this);
                return;
            }

            GameObject settingPanelObj = Instantiate(settingPanel, sceneCanvas.transform);
            settingPanelObj.transform.SetAsLastSibling();
            settingPanelObj.SetActive(true);

            // 直接從剛生成的面板抓取元件，並包含 inactive 子物件，避免全域查找選錯場景或漏掉預設關閉的 UI。
            selectPlayerCount = settingPanelObj.GetComponentInChildren<SelectPlayerCount>(true);
            selectGameUnit = settingPanelObj.GetComponentInChildren<SelectGameUnit>(true);

            if (selectPlayerCount == null || selectGameUnit == null)
            {
                // 設定流程需要兩個事件來源都存在，否則人數與教學選擇無法可靠推進。
                Debug.LogError("[GameManager] GameSetting 預製體缺少 SelectPlayerCount 或 SelectGameUnit。", settingPanelObj);
                return;
            }

            SubscribeSetupEvents();
            // 設定面板由 Prefab 動態生成，需在生成後補上共用按鈕音效，避免 GameSetting 內的按鈕漏播回饋。
            AudioManager.TryRegisterButtonsInChildren(settingPanelObj);
            print("Setting Panel Active");
        }

        /// <summary>
        /// 尋找目前作用中場景的 Canvas，排除跨場景保留的轉場 Canvas。
        /// </summary>
        /// <returns>回傳目前作用中場景的 Canvas；若找不到則回傳 null。</returns>
        private Canvas FindActiveSceneCanvas()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (Canvas canvas in canvases)
            {
                if (canvas != null && canvas.gameObject.scene == activeScene)
                {
                    // 只接受 active scene 擁有的 Canvas，避免抓到 DontDestroyOnLoad 場景中的轉場遮罩。
                    return canvas;
                }
            }

            return null;
        }
        

        /// <summary>
        /// 訂閱設定面板事件，讓玩家完成人數與教學選擇後能推進遊戲流程。
        /// </summary>
        private void OnEnable()
        {
            if (instance != this)
            {
                return;
            }

            // GameManager 跨場景保留，必須監聽新 Game 場景載入後重新連接場景內的計時器。
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            SubscribeSetupEvents();
        }

        /// <summary>
        /// 訂閱設定面板事件，並避免同一個 GameManager 重複訂閱。
        /// </summary>
        private void SubscribeSetupEvents()
        {
            if (hasSubscribedSetupEvents)
            {
                return;
            }

            if (instance != this)
            {
                // 重複的 GameManager 會在 Awake 被銷毀，不應再連接設定面板事件。
                return;
            }

            if (selectPlayerCount == null || selectGameUnit == null)
            {
                // 設定流程必須同時取得人數與教學選擇事件來源，避免只訂到一半造成流程狀態不完整。
                return;
            }

            if (selectPlayerCount != null)
            {
                // 設定面板是 Awake 動態產生的 UI，訂閱前保留 null 防護以避免場景綁定異常中斷流程。
                selectPlayerCount.OnPlayerCountConfirmed += GeneratePlayerData;
            }

            if (selectGameUnit != null)
            {
                // 教學選擇與倒數啟動相依，因此只在元件確實存在時建立事件連線。
                selectGameUnit.OnComfirmPlayTech += GameDataGameTechPanel;
            }

            // 兩個事件來源都準備好才標記已訂閱，避免缺件時讓後續補救流程誤以為事件已連線。
            hasSubscribedSetupEvents = true;
        }

        /// <summary>
        /// 解除跨場景事件引用，避免返回主畫面或重新開始遊戲時留下已銷毀物件的訂閱。
        /// </summary>
        private void OnDisable()
        {
            // 物件銷毀或回主選單時解除場景事件，避免舊 GameManager 在新局中恢復已失效的倒數狀態。
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            UnsubscribeSetupEvents();

            if (activeGameTimer != null)
            {
                // GameManager 跨場景保留，離開場景時要解除舊 Timer 事件避免殘留引用。
                activeGameTimer.OnTimerExpired -= HandleGameTimerExpired;
                activeGameTimer = null;
            }
        }

        /// <summary>
        /// 處理 Unity 場景載入完成事件，讓掃描後返回 Game 時可以接續倒數。
        /// </summary>
        /// <param name="scene">剛載入完成的場景，型別為 Scene。</param>
        /// <param name="loadSceneMode">場景載入模式，型別為 LoadSceneMode。</param>
        private void HandleSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.name != GameSceneName)
            {
                return;
            }

            // 分數勝利必須優先於倒數恢復，避免達標後回到 Game 場景又繼續計時或觸發時間到結算。
            if (PlayPendingScoreVictoryTransitionIfNeeded())
            {
                return;
            }

            // 從 AR 掃描回到 Game 且尚未結算時延續局內音樂；同一首播放中時 AudioManager 不會重播。
            AudioManager.TryPlayMusic(AudioTrack.Gameplay);

            // 只有掃描流程暫停過的局才恢復倒數，避免首次進入 Game 時跳過玩家設定流程。
            ResumeTimerAfterScanIfNeeded();
        }

        /// <summary>
        /// 解除設定面板事件訂閱，讓跨場景與回主畫面時的物件銷毀順序保持安全。
        /// </summary>
        private void UnsubscribeSetupEvents()
        {
            if (selectPlayerCount != null)
            {
                // Unity 切場景時 UI 可能已先被銷毀，退訂前檢查可避免重新開始流程被 NullReference 中斷。
                selectPlayerCount.OnPlayerCountConfirmed -= GeneratePlayerData;
            }

            if (selectGameUnit != null)
            {
                // 與人數設定相同，教學選擇事件需要在物件仍有效時才解除。
                selectGameUnit.OnComfirmPlayTech -= GameDataGameTechPanel;
            }

            // 無論事件來源是否已被場景卸載銷毀，都要重置旗標，避免下次新局無法重新訂閱。
            hasSubscribedSetupEvents = false;
        }

                
        private void Update()
        {
            // 當場景變更時，檢查是否回到 "Menu"，如果是就刪除 GameManager
            if (SceneManager.GetActiveScene().name == "Start")
            {
                Destroy(gameObject);
                instance = null; // 確保回到 Menu 時可以重新創建 GameManager
            }
        }
        
        

        
        
        //--------------------- 玩家順序選擇 ----------------------------
        //呼叫有監聽此事件的更新UI或是資料
        public void ChosePlayerBtn(int index)
        {
            // 防呆：尚未初始化 gameData 或 playersData 時直接返回
            if (gameData == null || gameData.Count == 0)
            {
                Debug.LogError("[GameManager] gameData 尚未初始化，請先完成人數設定並生成資料。", this);
                return;
            }
            if (playersData == null || playersData.Count == 0)
            {
                Debug.LogError("[GameManager] playersData 尚未生成。", this);
                return;
            }

            // index 期望是 1-based（1~playerCount）
            if (index < 1 || index > gameData[0].playerCount)
            {
                Debug.LogError($"[GameManager] ChosePlayerBtn index 超出範圍：{index}");
                return;
            }

            // --------------------- 勝條件判斷 ----------------------------
            // //每一回合開始都進行勝利判斷
            // if (GameVictory(gameData[0].currentPlayer - 1))
            // {
            //     FindFirstObjectByType<TransitionManager>().ShowTransition("遊戲結束！！！");
            //     return;
            // }
            
            // --------------------- 顯示跑馬燈 ----------------------------
            FindFirstObjectByType<TransitionManager>().ShowTransition(index + "P 玩家開始");

            // --------------------- 更換為新玩家的資料 ----------------------------
            gameData[0].currentPlayer = index;
            
            //當結束回合按鈕觸發後，就更新玩家UI資訊
            ChangePlayerUI?.Invoke(gameData[0]);
        }

        /// <summary>
        /// 判斷指定玩家是否達到目前玩家人數對應的勝利分數，並標記稍後結算。
        /// </summary>
        /// <param name="index">玩家在 playersData 中的 0-based 索引，型別為 int。</param>
        /// <returns>若玩家已達到勝利分數則回傳 true；否則回傳 false。</returns>
        private bool GameVictory(int index)
        {
            if (gameData == null || gameData.Count == 0)
            {
                // 尚未建立遊戲資料時沒有玩家人數可判定，避免設定流程中誤觸結算。
                return false;
            }

            if (playersData == null || index < 0 || index >= playersData.Count)
            {
                // 分數判定必須對應有效玩家，否則跨場景回來時可能因舊索引造成例外。
                return false;
            }

            var player = playersData[index];
            int victoryScore = GetVictoryScoreThreshold(gameData[0].playerCount);

            if (victoryScore <= 0)
            {
                // 不支援的玩家人數不應套用任一門檻，避免錯誤設定直接判定勝利。
                return false;
            }

            if (player.score >= victoryScore)
            {
                playersData[index].isWin = true;
                hasPendingScoreVictory = true;
                isTimerPausedForScan = false;
                hasStartedGameTimer = false;
                isTimeUpProcessing = true;

                if (activeGameTimer != null && !activeGameTimer.HasExpired)
                {
                    // 分數達標後本局已進入結算流程，先停住倒數避免等待過場時又觸發時間到。
                    activeGameTimer.PauseTimer();
                }

                if (SceneManager.GetActiveScene().name == GameSceneName)
                {
                    // 若分數更新發生在 Game 場景，直接播放過場；AR 場景則等回到 Game 後再處理。
                    PlayPendingScoreVictoryTransitionIfNeeded();
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 依玩家人數取得 Inspector 中設定的勝利分數。
        /// </summary>
        /// <param name="playerCount">本局玩家人數，型別為 int。</param>
        /// <returns>回傳該玩家人數的勝利分數；若人數不支援則回傳 0。</returns>
        private int GetVictoryScoreThreshold(int playerCount)
        {
            switch (playerCount)
            {
                case 2:
                    // 二人局節奏較長，因此保留獨立門檻讓設計端可從 Inspector 微調。
                    return twoPlayerVictoryScore;
                case 3:
                    // 三人局門檻獨立保存，避免未來調平衡時需要改動程式碼。
                    return threePlayerVictoryScore;
                case 4:
                    // 四人局回合等待時間較長，使用較低門檻並允許 Inspector 調整。
                    return fourPlayerVictoryScore;
                default:
                    Debug.LogWarning($"[GameManager] 不支援的玩家人數勝利判定：{playerCount}", this);
                    return 0;
            }
        }

        /// <summary>
        /// 在 Game 場景中播放分數勝利過場，並於動畫結束後開啟結算結果。
        /// </summary>
        /// <returns>若存在待處理的分數勝利流程則回傳 true；否則回傳 false。</returns>
        private bool PlayPendingScoreVictoryTransitionIfNeeded()
        {
            if (!hasPendingScoreVictory)
            {
                return false;
            }

            hasPendingScoreVictory = false;

            TransitionManager transitionManager = FindFirstObjectByType<TransitionManager>();
            if (transitionManager == null)
            {
                // 缺少過場控制器時仍要完成結算，避免玩家達標後卡在無法操作的狀態。
                Debug.LogWarning("[GameManager] 找不到 TransitionManager，將直接進入分數勝利結算畫面。", this);
                TriggerGameVictory();
                return true;
            }

            // 結算畫面等過場完整播放後再開啟，符合「先提示遊戲結束，再顯示結果」的流程。
            transitionManager.ShowTransition(ScoreVictoryTransitionMessage, TriggerGameVictory);
            return true;
        }

        public void TriggerGameVictory()
        {
            // 進入結算後不應再恢復掃描前倒數，否則結果面板可能被時間到流程再次觸發。
            isTimeUpProcessing = true;
            isTimerPausedForScan = false;
            hasStartedGameTimer = false;
            hasPendingScoreVictory = false;

            // 結算階段要隱藏局內背景音樂，因此直接切換成結算音樂。
            AudioManager.TryPlayMusic(AudioTrack.GameResult);

            if (activeGameTimer != null && !activeGameTimer.HasExpired)
            {
                // 手動結算時也要停住倒數，避免背景倒數歸零後再次開啟結算流程。
                activeGameTimer.PauseTimer();
            }

            PlayerUIManager uiManager = FindFirstObjectByType<PlayerUIManager>();
            uiManager.OpenGameResultPanel();
                
            //事件觸發
            OnGameVictory?.Invoke(playersData);
        }

        // --------------------- 玩家資料產生 ----------------------------
        
        //初始化玩家資料
        private void GeneratePlayerData(int playerCount)
        {
            // 重新生成前先清空，避免重複累加
            playersData.Clear();
            gameData.Clear();

            // 新局資料建立時清除上一局跨場景倒數狀態，避免返回 Game 時恢復到舊秒數。
            pausedTimerRemainingSeconds = 0f;
            isTimerPausedForScan = false;
            hasStartedGameTimer = false;
            isTimeUpProcessing = false;
            hasPendingScoreVictory = false;

            // 初始化 GameData：currentPlayer=0 代表尚未選擇任何玩家
            gameData.Add(new GameData(true, playerCount, 0));

            // 防呆：playerSprites 必須有且數量足夠
            if (playerSprites == null || playerSprites.Length < playerCount)
            {
                Debug.LogError($"[GameManager] playerSprites 未設定或數量不足。playerCount={playerCount}, playerSprites.Length={(playerSprites==null?0:playerSprites.Length)}", this);
                return;
            }
            
            for (int i = 1; i <= playerCount; i++)
            {
                string playerName = $"玩家 {i}";
                int initialScore = 0;
                int scanCount = 2;
                playersData.Add(new PlayerData(playerName, playerSprites[i-1],initialScore,scanCount));
            }
            // 通知 UI 更新,UIManager產生相對應的UI
            OnPlayerDataGenerated?.Invoke(playersData);
        }

        
        // --------------------- 玩家資料讀取、存取 ----------------------------
        
        //-----回傳當前遊戲玩家的資料出去-----
        public PlayerData GetPlayerData()
        {
            if (gameData == null || gameData.Count == 0) return null;
            if (gameData[0].currentPlayer <= 0) return null;
            return playersData[gameData[0].currentPlayer-1];
        }
        
        //-----回傳所有玩家的資料，提供UIManager進行使用-----
        public List<PlayerData> GetAllPlayerData()
        {
            return playersData;
        }

        /// <summary>
        /// 更新目前玩家資料，並在分數達標時建立回到 Game 場景後的結算流程。
        /// </summary>
        /// <param name="currentPlayerData">目前玩家資料，型別為 PlayerData。</param>
        public void UpdatePlayerData(PlayerData currentPlayerData)
        {
            if (gameData == null || gameData.Count == 0 || gameData[0].currentPlayer <= 0)
            {
                // currentPlayer 等於 0 代表尚未選擇玩家，此時不得寫入或判定勝利。
                Debug.LogWarning("[GameManager] 尚未選擇目前玩家，無法更新玩家資料。", this);
                return;
            }

            int currentPlayerIndex = gameData[0].currentPlayer - 1;
            if (playersData == null || currentPlayerIndex < 0 || currentPlayerIndex >= playersData.Count)
            {
                // 跨場景流程若資料不同步，先阻止寫入避免破壞玩家清單順序。
                Debug.LogWarning($"[GameManager] 目前玩家索引超出資料範圍：{currentPlayerIndex}", this);
                return;
            }

            // 使用前面驗證過的索引寫回，避免 currentPlayer 在同一方法內被重複換算造成維護風險。
            playersData[currentPlayerIndex] = currentPlayerData;
            GameVictory(currentPlayerIndex);
        }
        //-----更新當前所有的玩家數據-----
        public void UpdateAllPlayerData(List<PlayerData> currentPlayerDatas)
        {
            for (int i = 0; i < playersData.Count; i++)
            {
                playersData[i] = currentPlayerDatas[i];
            }
            // playersData = currentPlayerDatas;
        }
        
        // --------------------- Game Data ----------------------------
        
        /// <summary>
        /// 寫入是否需要教學流程，並依玩家選擇決定倒數計時的開始時機。
        /// </summary>
        /// <param name="isOpen">是否開啟教學面板。</param>
        private void GameDataGameTechPanel(bool isOpen)
        {
            gameData[0].isTech = isOpen;
            //如果需要教學就開啟介紹
            if (isOpen)
            {
                Canvas canvas = FindActiveSceneCanvas();
                if (canvas == null)
                {
                    // 教學面板同樣必須掛在 Game 場景 Canvas，避免被跨場景轉場物件銷毀。
                    Debug.LogError("[GameManager] 找不到目前場景的 Canvas，無法建立教學面板。", this);
                    return;
                }

                GameObject techPanelObj = Instantiate(techPanel,canvas.transform);
                techPanelObj.transform.SetAsLastSibling();
                techPanelObj.SetActive(true);
                // 教學面板同樣是執行期生成的 UI，因此需要在生成後補註冊按鈕音效。
                AudioManager.TryRegisterButtonsInChildren(techPanelObj);
                return;
            }

            // 若玩家未開啟教學，設定完成就代表正式進入遊戲，因此此時才啟動倒數。
            StartGameTimerAfterSetup();
        }

        /// <summary>
        /// 在玩家完成設定或教學後啟動場景中的遊戲倒數計時器。
        /// </summary>
        public void StartGameTimerAfterSetup()
        {
            if (!BindActiveGameTimer())
            {
                return;
            }

            // 正式開始新局倒數時清除掃描暫停旗標，避免舊局狀態干擾下一次設定流程。
            isTimeUpProcessing = false;
            isTimerPausedForScan = false;
            activeGameTimer.StartTimer();
            pausedTimerRemainingSeconds = activeGameTimer.GetRemainingSeconds();
            hasStartedGameTimer = activeGameTimer.HasStarted;
        }

        /// <summary>
        /// 在進入 AR 掃描場景前暫停目前遊戲倒數。
        /// </summary>
        public void PauseGameTimerForScan()
        {
            if (!hasStartedGameTimer || isTimeUpProcessing)
            {
                return;
            }

            if (!BindActiveGameTimer())
            {
                return;
            }

            if (!activeGameTimer.HasStarted || activeGameTimer.HasExpired)
            {
                // 已結束或尚未開始的計時器不應被標記為掃描暫停，避免回場景後重新啟動。
                isTimerPausedForScan = false;
                return;
            }

            // 掃描場景不應消耗局內時間，因此在切場景前先把剩餘秒數交給 GameManager 保存。
            pausedTimerRemainingSeconds = activeGameTimer.PauseTimer();
            isTimerPausedForScan = pausedTimerRemainingSeconds > 0f;
        }

        /// <summary>
        /// 將目前場景中的 GameTimer 綁定到跨場景保留的 GameManager。
        /// </summary>
        /// <returns>若成功找到並綁定 GameTimer 則回傳 true；否則回傳 false。</returns>
        private bool BindActiveGameTimer()
        {
            // 計時器屬於 Game 場景 UI，使用查找可避免 GameManager 跨場景保留後持有失效引用。
            GameTimer gameTimer = FindFirstObjectByType<GameTimer>();
            if (gameTimer == null)
            {
                Debug.LogWarning("[GameManager] 找不到 GameTimer，請確認 Game 場景的 Time 物件已掛載計時器。", this);
                return false;
            }

            if (activeGameTimer != null && activeGameTimer != gameTimer)
            {
                // 重新綁定前先解除舊事件，避免同一個 GameManager 收到多次時間到通知。
                activeGameTimer.OnTimerExpired -= HandleGameTimerExpired;
            }

            activeGameTimer = gameTimer;
            activeGameTimer.OnTimerExpired -= HandleGameTimerExpired;
            activeGameTimer.OnTimerExpired += HandleGameTimerExpired;
            return true;
        }

        /// <summary>
        /// 掃描後返回 Game 場景時，將新場景計時器恢復到離開前的剩餘時間。
        /// </summary>
        private void ResumeTimerAfterScanIfNeeded()
        {
            if (!isTimerPausedForScan || !hasStartedGameTimer || isTimeUpProcessing)
            {
                return;
            }

            if (!BindActiveGameTimer())
            {
                return;
            }

            // 先清除旗標再恢復，避免 ResumeTimer 在零秒邊界觸發事件時被場景載入流程重入。
            isTimerPausedForScan = false;
            activeGameTimer.ResumeTimer(pausedTimerRemainingSeconds);
        }

        /// <summary>
        /// 處理遊戲時間歸零後的結算流程。
        /// </summary>
        private void HandleGameTimerExpired()
        {
            if (isTimeUpProcessing)
            {
                return;
            }

            // 時間到流程只允許進入一次，避免動畫回呼或事件重複觸發造成結算面板重複開啟。
            isTimeUpProcessing = true;

            TransitionManager transitionManager = FindFirstObjectByType<TransitionManager>();
            if (transitionManager == null)
            {
                Debug.LogWarning("[GameManager] 找不到 TransitionManager，將直接進入結算畫面。", this);
                TriggerGameVictory();
                return;
            }

            transitionManager.ShowTransition(TimeUpTransitionMessage, TriggerGameVictory);
        }
        
        //-----獲得遊戲資料-----
        public GameData GetGameData()
        {
            if (gameData.Count != 0)
            {
                return gameData[0];
            }

            return null;
        }
        
        //-----更新遊戲資料的內容-----
        public void UpdateGameData(GameData newGameData)
        {
            gameData[0] = newGameData;
        }
    }
}
