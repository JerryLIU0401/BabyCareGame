using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 控制開始畫面的輸入偵測，並在玩家確認開始時切換到遊戲場景。
/// </summary>
public class MenuController : MonoBehaviour
{
    // 透過 Inspector 綁定轉場 Prefab，讓 Start 場景可以沿用專案既有的淡入淡出流程。
    [SerializeField] SwitchScene switchScenePrefab;

    // 防止玩家快速連點造成重複載入場景，避免 Start 到 Game 的轉場流程被觸發多次。
    private bool isLoading;

    /// <summary>
    /// 每幀偵測玩家是否已在開始畫面完成點擊。
    /// </summary>
    private void Update()
    {
        if (isLoading)
        {
            return;
        }

        if (HasStartInput())
        {
            StartGame();
        }
    }

    /// <summary>
    /// 判斷玩家是否輸入開始遊戲指令。
    /// </summary>
    /// <returns>若偵測到手機觸控結束或 PC 滑鼠左鍵放開，回傳 true；否則回傳 false。</returns>
    private bool HasStartInput()
    {
        if (Input.GetMouseButtonUp(0))
        {
            return true;
        }

        if (Input.touchCount <= 0)
        {
            return false;
        }

        TouchPhase touchPhase = Input.GetTouch(0).phase;

        return touchPhase == TouchPhase.Ended;
    }

    /// <summary>
    /// 啟動遊戲場景切換流程。
    /// </summary>
    private void StartGame()
    {
        isLoading = true;

        if (switchScenePrefab == null)
        {
            // 若場景遺漏轉場 Prefab 綁定，仍保留直接切場景作為保底，避免玩家卡在開始畫面。
            SceneManager.LoadScene("Game");
            return;
        }

        SwitchScene switchScenes = Instantiate(switchScenePrefab);
        switchScenes.StartCoroutine(switchScenes.loadFadeOutInScenes("Game"));
    }
}
