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
    
    // UI控制
    [SerializeField] private GameObject cardAttackView;
    [SerializeField] private GameObject specialCardView;
    [SerializeField] private Text specialCardText;
    
    //轉場
    [SerializeField] SwitchScene switchScenePrefab;
    
    //註冊事件
    public event Action<List<PlayerData>,ModelInfo,int> OnGenerateRemovePlayerUI;
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

   
    
    //卡牌種類判定 拆除卡 建設卡 地域卡
    private void CardState( ModelInfo currentModelInfo )
    {
        
        switch (currentModelInfo.modelType)
        {
            case ModelType.Special: //特殊卡面功能
                UseSpecialCardEffect(currentModelInfo.specialEffect);                
                break;
            case ModelType.Foundation: //獲得點數
                GetFoundationCardScore(currentModelInfo);
                break;
            case ModelType.Building: //建造建築物＆獲得點數
                GetBuildingCardScore(currentModelInfo);
                break;
            case ModelType.Remove: //猜除建築物並扣點
                RemoveBuilding(currentModelInfo);
                break;
            case ModelType.Reduce: //單純扣點
                RemoveBuilding(currentModelInfo);
                break;
            case ModelType.Location:
                RemoveBuilding(currentModelInfo);
                break;
            default:
                print("該模型沒有指派種類");
                break;
        }
    }

    //獲取Special卡牌的效果 進行效果展示
    private void UseSpecialCardEffect(string effectName)
    {
        if (effectName != null)
        {
            specialCardView.SetActive(true);
            specialCardText.text = effectName;
        }
    }
    
    // 獲得建築物的分數,需要再把建築物寫入
    private void GetBuildingCardScore(ModelInfo buildModelInfo)
    {
        Debug.Log($"該模型獲得 {buildModelInfo.score} 分");
                
        //將資料寫入GameManager中
        PlayerData currentPlayerData = gameManager.GetPlayerData();
        currentPlayerData.score += buildModelInfo.score;
        currentPlayerData.AddBuilding(buildModelInfo.colorType,buildModelInfo.modelName);
        gameManager.UpdatePlayerData(currentPlayerData);
                
        GoGameScene();
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
    
    //破壞別人的建築物
    private void RemoveBuilding(ModelInfo buildModelInfo)
    {
        print($"現在玩家為：第{gameManager.GetGameData().currentPlayer}位");
        cardAttackView.SetActive(true);
        //獲取當前玩家是誰
        int currentPlayer = gameManager.GetGameData().currentPlayer;
        //獲取玩家資訊並回傳
        OnGenerateRemovePlayerUI?.Invoke(gameManager.GetAllPlayerData(),buildModelInfo,currentPlayer);
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
        if (scanData.scanCount < 2)
        {
            if (scanData.scanCount == 1)
                scanData.isScan = false;
            scanData.scanCount++;
        }
        else
        {
            scanData.scanCount = 2;
        }
        gameManager.UpdateGameData(scanData);
        GoGameScene();
    }
    
    
}
