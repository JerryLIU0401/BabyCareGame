using System.Collections;
using System.Collections.Generic;
using CardModel;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]


//紀錄每一位玩家的遊戲資訊
public class PlayerData
{
    public string playerName;
    public Sprite playerSprite;
    //環保值分數
    public int score;
    
    public int scanCount = 0;

    //判斷有幾種不同顏色的建築物
    public bool blueBuilding = false;
    public bool redBuilding = false;
    public bool yellowBuilding = false;
    public bool purpleBuilding = false;
    
    //該玩家是否贏得遊戲
    public bool isWin = false;
    
    //------------------------------
    // 用於 Inspector 顯示的分類建築物
    [SerializeField]
    private ColorBuildingList colorBuildings = new ColorBuildingList();

    // 用於運行時快速查詢的 Dictionary（非序列化）
    [System.NonSerialized]
    public Dictionary<BuildingColor, List<BuildingInfo>> buildingColors = new Dictionary<BuildingColor, List<BuildingInfo>>();
    
    // 建築物資訊類
    [System.Serializable]
    public class BuildingInfo
    {
        public string buildingName;
    }

    // 按顏色分類的序列化結構
    [System.Serializable]
    private class ColorBuildingList
    {
        [SerializeField] public List<BuildingInfo> blueBuildings = new List<BuildingInfo>();
        [SerializeField] public List<BuildingInfo> redBuildings = new List<BuildingInfo>();
        [SerializeField] public List<BuildingInfo> yellowBuildings = new List<BuildingInfo>();
        [SerializeField] public List<BuildingInfo> purpleBuildings = new List<BuildingInfo>();
        // 如果 BuildingColor 增加新顏色，這裡需要手動添加對應的 List
    }
    
    // 依序回傳建築顏色是否擁有建築物的 bool 列表（順序：藍、紅、黃、紫）
    public List<bool> GetBuildingColorStateList()
    {
        return new List<bool>
        {
            colorBuildings.blueBuildings.Count > 0,  // 是否有藍色建築
            colorBuildings.redBuildings.Count > 0,   // 是否有紅色建築
            colorBuildings.yellowBuildings.Count > 0, // 是否有黃色建築
            colorBuildings.purpleBuildings.Count > 0, // 是否有紫色建築
        };
    }
    
    
    // 依序回傳建築顏色是否擁有建築物的 bool 列表（順序：藍、紅、黃、紫）
    public List<int> GetColorBuildingsCountList()
    {
        return new List<int>
        {
            colorBuildings.blueBuildings.Count,  // 是否有藍色建築
            colorBuildings.redBuildings.Count,   // 是否有紅色建築
            colorBuildings.yellowBuildings.Count, // 是否有黃色建築
            colorBuildings.purpleBuildings.Count, // 是否有紫色建築
        };
    }
    
    //調整建築物的顏色種類判斷

    public void ColorBuildingState()
    {
        blueBuilding = (colorBuildings.blueBuildings.Count > 0)? true : false; 
        redBuilding = (colorBuildings.redBuildings.Count > 0)? true : false;
        yellowBuilding = (colorBuildings.yellowBuildings.Count > 0)? true : false;
        purpleBuilding = (colorBuildings.purpleBuildings.Count > 0)? true : false;
    }
    
    //回傳是否有三個顏色以上的建築
    public bool HasThreeOrMoreBuildings()
    {
        int trueCount = 0;

        if (blueBuilding) trueCount++;
        if (redBuilding) trueCount++;
        if (yellowBuilding) trueCount++;
        if (purpleBuilding) trueCount++;
        
        return trueCount >= 3;
    }
    
