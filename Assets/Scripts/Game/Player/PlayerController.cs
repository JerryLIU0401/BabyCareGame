using System;
using System.Collections;
using System.Collections.Generic;
using Function.Select;
using Manager;
using TMPro;
using UnityEngine;


//負責玩家的點數相關處理
//顯示玩家當前的分數並時時更新到data中
namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        private GameManager gameManager;
        [SerializeField] private TMP_Text playScore;
        
        
        private void Awake()
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
        
        
        private void OnEnable()
        {
            gameManager.ChangePlayerUI += ChangUI;
            gameManager.OnPlayerDataGenerated += InitialPlayerScore;
        }

        private void OnDisable()
        {
            gameManager.ChangePlayerUI -= ChangUI;
            gameManager.OnPlayerDataGenerated -= InitialPlayerScore;
        }

        //遊戲一開始寫入玩家的資料,因為遊戲一開始一定是零 所以就不在特別讀取資料則直接寫入
        private void InitialPlayerScore(List<PlayerData> playerData)
        {
            playScore.text = 0.ToString();
        }
        
        // //調整現在開始的玩家數據及顯示，回合結束時運作
        private void ChangUI(GameData currentGameDataData)
        {
            playScore.text = $"{gameManager.GetPlayerData().score}";
        
        }
        
        //當玩家分數有進行改變時就呼叫該函示，來更新資料以及文字內容
        public void EditPlayerScoreData(int score)
        {
            print("score");
            playScore.text = $"{score}";
        }
        
    }
}
