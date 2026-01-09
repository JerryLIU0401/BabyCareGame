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
        
        //預設掃描次數
        [SerializeField] private int initialScanCount = 2;
        
        
        [SerializeField] private List<PlayerData> playersData = new List<PlayerData>();
        [SerializeField] private List<GameData> gameData = new List<GameData>();

        private SelectPlayerCount selectPlayerCount;
        private SelectGameUnit selectGameUnit;
        
        // 定義事件，玩家資料生成事件
        public event Action<List<PlayerData>> OnPlayerDataGenerated; 
        
        // 定義事件，更改玩家UI
        public event Action<GameData> ChangePlayerUI;
        
        //傳遞掃描的次數
        public event Action<int> UpdateScanCount ; 
        
        //傳遞勝利條件達成後的玩家資訊給結算畫面
        public event Action<List<PlayerData>> OnGameVictory;
        
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
            selectGameUnit.OnComfirmPlayUnit += WriteGameDataGameUnit;
        }

        private void OnDisable()
        {
            selectPlayerCount.OnPlayerCountConfirmed -= GeneratePlayerData;
            selectGameUnit.OnComfirmPlayUnit -= WriteGameDataGameUnit;
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
        
        //下一回合開始，並呼叫有監聽此事件的更新UI或是資料
        public void NextPlayerBtn()
        {
            //先更新玩家有的建築顏色判斷，在進行勝利判斷
            foreach (PlayerData player in playersData)
            {
                player.ColorBuildingState();
            }
            
            //每一回合開始都進行勝利判斷
            if (GameVictory(gameData[0].currentPlayer - 1))
            {
                FindFirstObjectByType<TransitionManager>().ShowTransition("遊戲結束！！！");
                return;
            }
            
            FindFirstObjectByType<TransitionManager>().ShowTransition("下一位玩家開始");
            
            //紀錄當前玩家順序
            if (gameData[0].currentPlayer >= gameData[0].playerCount)
            {
                gameData[0].currentPlayer = 1;
            }
            else
            {
                gameData[0].currentPlayer++;
            }

            gameData[0].scanCount = initialScanCount;
            gameData[0].isScan = false;
            //每回合可以掃描的次數
            UpdateScanCount?.Invoke(initialScanCount);
            
            //當結束回合按鈕觸發後，就更新玩家UI資訊
            ChangePlayerUI?.Invoke(gameData[0]);
        }

        
        //遊戲勝利判定
        //將會在玩家按下結束回合時進行勝利判斷，然後顯示遊戲最終成績
        private bool GameVictory(int index)
        {
            var player = playersData[index];
            //勝利條件為 分數 > 30 || 擁有三種不同的的建築物
            if (player.score >= 30 || player.HasThreeOrMoreBuildings())
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

        //－－－－－－PlayerDate資料－－－－－－－
        
        //初始化玩家資料
        private void GeneratePlayerData(int playerCount)
        {
            
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

        //回傳當前遊戲玩家的資料出去
        public PlayerData GetPlayerData()
        {
            //因為currentPlayer的範圍是 1 到 4 所以在傳遞玩家資料時要減一
            return playersData[gameData[0].currentPlayer-1];
        }
        
        //回傳所有玩家的資料，提供UIManager進行使用
        public List<PlayerData> GetAllPlayerData()
        {
            return playersData;
        }

        //更新當前的玩家數據
        public void UpdatePlayerData(PlayerData currentPlayerData)
        {
            playersData[gameData[0].currentPlayer-1] = currentPlayerData;
        }
        //更新當前所有的玩家數據
        public void UpdateAllPlayerData(List<PlayerData> currentPlayerDatas)
        {
            for (int i = 0; i < playersData.Count; i++)
            {
                playersData[i] = currentPlayerDatas[i];
            }
            // playersData = currentPlayerDatas;
        }
        
        //－－－－－－GameDate資料－－－－－－－
        
        //利用監聽事件，將選擇的遊戲單元寫入資料中
        private void WriteGameDataGameUnit(String unit)
        {
            gameData[0].unitName = unit;
        }
        
        //用來儲存和初始化遊戲數據
        public void InitialGameData(int startPlayer,int playerCount)
        {
            gameData.Add(new GameData(true,playerCount,startPlayer,initialScanCount,initialScanCount));

        }
        
        //獲得遊戲資料
        public GameData GetGameData()
        {
            if (gameData.Count != 0)
            {
                return gameData[0];
            }

            return null;
        }
        
        //更新遊戲資料的內容
        public void UpdateGameData(GameData newGameData)
        {
            gameData[0] = newGameData;
        }
    }
}