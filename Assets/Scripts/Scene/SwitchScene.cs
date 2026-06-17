using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 負責跨場景的全螢幕淡入淡出轉場，並在轉場完成後釋放遮罩物件。
/// </summary>
public class SwitchScene : MonoBehaviour
{
    // 轉場遮罩會跨場景保留，因此必須在淡出結束時明確釋放，避免透明物件持續攔截 UI 點擊。
    private CanvasGroup canvasGroup;

    // 保留 public 欄位以維持既有 Prefab Inspector 綁定，不在本次修正中改動序列化介面。
    public float fadeInDuration;
    public float fadeOutDuration;


    /// <summary>
    /// 初始化轉場遮罩並讓物件在場景載入時不被 Unity 自動銷毀。
    /// </summary>
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        // 轉場物件必須跨過 LoadScene 的卸載流程，才能在新場景完成淡入收尾。
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 先淡出目前畫面、載入指定場景，再淡入新場景並銷毀轉場物件。
    /// </summary>
    /// <param name="sceneName">要載入的 Unity 場景名稱。</param>
    /// <returns>回傳 Unity Coroutine，讓呼叫端可等待逐幀轉場流程。</returns>
    public IEnumerator loadFadeOutInScenes(string sceneName)
    {
        yield return FadeOut(fadeOutDuration);
        yield return loadScenes(sceneName);
        yield return FadeIn(fadeInDuration);
    }
    
    /// <summary>
    /// 僅播放淡出與淡入，不切換場景。
    /// </summary>
    /// <returns>回傳 Unity Coroutine，供需要純遮罩動畫的流程使用。</returns>
    public IEnumerator FadeOutInScenes()
    {
        yield return FadeOut(fadeOutDuration);
        yield return FadeIn(fadeInDuration);
    }


    /// <summary>
    /// 將轉場遮罩淡到完全不透明，避免場景載入期間看到未完成的 UI 狀態。
    /// </summary>
    /// <param name="time">淡出持續秒數。</param>
    /// <returns>回傳 Unity Coroutine，逐幀更新 CanvasGroup 透明度。</returns>
    public IEnumerator FadeOut(float time)
    {
        // 轉場期間需要攔截操作，避免玩家在新舊場景交界時點到尚未初始化完成的 UI。
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // duration 防止 Inspector 被設成 0 時發生除以零，也讓測試場景能安全使用瞬間轉場。
        float duration = Mathf.Max(time, Mathf.Epsilon);
        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha = Mathf.Min(canvasGroup.alpha + Time.deltaTime / duration, 1f);
            yield return null;
        }

        // 明確固定到 1，避免浮點累積誤差影響後續淡入判斷。
        canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// 將轉場遮罩淡到完全透明，完成後關閉互動攔截並銷毀自身。
    /// </summary>
    /// <param name="time">淡入持續秒數。</param>
    /// <returns>回傳 Unity Coroutine，逐幀更新 CanvasGroup 透明度。</returns>
    public IEnumerator FadeIn(float time)
    {
        // duration 防止零秒設定造成無限或非法數值，讓收尾流程仍能穩定執行。
        float duration = Mathf.Max(time, Mathf.Epsilon);
        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha = Mathf.Max(canvasGroup.alpha - Time.deltaTime / duration, 0f);
            yield return null;
        }

        // 收尾時必須關閉 Raycast 攔截，避免物件銷毀延後一幀時透明遮罩仍擋住下一個畫面的按鈕。
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        Destroy(gameObject);
    }

    /// <summary>
    /// 載入指定 Unity 場景。
    /// </summary>
    /// <param name="sceneName">要載入的 Unity 場景名稱。</param>
    /// <returns>回傳 Unity Coroutine，讓轉場流程能等待載入呼叫完成一幀。</returns>
    public IEnumerator loadScenes(string sceneName)
    {
        // 集中透過此方法切換場景，讓淡入淡出流程的呼叫順序維持一致。
        SceneManager.LoadScene(sceneName);
        yield return null;
    }
}
