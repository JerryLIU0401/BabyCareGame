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
    
    //該玩家是否贏得遊戲
    public bool isWin = false;
    
    
    public PlayerData(string name,Sprite sprite ,int initialScore,int scanCardCount)
    {
        playerName = name;
        playerSprite = sprite;
        score = initialScore;
        scanCount = scanCardCount;
    }
}
