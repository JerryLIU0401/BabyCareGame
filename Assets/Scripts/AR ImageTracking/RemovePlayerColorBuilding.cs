using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

//用來顯示玩家的建築物名稱列表和選擇
public class RemovePlayerColorBuilding : MonoBehaviour
{
    private RemoveAntherBuliding removeAntherBuliding;

    private GameManager gameManager;

    private GameObject currentObject;
    
    [SerializeField] private Sprite[] blockSprites;
    private Sprite currentBlockSprite;

    private int removeScore;
    //轉場
    [SerializeField] SwitchScene switchScenePrefab;
    
    // 玩家建築名稱預製體
    [SerializeField] private GameObject buildListUIPrefab;

    // 放置 UI 的父物件
    [SerializeField] private Transform uiParent;
    private List<GameObject> buildListUIObjects = new List<GameObject>();
    

    private void Awake()
    {
        removeAntherBuliding = FindFirstObjectByType<RemoveAntherBuliding>();
        gameManager = FindFirstObjectByType<GameManager>();
    }

    private void OnEnable()
    {
        removeAntherBuliding.OnGeneratePlayerBulidingList += GernerateBuildingUI;
    }

    private void OnDisable()
    {
        removeAntherBuliding.OnGeneratePlayerBulidingList -= GernerateBuildingUI;
    }

    // 產生目前顏色 BuildingName 的 List
    private void GernerateBuildingUI(int targetPlayerIndex, int score,BuildingColor buildingColor, List<PlayerData.BuildingInfo> buildingList)
    {
        removeScore = score;
        foreach (Transform child in uiParent)
        {
            Destroy(child.gameObject);
        }

        buildListUIObjects.Clear();

        switch (buildingColor)
        {
            case BuildingColor.Blue:
                currentBlockSprite = blockSprites[0];
                break;
            case BuildingColor.Red:
                currentBlockSprite = blockSprites[1];
                break;
            case BuildingColor.Yellow:
                currentBlockSprite = blockSprites[2];
                break;
            case BuildingColor.Purple:
                currentBlockSprite = blockSprites[3];
                break;
        }

        for (int i = 0; i < buildingList.Count; i++)
        {
            GameObject buildBlockUI = Instantiate(buildListUIPrefab, uiParent);
            buildListUIObjects.Add(buildBlockUI);

            // 設定建築名稱
            var bg = buildBlockUI.GetComponent<Image>();
            bg.sprite = currentBlockSprite;
            
            var buildingNameText = buildBlockUI.transform.Find("name").GetComponentInChildren<Text>();
            var confirmButton = buildBlockUI.GetComponent<Button>(); // 找到按鈕

            if (buildingNameText != null)
            {
                buildingNameText.text = $"{buildingList[i].buildingName}";
            }
            
            if (confirmButton != null)
            {
                // 需要用區域變數，避免閉包 (closure) 問題
                int buildIndex = i;
                confirmButton.onClick.AddListener(() => ChooseBuildingName(targetPlayerIndex,buildIndex,buildingColor,buildingList));
                
            }

        }
    }

    // 選擇要拆除的建築按鈕
    private void ChooseBuildingName(int targetIndex,int buildIndex ,BuildingColor buildingColor,List<PlayerData.BuildingInfo> buildingList)
    {
        List<PlayerData> targetPlayerDatas = gameManager.GetAllPlayerData();
        targetPlayerDatas[targetIndex].score -= removeScore;
        targetPlayerDatas[targetIndex].RemoveBuilding(buildingColor,buildingList[buildIndex].buildingName);
        //避免分數變成負數
        if (targetPlayerDatas[targetIndex].score < 0)
        {
            targetPlayerDatas[targetIndex].score = 0;
        }
        print(targetPlayerDatas[targetIndex].score);
        gameManager.UpdateAllPlayerData(targetPlayerDatas);
        
        GoGameScene();
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
