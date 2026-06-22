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
    /// <summary>
    /// 接收玩家確認使用的卡牌資料，並依卡牌類型套用對應遊戲規則。
    /// </summary>
    /// <param name="currentModelInfo">目前被玩家確認使用的卡牌模型資訊，型別為 ModelInfo。</param>
    private void CardState(ModelInfo currentModelInfo)
    {
        if (currentModelInfo == null)
        {
            // 使用按鈕理論上只會在已有模型時出現，保留防護可避免 Prefab 缺 ModelInfo 時中斷 AR 流程。
            Debug.LogWarning("[ARCardManager] 收到空的卡牌資料，略過使用流程。", this);
            return;
        }

        // 確認使用音效由跨場景 AudioManager 播放，返回 Game 場景時仍能完整播完。
        AudioManager.TryPlaySoundEffect(SoundEffect.Score);
        
        switch (currentModelInfo.modelType)
        {
            //獲得分數，並播放動畫
            case ModelType.Foundation: 
                GetFoundationCardScore(currentModelInfo);
                break;
        }
    }
    
    
    /// <summary>
    /// 依基礎分數卡更新目前玩家分數，並啟動返回 Game 場景流程。
    /// </summary>
    /// <param name="buildModelInfo">被玩家確認使用的分數卡資料，型別為 ModelInfo。</param>
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
