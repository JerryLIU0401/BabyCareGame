using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using Function.Select;
using Player;
using TMPro;
using UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

//-------未來再修改寫法,讓整體變得更漂亮更好維護-------

//根據選擇的玩家數量來生成相對應的玩家區塊並調整相對應的間距
//所有的UI都放在這裡，避免我轉場回來之後資料都不見
namespace Manager
{
    public class PlayerUIManager : MonoBehaviour
    {
        
        private PlayerGridLayoutAdjust playerGridLayoutAdjust;
        private GameManager gameManager;
        
        //紀錄當前遊玩玩家的順序
        private int currentPlayerCount;
        
        // --------------------- UI 素材 ----------------------------
        // 玩家 UI 預製體
        [SerializeField] private GameObject playerUIPrefab;
        // 放置 UI 的父物件
        [SerializeField] private Transform uiParent;
        
        private List<GameObject> playerUIObjects = new List<GameObject>();
        
        //玩家背景顏色
        [SerializeField] private Sprite[] bgSprites;
        
        // 存放玩家頭像的陣列
        [SerializeField] private Sprite[] playerSprites;
      
        //UI的顯示
        [SerializeField] private GameObject gameOverPanel;
        
        
        
        private void Awake()
        {
            gameManager = FindFirstObjectByType<GameManager>();
            playerGridLayoutAdjust = GetComponent<PlayerGridLayoutAdjust>();
        }

        private void Start()
        {
            List<PlayerData> playerData = gameManager.GetAllPlayerData();
            
            //轉場回來執行資料讀取
            if (playerData.Count != 0)
            {
                GameData gameData = gameManager.GetGameData();
                PlayerController player = FindFirstObjectByType<PlayerController>();
                player.EditPlayerScoreData(gameManager.GetPlayerData().score);
                GeneratePlayerUI(playerData);
                DisplayCurrentPlayer(gameData);
            }
        }

        private void OnEnable()
        {
            // 下一位玩家啟動要改變當前資訊
            gameManager.ChangePlayerUI += DisplayCurrentPlayer;
            // 基本資料生成完成，要產生相對應 UI
            gameManager.OnPlayerDataGenerated += GeneratePlayerUI;
        }

        private void OnDisable()
        {
            gameManager.OnPlayerDataGenerated -= GeneratePlayerUI;
            gameManager.ChangePlayerUI -= DisplayCurrentPlayer;
        }
        
        // --------------------- 生成玩家 UI ----------------------------
        private void GeneratePlayerUI(List<PlayerData> players)
        {
            // 清除現有的 UI
            foreach (Transform child in uiParent)
            {
                Destroy(child.gameObject);
            }
            playerUIObjects.Clear();
            

            for (int i = 0; i < players.Count; i++)
            {
                GameObject playerUI = Instantiate(playerUIPrefab, uiParent);
                playerUIObjects.Add(playerUI);

                // 設定玩家背景
                 var bg = playerUI.transform.Find("DispalyUI").transform.Find("BG").GetComponent<Image>();
                
                // 設定玩家頭像
                var playerIcon = playerUI.transform.Find("DispalyUI").transform.Find("PlayerIcon").GetComponentInChildren<Image>();
                
                // 將每一位玩家的分數印行更新＆顯示
                var playerScoeText = playerUI.transform.Find("DispalyUI").GetComponentInChildren<TMP_Text>();
                
                // 綁定頭像按鈕：點擊後切換到該玩家（index 使用 1-based）
                var playerBtn = playerUI.GetComponent<Button>();
                
                int playerIndex = i + 1; // 1-based
                
                if (playerBtn != null)
                {
                    playerBtn.onClick.RemoveAllListeners();
                    playerBtn.onClick.AddListener(() => gameManager.ChosePlayerBtn(playerIndex));
                }
                else
                {
                    Debug.LogWarning("[PlayerUIManager] DispalyUI/PlayerIcon 缺少 Button 元件，無法點擊切換玩家。", playerUI);
                }
                
                // 確認玩家頭像顯示正確
                if (playerIcon != null && playerSprites.Length > 0)
                {
                    bg.sprite = bgSprites[i % bgSprites.Length];
                    playerIcon.sprite = playerSprites[i % playerSprites.Length]; // 確保不超出陣列範圍
                }
                
                // 顯示該玩家分數
                if (playerScoeText != null)
                {
                    playerScoeText.text = $"{players[i].score}分";
                }
            }
        }

        // --------------------- 更換玩家調整 UI 介面 ----------------------------
        private void DisplayCurrentPlayer(GameData gameData)
        {
            for (int i = 1; i <= playerUIObjects.Count; i++)
            {
                var currentPlayer = gameManager.GetAllPlayerData()[i - 1];
                var playerObject = playerUIObjects[i - 1];
                
                //進行當前玩家的UI顯示
                var displayObject = playerObject.transform.Find("DispalyUI").gameObject;
                var currentObject = playerObject.transform.Find("CurrentPlayer").gameObject;

                //更新玩家的分數顯示
                var playerScoeText = playerObject.transform.Find("DispalyUI").GetComponentInChildren<TMP_Text>();
                
                if (i == gameData.currentPlayer)
                {
                    displayObject.SetActive(true);
                    currentObject.SetActive(true);
                    
                    //調整文字內容
                    var currentText = currentObject.GetComponentInChildren<Text>();
                    if (currentText != null)
                    {
                        currentText.text = $"{gameData.currentPlayer}P的回合";
                    }
                }
                else
                {
                    displayObject.SetActive(true);
                    currentObject.SetActive(false);
                    if (playerScoeText != null)
                    {
                        playerScoeText.text = $"{currentPlayer.score}分";
                    }
                }
            }
        }

        public void OpenGameResultPanel()
        {
            gameOverPanel.SetActive(true);
        }
    }
}
