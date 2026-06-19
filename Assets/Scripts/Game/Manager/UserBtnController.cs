using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Manager
{
    /// <summary>
    /// 控制遊戲中玩家操作按鈕，包含轉盤、掃描卡片與結算入口。
    /// </summary>
    public class UserBtnController : MonoBehaviour
    {
        private GameManager gameManager;
        
        // private bool isScan = false;
        // [SerializeField] private GameObject dropCardPanel;
        // [SerializeField] private Image spinBtnImage;
        // [SerializeField] private Sprite dropBtnDefaultImage;
        // [SerializeField] private Sprite dropBtnUseImage;
        //[SerializeField] private TMP_Text scanText;
        //[SerializeField] private Button nextButton;

        [SerializeField] private Button spinPanelButton;
        [SerializeField] private Button resultBtn;
        [SerializeField] private Button scanButton;
        
        [SerializeField] private GameObject spinPanel;
        
        //轉場
        [SerializeField] SwitchScene switchScenePrefab;
        private void Awake()
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
        
        

        private void Start()
        {
            if (gameManager != null)
            {
                if (resultBtn != null)
                {
                    resultBtn.onClick.RemoveAllListeners(); // 先清除舊的綁定
                    resultBtn.onClick.AddListener(gameManager.TriggerGameVictory); // 重新綁定
                }
            }
            
        }
        
        // public void DropCardBtn()
        // {
        //     if (!isScan)
        //     {
        //         dropCardPanel.SetActive(true);
        //     }
        // }
        
        
        //轉盤設定
        public void OpenSpin()
        {
            GameData btnGameData = gameManager.GetGameData();
            
            //避免沒有選人造成錯誤
            if (btnGameData.currentPlayer == 0)
            {
                return;
            }
           
            spinPanel.SetActive(true);
            print("開啟轉盤");
        }
        
        /// <summary>
        /// 處理掃描卡片按鈕，並在切換 AR 場景前暫停遊戲倒數。
        /// </summary>
        public void ScanCardBtn()
        {
            GameData btnGameData = gameManager.GetGameData();
            
            //避免沒有選人造成錯誤
            if (btnGameData.currentPlayer == 0)
            {
                return;
            }
            
            scanButton.GetComponent<Button>().enabled = false;
            print("掃描卡片");
            gameManager.UpdateGameData(btnGameData);
            // 掃描期間不應扣除局內時間，因此切場景前先由 GameManager 保存目前倒數狀態。
            gameManager.PauseGameTimerForScan();
            GoARScene();
        }
     
        
        /// <summary>
        /// 載入 AR 掃描場景。
        /// </summary>
        private void GoARScene()
        {
            // 透過共用轉場流程進入 AR，讓返回 Game 時仍可由 GameManager 接續倒數狀態。
            SwitchScene switchScenes = Instantiate(switchScenePrefab);
            switchScenes.StartCoroutine(switchScenes.loadFadeOutInScenes("ARImageTrackingScene"));
        }
    }
}
