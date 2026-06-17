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
        
        private static GameManager instance;
        private SelectPlayerCount selectPlayerCount;
        private SelectGameUnit selectGameUnit;

        // 記錄設定面板事件是否已訂閱，避免 OnEnable 與動態初始化流程重複綁定同一批事件。
        private bool hasSubscribedSetupEvents;

        // 記錄目前場景中的計時器，讓跨場景保留的 GameManager 可以正確解除舊事件。
        private GameTimer activeGameTimer;

        // 時間到流程只能執行一次，避免倒數與其他結算入口重複打開結算面板。
        private bool isTimeUpProcessing;

        
        //----------------------Data---------------------------
        
        [SerializeField] private List<PlayerData> playersData = new List<PlayerData>();
        [SerializeField] private List<GameData> gameData = new List<GameData>();

   
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
            UnsubscribeSetupEvents();

            if (activeGameTimer != null)
            {
                // GameManager 跨場景保留，離開場景時要解除舊 Timer 事件避免殘留引用。
                activeGameTimer.OnTimerExpired -= HandleGameTimerExpired;
                activeGameTimer = null;
            }
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

        // --------------------- 勝條件判斷 ----------------------------
        //遊戲勝利判定
        //將會在玩家按下結束回合時進行勝利判斷，然後顯示遊戲最終成績
        private bool GameVictory(int index)
        {
            var player = playersData[index];
            //寫入勝利條件
            if (player.score >= 30 )
            {
                playersData[index].score = 30;
                playersData[index].isWin = true;
                TriggerGameVictory();

                return true;
            }

            return false;
        }

        public void TriggerGameVictory()
        {
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

        //-----更新當前的玩家數據-----
        public void UpdatePlayerData(PlayerData currentPlayerData)
        {
            playersData[gameData[0].currentPlayer-1] = currentPlayerData;
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
            // 計時器屬於 Game 場景 UI，使用查找可避免 GameManager 跨場景保留後持有失效引用。
            GameTimer gameTimer = FindFirstObjectByType<GameTimer>();
            if (gameTimer == null)
            {
                Debug.LogWarning("[GameManager] 找不到 GameTimer，請確認 Game 場景的 Time 物件已掛載計時器。", this);
                return;
            }

            if (activeGameTimer != null)
            {
                // 重新綁定前先解除舊事件，避免同一個 GameManager 收到多次時間到通知。
                activeGameTimer.OnTimerExpired -= HandleGameTimerExpired;
            }

            activeGameTimer = gameTimer;
            activeGameTimer.OnTimerExpired -= HandleGameTimerExpired;
            activeGameTimer.OnTimerExpired += HandleGameTimerExpired;
            isTimeUpProcessing = false;
            gameTimer.StartTimer();
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
