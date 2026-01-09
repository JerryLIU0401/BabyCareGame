using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Manager;
using Player;
using Random = UnityEngine.Random;

namespace Function.Select
{
    //用來選擇遊戲開始順序
    //接受SelectPlayerCount的玩家人數
    public class SelectOrder : MonoBehaviour
    {
        //用於儲存數據
        private GameManager gameManager;
        private PlayerController playerController;
        private PlayerUIManager playerUIManager;
        
        private int order = 0;
        private int startPlayer;
        private SelectPlayerCount selectPlayerCount;


        private void Awake()
        {
            gameManager = FindFirstObjectByType<GameManager>();
            playerController = FindFirstObjectByType<PlayerController>();
            playerUIManager = FindFirstObjectByType<PlayerUIManager>();
            selectPlayerCount = FindFirstObjectByType<SelectPlayerCount>();
        }

        private void OnEnable()
        {
            gameManager.OnPlayerDataGenerated += DisplayPlayerMessenger;
        }

        private void OnDisable()
        {
            gameManager.OnPlayerDataGenerated -= DisplayPlayerMessenger;
        }

        private void DisplayPlayerMessenger(List<PlayerData> playerCount)
        {
            order = playerCount.Count;
            startPlayer = Random.Range(1, order+1);
            FindFirstObjectByType<TransitionManager>().ShowTransition($"第{startPlayer}位玩家開始");
            
            //儲存遊戲數據,將玩家的數量以及第幾位玩家的開始傳入
            gameManager.InitialGameData(startPlayer,order);
            
            //通知玩家UI進行更改
            playerUIManager.GetStartPlayerCount(startPlayer);
        }



       
    }
}
