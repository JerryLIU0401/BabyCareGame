using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    //用來判斷有沒有手指點擊螢幕，如果有的話我們就進入遊戲
    // [Header("場景轉場物體")]
    // public SwitchScenes scenesCanvaPrefabs;
    //轉場
    [SerializeField] SwitchScene switchScenePrefab;
    void Update()
    {
        if (Input.touchCount > 0)
        {
            TouchPhase touchPhase = Input.GetTouch(0).phase;
            switch (touchPhase)
            {
                case TouchPhase.Ended:
                    // PlayGame();
                    SceneManager.LoadScene("Game");
                    break;
            }
        }
    }

    //移動到AR場景
    private void GoGameScene()
    {
        SwitchScene switchScenes = Instantiate(switchScenePrefab);
        switchScenes.StartCoroutine(switchScenes.loadFadeOutInScenes("Game"));
    }
    
}