    //獲得當前顏色建築的顏色
    public int GetColorBuildingsCount(BuildingColor color)
    {
        switch (color)
        {
            case BuildingColor.Blue:
                return colorBuildings.blueBuildings.Count;
                break;
            case BuildingColor.Red:
                return colorBuildings.redBuildings.Count;
                break;
            case BuildingColor.Yellow:
                return colorBuildings.yellowBuildings.Count;
                break;
            case BuildingColor.Purple:
                return colorBuildings.purpleBuildings.Count;
                break;
            default:
                return 0;
        }
    }

    //回傳當前顏色的建築list
    public List<BuildingInfo> GetBuildingColorList(BuildingColor color)
    {
        switch (color)
        {
            case BuildingColor.Blue:
                return colorBuildings.blueBuildings;
                break;
            case BuildingColor.Red:
                return colorBuildings.redBuildings;
                break;
            case BuildingColor.Yellow:
                return colorBuildings.yellowBuildings;
                break;
            case BuildingColor.Purple:
                return colorBuildings.purpleBuildings;
                break;
            default:
                return null;
        }
    }
    
    //------------------------------
    
    public PlayerData(string name,Sprite sprite ,int initialScore,int scanCardCount)
    {
        playerName = name;
        playerSprite = sprite;
        score = initialScore;
        scanCount = scanCardCount;
        
        //---------------------
        // 初始化 Dictionary
        foreach (BuildingColor color in System.Enum.GetValues(typeof(BuildingColor)))
        {
            buildingColors[color] = new List<BuildingInfo>();
        }
        SyncSerializedToDictionary(); // 確保初始同步
        //---------------------
        
        // // Test 拆除功能
        // AddBuilding(BuildingColor.Red,"test");
        // // AddBuilding(BuildingColor.Blue,"test1");
        // // AddBuilding(BuildingColor.Green,"test2");
        // // AddBuilding(BuildingColor.Green,"test3");
    }
    
    //----------------------
    // 同步 Inspector 資料到 Dictionary
    private void SyncSerializedToDictionary()
    {
        buildingColors.Clear();
        buildingColors[BuildingColor.Red] = colorBuildings.redBuildings;
        buildingColors[BuildingColor.Blue] = colorBuildings.blueBuildings;
        buildingColors[BuildingColor.Yellow] = colorBuildings.yellowBuildings;
        buildingColors[BuildingColor.Purple] = colorBuildings.purpleBuildings;
    }

    // 同步 Dictionary 到 Inspector 資料
    private void SyncDictionaryToSerialized()
    {
        colorBuildings.redBuildings = buildingColors[BuildingColor.Red];
        colorBuildings.blueBuildings = buildingColors[BuildingColor.Blue];
        colorBuildings.yellowBuildings = buildingColors[BuildingColor.Yellow];
        colorBuildings.purpleBuildings = buildingColors[BuildingColor.Purple];
    }
    // 判斷玩家是否擁有某顏色的建築
    public bool HasBuildingOfColor(BuildingColor color)
    {
        SyncSerializedToDictionary();
        return buildingColors.ContainsKey(color) && buildingColors[color].Count > 0;
    }

    // 新增建築物到指定顏色
    public void AddBuilding(BuildingColor color, string buildingName)
    {
        BuildingInfo newBuilding = new BuildingInfo { buildingName = buildingName };
        if (buildingColors.ContainsKey(color))
        {
            buildingColors[color].Add(newBuilding);
            SyncDictionaryToSerialized(); // 更新 Inspector 顯示
        }
    }
    
    //可以選擇想要去除的建築物名稱
    public void RemoveBuilding(BuildingColor color, string buildingName)
    {
        if (buildingColors.ContainsKey(color))
        {
            BuildingInfo toRemove = buildingColors[color].Find(b => b.buildingName == buildingName);
            if (toRemove != null)
            {
                buildingColors[color].Remove(toRemove);
                SyncDictionaryToSerialized(); // 更新 Inspector
            }
        }
    }

    // 在 Inspector 修改時調用（僅編輯器有效）
    public void OnValidate()
    {
        SyncSerializedToDictionary();
    }
    //----------------------
    
}
