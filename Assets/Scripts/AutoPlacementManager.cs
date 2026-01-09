using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using UnityEngine;

public class AutoPlacementManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private float cellSize = 2f;     // 格子大小（建築物間距）

    [Header("Building Settings")]
    [SerializeField] private GameObject buildingPrefab; // 建築物的預置體
    [SerializeField] private Transform gridParent;     // 放置格子的父物件（組織場景層級）

    [Header("Building Data List")]
    public List<BuildingData> buildingDataList = new List<BuildingData>();
    private List<BuildingColor> colorList = new List<BuildingColor> { BuildingColor.Blue, BuildingColor.Red, BuildingColor.Purple, BuildingColor.Yellow };
    
    private GameManager gameManager;
    private PlayerUIManager uiManager;

    [System.Serializable]
    public class BuildingData
    {
        public BuildingColor color;     // 房子的顏色
        public int size = 1;    // 佔用格子的數量（1~n）
    }
    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        uiManager = FindFirstObjectByType<PlayerUIManager>();
    }
    
    private void OnEnable()
    {
        uiManager.OnModelDisplay += GenerateBuildings;
    }

    private void OnDisable()
    {
        uiManager.OnModelDisplay -= GenerateBuildings;
    }

    /// <summary>
    /// 生成建築物並自動放置
    /// </summary>
    private void GenerateBuildings()
    { 
        //每回合開始前都先將上一回的資料進行清除
        buildingDataList.Clear();
        foreach (Transform child in gridParent)
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
                BuildingData data = new BuildingData();
                data.color = colorList[i];
                buildingDataList.Add(data);
            }
        }

        if (buildingDataList.Count <= 0) return;

        int colCount = 3; // 每列顯示3棟
        float cellSizeX = cellSize;
        float cellSizeZ = cellSize;

        int rowCount = Mathf.CeilToInt(buildingDataList.Count / (float)colCount);

        float totalWidth = (colCount - 1) * cellSizeX;
        float totalDepth = (rowCount - 1) * cellSizeZ;

        Vector3 startPosition = transform.position - new Vector3(totalWidth / 2f, 0, totalDepth / 2f);

        for (int i = 0; i < buildingDataList.Count; i++)
        {
            BuildingData data = buildingDataList[i];

            int row = i / colCount;
            int col = i % colCount;

            Vector3 spawnPosition = startPosition + new Vector3(col * cellSizeX, 0, row * cellSizeZ);

            GameObject building = Instantiate(buildingPrefab, spawnPosition, Quaternion.identity, gridParent);
            building.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f); // 固定大小

            Renderer renderer = building.GetComponent<Renderer>();
            if (renderer != null)
            {
                switch (data.color)
                {
                    case BuildingColor.Blue: renderer.material.color = Color.blue; break;
                    case BuildingColor.Red: renderer.material.color = Color.red; break;
                    case BuildingColor.Purple: renderer.material.color = new Color(0.5f, 0f, 0.5f);; break;
                    case BuildingColor.Yellow: renderer.material.color = Color.yellow; break;
                }
            }
        }
    }
}
