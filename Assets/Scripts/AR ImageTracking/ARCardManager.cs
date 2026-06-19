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
    // 防止玩家連點使用卡牌時重複啟動清理與切場景流程，避免 AR Simulation 在同一段生命週期被多次停用。
    private bool isChangingScene;
    
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
    
    
    /// <summary>
    /// 啟動返回 Game 場景流程。
    /// </summary>
    public void GoGameScene()
    {
        if (isChangingScene)
        {
            return;
        }

        StartCoroutine(GoGameSceneRoutine());
    }

    /// <summary>
    /// 分幀清理 AR 追蹤狀態後再切回 Game 場景。
    /// </summary>
    /// <returns>等待 AR 清理與場景轉場完成的協程列舉器。</returns>
    private IEnumerator GoGameSceneRoutine()
    {
        isChangingScene = true;

        if (imageTrackingController != null)
        {
            // XR Simulation 會在背景更新追蹤品質，因此需要等待控制器完成安全離場清理再卸載場景。
            yield return imageTrackingController.CleanupARBeforeSceneChangeRoutine();
        }
        else
        {
            // 缺少追蹤控制器時仍允許回到 Game，避免測試場景因 Inspector 綁定遺失卡死在 AR 場景。
            Debug.LogWarning("[ARCardManager] 找不到 ImageTrackingController，將略過 AR 清理並直接返回 Game。", this);
        }

        if (switchScenePrefab == null)
        {
            // 轉場 Prefab 是離場流程的最後一步，缺少時明確回報可避免誤判為計分失敗。
            Debug.LogError("[ARCardManager] switchScenePrefab 尚未綁定，無法返回 Game 場景。", this);
            isChangingScene = false;
            yield break;
        }

        SwitchScene switchScenes = Instantiate(switchScenePrefab);
        yield return switchScenes.StartCoroutine(switchScenes.loadFadeOutInScenes("Game"));
    }

    public void BackGameScene()
    {
        GameData scanData = gameManager.GetGameData();
        gameManager.UpdateGameData(scanData);
        GoGameScene();
    }
    
    
}
