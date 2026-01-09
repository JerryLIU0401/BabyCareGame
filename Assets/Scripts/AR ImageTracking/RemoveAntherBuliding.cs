using System;
using System.Collections;
using System.Collections.Generic;
using CardModel;
using Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//用來破壞敵人的建築物，並扣除5點環保值
public class RemoveAntherBuliding : MonoBehaviour
{
    private ARCardManager cardManager;
    private GameManager gameManager;

    [SerializeField] private Image currentPlayerImage;
    
    // 玩家 UI 預製體
    [SerializeField] private GameObject playerUIPrefab;
    
    // 放置 UI 的父物件
    [SerializeField] private Transform uiParent;
    
    // 存放玩家背景顏色
    [SerializeField] private Sprite[] cardBgSprites;
    // 存放玩家頭像的陣列
    [SerializeField] private Sprite[] playerSprites;
    private List<GameObject> playerUIObjects = new List<GameObject>();
    
    //UI顯示
    [SerializeField] private GameObject againView;

    [SerializeField] private GameObject removeBuildingView;

    [SerializeField] private GameObject locationView;
    [SerializeField] private TMP_Text useOneCardText;
    [SerializeField] private TMP_Text useTwoCardText;
    
    //轉場
    [SerializeField] SwitchScene switchScenePrefab;
    
    //該建築物的分數
    private int removeScore;
    private BuildingColor buildingColor;
    private string buildingName;
    private bool useLocationCard = false;
    private int currentPlayerIndex;
    private int btnTarget;
    
    //事件註冊
    public event Action<int,int,BuildingColor,List<PlayerData.BuildingInfo>> OnGeneratePlayerBulidingList; 
    public event Action<Transform,List<bool>,List<int>> OnDisplayBuildCount; 
    private void Awake()
    {
        cardManager = FindFirstObjectByType<ARCardManager>();
        gameManager = FindFirstObjectByType<GameManager>();
    }

    private void OnEnable()
    {
        cardManager.OnGenerateRemovePlayerUI += GenerateUI;
    }

    private void OnDisable()
    {
        cardManager.OnGenerateRemovePlayerUI -= GenerateUI;
    }

    //選擇攻擊的玩家後的執行 還要消除那個顏色建築物的其中一個，自由選擇
    public void AttackPlayerChoose(int targetPlayerIndex)
    {
        
        List<PlayerData> targetPlayerDatas = gameManager.GetAllPlayerData();
        
        //有這個顏色的話就扣除相對應的分數並移除建築物
        if (targetPlayerDatas[targetPlayerIndex].HasBuildingOfColor(buildingColor))
        {
           
            removeBuildingView.SetActive(true);
            List<PlayerData.BuildingInfo> targetBuildinglist = targetPlayerDatas[targetPlayerIndex].GetBuildingColorList(buildingColor);
            OnGeneratePlayerBulidingList?.Invoke(targetPlayerIndex,removeScore,buildingColor,targetBuildinglist);
        }
        else
        {
            //沒有就讓玩家在選擇一次
            againView.SetActive(true);
        }
    }
    
    //扣相對應的模型點數
    public void ReducePlayerScore(int targetPlayerIndex)
    {
        List<PlayerData> targetPlayerDatas = gameManager.GetAllPlayerData();
        targetPlayerDatas[targetPlayerIndex].score -= removeScore;
            
        //避免分數變成負數
        if (targetPlayerDatas[targetPlayerIndex].score < 0)
        {
            targetPlayerDatas[targetPlayerIndex].score = 0;
        }
        gameManager.UpdateAllPlayerData(targetPlayerDatas);
            
        GoGameScene();
    }
    
    //根據地域卡扣相對應的模型點數,點數要有正負值
    public void LocationReducePlayerScore(int targetPlayerIndex,int currentIndex)
    {
        List<PlayerData> targetPlayerDatas = gameManager.GetAllPlayerData();

        if (useLocationCard)
        {
            removeScore = (int)((float)removeScore * 2.5f);
        }

        if (removeScore > 0)
        {
            targetPlayerDatas[currentIndex].score += removeScore;
        }
        else
        {
            print($"攻擊 {targetPlayerIndex} 玩家");
            targetPlayerDatas[targetPlayerIndex].score += removeScore;
            //避免分數變成負數
            if (targetPlayerDatas[targetPlayerIndex].score < 0)
            {
                targetPlayerDatas[targetPlayerIndex].score = 0;
            }
        }
        
        gameManager.UpdateAllPlayerData(targetPlayerDatas);
            
        GoGameScene();
    }
    
