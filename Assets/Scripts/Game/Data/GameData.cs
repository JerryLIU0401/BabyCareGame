using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
//紀錄遊戲內的重要資訊以及目前回合可以觸發的按鍵次數為何
public class GameData
{
    //一個人可以掃描的次數
    public int scanCountTime;
    //看是否已經完成初始設定
    public bool isSetting = false;
    //遊玩單元
    public string unitName = "";
    //玩家人數
    public int playerCount;
    //紀錄當前進行到的玩家
    public int currentPlayer;
    //紀錄當前回合的掃牌次數
    public int scanCount = 0;
    //判斷是否有掃描卡片
    public bool isScan = false;
    //初始化基本資料
    public GameData(bool gameStart,int player,int initialPlayer,int initialScanCOunt, int scanCardCount)
    {
        isSetting = gameStart;
        playerCount = player;
        currentPlayer = initialPlayer;
        scanCountTime = initialScanCOunt;
        scanCount = scanCardCount;
    }
    
}