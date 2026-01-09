using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameResultDisplay : MonoBehaviour
{
    //用來顯示遊戲排名
    GameManager gameManager;
    
    //存放所需的UI物件
    [SerializeField] private GameObject[] numberBlock;
    //前四名放置圖像的位置
    [SerializeField] private Image[] playerImages;
    
    //轉場
    [SerializeField] SwitchScene switchScenePrefab;

    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    private void OnEnable()
    {
        gameManager.OnGameVictory += GameResultUIDisplay;
    }

    private void OnDisable()
    {
        gameManager.OnGameVictory -= GameResultUIDisplay;
    }
    
    
    private void GameResultUIDisplay(List<PlayerData> players)
    {

        // Step 2: 依照分數排序
        players.Sort((a, b) => b.score.CompareTo(a.score));
        playerImages[0].sprite = players[0].playerSprite;
        
        // Step 3: 動態生成並設定資料
        for(int i=0;i<players.Count;i++)
        {
            numberBlock[i].SetActive(true);


            playerImages[i].sprite = players[i].playerSprite;

        }
    }

    public void BackToMenu()
    {
        SwitchScene switchScenes = Instantiate(switchScenePrefab);
        switchScenes.StartCoroutine(switchScenes.loadFadeOutInScenes("Start"));
    }
}
