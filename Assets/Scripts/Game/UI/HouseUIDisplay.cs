using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Manager;
using UnityEngine.UI;

public class HouseUIDisplay : MonoBehaviour
{
    
    [Header("Building Settings")]
    [SerializeField] private GameObject buildingUIPrefab; // 建築物的預置體
    [SerializeField] private Transform uiParent;     // 放置格子的父物件（組織場景層級）
    
    [SerializeField] private Sprite[] houseSprites;
    
    private GameManager gameManager;
    private PlayerUIManager uiManager;
    
    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        uiManager = FindFirstObjectByType<PlayerUIManager>();
        
        foreach (Transform child in uiParent)
        {
            Destroy(child.gameObject);
        }
    }
    
    private void OnEnable()
    {
        uiManager.OnModelDisplay += GenerateBuildingsUI;
    }

    private void OnDisable()
    {
        uiManager.OnModelDisplay -= GenerateBuildingsUI;
    }
    
    
    private void GenerateBuildingsUI()
    { 
        // //每回合開始前都先將上一回的資料進行清除
        // buildingDataList.Clear();
        foreach (Transform child in uiParent)
        {
            Destroy(child.gameObject);
        }
        
        PlayerData player = gameManager.GetPlayerData();
        
        if (player == null)
        {
            return;
        }
        
        List<int> buildCount = new List<int>();
        buildCount = player.GetColorBuildingsCountList();

        if (buildCount.Count != 0)
        {
            for (int i = 0; i < buildCount.Count; i++)
            {
                if(buildCount[i] == 0) continue;
                
                GameObject houseObj = Instantiate(buildingUIPrefab, uiParent);
                var houseIcon = houseObj.GetComponent<Image>();

                houseIcon.sprite = houseSprites[i];
            }
        }

       
    }
}
