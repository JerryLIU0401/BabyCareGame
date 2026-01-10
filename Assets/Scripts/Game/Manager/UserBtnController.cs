using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Manager
{
    //用來控制玩家遊玩時的按鈕設定，包含掃描卡片以及丟棄卡片所需要執行的內容

    public class UserBtnController : MonoBehaviour
    {
        private GameManager gameManager;
        
        private bool isScan = false;
        [SerializeField] private GameObject dropCardPanel;
        [SerializeField] private Image dropBtnImage;
        [SerializeField] private Sprite dropBtnDefaultImage;
        [SerializeField] private Sprite dropBtnUseImage;
        //[SerializeField] private TMP_Text scanText;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button dropButton;
        [SerializeField] private Button resultBtn;
        [SerializeField] private Button scanButton;
        
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
                if (nextButton != null)
                {
                    nextButton.onClick.RemoveAllListeners(); // 先清除舊的綁定
                    // nextButton.onClick.AddListener(gameManager.NextPlayerBtn); // 重新綁定
                }
                if (dropButton != null)
                {
                    dropButton.onClick.RemoveAllListeners(); // 先清除舊的綁定
                    // dropButton.onClick.AddListener(gameManager.NextPlayerBtn); // 重新綁定
                }

                if (resultBtn != null)
                {
                    resultBtn.onClick.RemoveAllListeners(); // 先清除舊的綁定
                    resultBtn.onClick.AddListener(gameManager.TriggerGameVictory); // 重新綁定
                }
            }
            
        }
        
        public void DropCardBtn()
        {
            if (!isScan)
            {
                dropCardPanel.SetActive(true);
            }
        }
        
        //掃卡片按鈕設定
        public void ScanCardBtn()
        {
            GameData btnGameData = gameManager.GetGameData();

            scanButton.GetComponent<Button>().enabled = false;
            print("掃描卡片");
            gameManager.UpdateGameData(btnGameData);
            GoARScene();
        }
     
        
        //移動到AR場景
        private void GoARScene()
        {
            SwitchScene switchScenes = Instantiate(switchScenePrefab);
            switchScenes.StartCoroutine(switchScenes.loadFadeOutInScenes("ARImageTrackingScene"));
        }
    }
}
