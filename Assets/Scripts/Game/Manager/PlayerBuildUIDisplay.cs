using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerBuildUIDisplay : MonoBehaviour
{
    // 玩家 UI 預製體
    [SerializeField] private GameObject playerBuildPrefab;

    // 存放玩家頭像的陣列
    [SerializeField] private Sprite[] playerBuildSprites;
    private List<GameObject> playerBuildUIObjects = new List<GameObject>();
    private PlayerUIManager playerUIManager;

    private void Awake()
    {
        
        playerUIManager = GetComponent<PlayerUIManager>();
        
        
    }

    private void OnEnable()
    {
        playerUIManager.OnDisplayBuildingUI += CreateBuildUI;
    }

    private void OnDisable()
    {
        playerUIManager.OnDisplayBuildingUI -= CreateBuildUI;
    }

    private void CreateBuildUI(Transform targetParent,List<bool> isPlayerBuild)
    {
        // 清除現有的 UI
        foreach (Transform child in targetParent)
        {
            Destroy(child.gameObject);
        }
        playerBuildUIObjects.Clear();
        for (int i = 0;i<isPlayerBuild.Count;i++) {
            
            if (isPlayerBuild[i]){
                
                GameObject buildingUI = Instantiate(playerBuildPrefab, targetParent);
                playerBuildUIObjects.Add(buildingUI);
        
                // 設定玩家頭像
                var buildIcon = buildingUI.transform.GetComponentInChildren<Image>();
        
                if (buildIcon != null && playerBuildSprites.Length > 0)
                {
                    buildIcon.sprite = playerBuildSprites[i]; // 確保不超出陣列範圍
                }
            }
        
        }
    }
}
