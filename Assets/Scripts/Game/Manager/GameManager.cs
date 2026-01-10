using System;
using System.Collections;
using System.Collections.Generic;
using Function.Select;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Manager
{
    //玩家資料產生 以及遊戲相關設定
    public class GameManager : MonoBehaviour
    {
        
        private static GameManager instance;
        private SelectPlayerCount selectPlayerCount;
        private SelectGameUnit selectGameUnit;

        
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
        [SerializeField] private Sprite[] playerSprites;
        private void Awake()
        {
            // //等於零代表目前沒有建立資料 也代表沒有遊戲設定
            if (gameData.Count == 0 )
            {
                Canvas canvas = FindFirstObjectByType<Canvas>();
                GameObject settingPanelObj = Instantiate(settingPanel,canvas.transform);
                settingPanelObj.transform.SetAsLastSibling();
                settingPanelObj.SetActive(true);
                print("Setting Panel Active");
            }
            
            selectPlayerCount = FindFirstObjectByType<SelectPlayerCount>();
            selectGameUnit = FindFirstObjectByType<SelectGameUnit>();

           
            
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        

        private void OnEnable()
        {
            selectPlayerCount.OnPlayerCountConfirmed += GeneratePlayerData;
            selectGameUnit.OnComfirmPlayTech += WriteGameDataGameTechPanel;
        }

        private void OnDisable()
        {
            selectPlayerCount.OnPlayerCountConfirmed -= GeneratePlayerData;
            selectGameUnit.OnComfirmPlayTech -= WriteGameDataGameTechPanel;
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
        
        //-----寫入是否需要教學關卡-----
        private void WriteGameDataGameTechPanel(bool isOpen)
        {
            gameData[0].isTech = isOpen;
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