    /// <summary>
    /// 用來生成要攻擊的玩家UI
    /// </summary>
    /// <param name="playerDatas"> 總玩家資訊 </param>
    /// <param name="currentModelInfo"> 該模型的資訊 </param>
    /// <param name="currentPlayer"> 當前玩家的順序 </param>
    private void GenerateUI(List<PlayerData> playerDatas,ModelInfo currentModelInfo,int currentPlayer)
    {
        //紀錄當前建築物的分數
        removeScore = currentModelInfo.score;
        buildingColor = currentModelInfo.colorType;
        buildingName = currentModelInfo.modelName;
        // 清除現有的 UI
        foreach (Transform child in uiParent)
        {
            Destroy(child.gameObject);
        }
        playerUIObjects.Clear();


        for (int i = 0; i < playerDatas.Count; i++)
        {
            //避免生成出與當前玩家一樣的Obj,因為只攻擊別人
            if (i == currentPlayer - 1)
            {
                print($"跳過{currentPlayer}位");
                
                //紀錄當前玩家的值
                btnTarget = i;
                
                currentPlayerImage.sprite = playerSprites[i];
                continue;
            }
            
            int target = i;
            
            GameObject playerUI = Instantiate(playerUIPrefab, uiParent);
            playerUIObjects.Add(playerUI);

            var bg  = playerUI.GetComponent<Image>();
            
            // 設定玩家頭像
            var playerIcon = playerUI.transform.Find("Icon").GetComponentInChildren<Image>();
            
            //將每一位玩家的分數印行更新＆顯示
            var playerScoeText = playerUI.transform.Find("Point").GetComponentInChildren<TMP_Text>();
            
            var attackButton = playerUI.GetComponent<Button>(); // 找到按鈕
            
            var buildParent = playerUI.transform.Find("House");
            
            if (playerIcon != null)
            {
                if (playerSprites.Length > 0)
                {
                    playerIcon.sprite = playerSprites[i]; // 確保不超出陣列範圍
                }

                if (cardBgSprites.Length > 0)
                {
                    bg.sprite = cardBgSprites[i];
                }
            }

            if (playerScoeText != null)
            {
                playerScoeText.text = $"{playerDatas[i].score} 點環保值";
            }

            if (attackButton != null)
            {
                
                // 需要用區域變數，避免閉包 (closure) 問題
                if (currentModelInfo.modelType == ModelType.Reduce)
                {
                    attackButton.onClick.AddListener(() => ReducePlayerScore(target));
                }
                else if (currentModelInfo.modelType == ModelType.Location)
                {
                    print(target);
                    locationView.SetActive(true);
                    if (removeScore > 0)
                    {
                        useOneCardText.text = "環保值 +4";
                        useTwoCardText.text = "環保值 +10";
                    }
                    else
                    {
                        useOneCardText.text = "環保值 -4";
                        useTwoCardText.text = "環保值 -10";
                    }
                    currentPlayerIndex = currentPlayer - 1;
                    attackButton.onClick.AddListener(() => LocationReducePlayerScore(target,currentPlayerIndex));
                
                }
                else
                {
                    attackButton.onClick.AddListener(() => AttackPlayerChoose(target));
                }
            }

            List<bool> isHaveBuild = playerDatas[i].GetBuildingColorStateList();
            List<int> isCountBuild = playerDatas[i].GetColorBuildingsCountList();
            OnDisplayBuildCount?.Invoke(buildParent,isHaveBuild,isCountBuild);
        }
    }

    public void UseOneLocationCardBtn()
    {
        useLocationCard = false;
        if (removeScore > 0)
        {
            LocationReducePlayerScore(btnTarget,currentPlayerIndex);
        }
    }

    public void UseTwoLocationCardBtn()
    {
        useLocationCard = true;
        if (removeScore > 0)
        {
            LocationReducePlayerScore(btnTarget,currentPlayerIndex);
        }
    }
    
    
    //移動到AR場景
    private void GoGameScene()
    {
        ImageTrackingController trackedImageManager = FindFirstObjectByType<ImageTrackingController>();
        trackedImageManager.CleanupARBeforeSceneChange();
        SwitchScene switchScenes = Instantiate(switchScenePrefab);
        switchScenes.StartCoroutine(switchScenes.loadFadeOutInScenes("Game"));
    }
}
