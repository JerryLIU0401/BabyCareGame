using System;
using System.Collections;
using System.Collections.Generic;
using CardModel;
using Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ARCardManager : MonoBehaviour
{
    private GameManager gameManager;
    private ImageTrackingController imageTrackingController;
    
    // ----------------- UI控制 -----------------
    [SerializeField] private GameObject specialCardView;
    [SerializeField] private Text specialCardText;
    
    // ----------------- 轉場 -----------------
    [SerializeField] SwitchScene switchScenePrefab;
    
    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        imageTrackingController = FindFirstObjectByType<ImageTrackingController>();
    }

    private void OnEnable()
    {
        imageTrackingController.OnUseCardAction += CardState;
    }

    private void OnDisable()
    {
        imageTrackingController.OnUseCardAction -= CardState;
    }

   
    
    //-------------------卡牌辨識-----------------------
    private void CardState( ModelInfo currentModelInfo )
    {
        
        switch (currentModelInfo.modelType)
        {
            //獲得分數，並播放動畫
            case ModelType.Foundation: 
                GetFoundationCardScore(currentModelInfo);
                break;
        }
    }
    
    
    // 直接獲得點數卡的點數
    private void GetFoundationCardScore(ModelInfo buildModelInfo)
    {
        Debug.Log($"該點數卡獲得 {buildModelInfo.score} 分");
                
        //將資料寫入GameManager中
        PlayerData currentPlayerData = gameManager.GetPlayerData();
        currentPlayerData.score += buildModelInfo.score;
        gameManager.UpdatePlayerData(currentPlayerData);
                
        GoGameScene();
    }
    
    
    //移動到Game場景
    public void GoGameScene()
    {
        imageTrackingController.CleanupARBeforeSceneChange();
        SwitchScene switchScenes = Instantiate(switchScenePrefab);
        switchScenes.StartCoroutine(switchScenes.loadFadeOutInScenes("Game"));
    }

    public void BackGameScene()
    {
        GameData scanData = gameManager.GetGameData();
        gameManager.UpdateGameData(scanData);
        GoGameScene();
    }
    
    
}
