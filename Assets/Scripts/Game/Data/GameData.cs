using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
//紀錄遊戲內的重要資訊以及目前回合可以觸發的按鍵次數為何
public class GameData
{

    //看是否已經完成初始設定
    public bool isSetting = false;
    
    //教學關卡是否需要
    public bool isTech = true;
    
    //玩家人數
    public int playerCount;
    
    //紀錄當前進行到的玩家
    public int currentPlayer;
    
    //初始化基本資料
    public GameData(bool gameStart,int player,int initialPlayer)
    {
        isSetting = gameStart;
        playerCount = player;
        currentPlayer = initialPlayer;
    }
    
}