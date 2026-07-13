using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Manager
{
    /// <summary>
    /// 控制遊戲內共用過場面板，讓玩家切換與結算提示能使用同一套動畫表現。
    /// </summary>
    public class TransitionManager : MonoBehaviour
    {
        [Header("過場面板的 RectTransform")]
        [SerializeField] private RectTransform panelTransform;
        
        [Header("CanvasGroup 用於控制透明度")]
        [SerializeField] private CanvasGroup canvasGroup;
        
        [Header("顯示的文字")]
        [SerializeField] private TMP_Text messageText; 
        
        [Header("滑入滑出持續時間")]
        [SerializeField] private float slideDuration = 0.5f; 
        
        [Header("停留在中間的時間")]
        [SerializeField] private float displayDuration = 1.5f; 
        
        [Header("控制滑動平滑度的動畫曲線")]
        [SerializeField] private AnimationCurve slideCurve; 
        
        [Header("螢幕外左側位置（執行時依 Canvas 寬度覆寫）")]
        [SerializeField] private Vector2 offScreenLeft = new Vector2(-800, 0);
        
        [Header("螢幕外右側位置（執行時依 Canvas 寬度覆寫）")]
        [SerializeField] private Vector2 offScreenRight = new Vector2(800, 0);
        
        [Header("中間位置")]
        [SerializeField] private Vector2 centerScreen = new Vector2(0, 0); 

        private Coroutine currentCoroutine;

        // 時間到結算這類關鍵過場必須完整播放，避免一般提示覆蓋後遺失結算回呼。
        private bool isCompletionLocked;

        private void Start()
        {
            // 先依目前 Canvas 尺寸計算畫面外位置，避免手機和平板版型不同時沿用固定座標。
            UpdateDynamicScreenPositions();

            // 初始化面板位置和透明度，讓第一個提示播放前不會露出在畫面中。
            panelTransform.anchoredPosition = offScreenLeft;
            canvasGroup.alpha = 0f;
        }

        /// <summary>
        /// 顯示過場動畫，供不需要後續流程的提示使用。
        /// </summary>
        /// <param name="message">要顯示的文字</param>
        public void ShowTransition(string message)
        {
            // 保留原本呼叫方式，讓切換玩家流程不需要知道動畫完成後是否有後續動作。
            ShowTransition(message, null, false);
        }

        /// <summary>
        /// 顯示過場動畫，並在動畫完整播放後執行指定回呼。
        /// </summary>
        /// <param name="message">要顯示的文字。</param>
        /// <param name="onCompleted">動畫結束後執行的回呼。</param>
        public void ShowTransition(string message, Action onCompleted)
        {
            // 帶有完成回呼的過場代表後面還有流程要接續，因此預設鎖定到動畫播完。
            ShowTransition(message, onCompleted, true);
        }

        /// <summary>
        /// 顯示過場動畫，並可指定本次動畫是否能被後續提示中斷。
        /// </summary>
        /// <param name="message">要顯示的文字。</param>
        /// <param name="onCompleted">動畫結束後執行的回呼。</param>
        /// <param name="lockUntilCompleted">是否鎖定到動畫播完才允許下一個提示覆蓋。</param>
        private void ShowTransition(string message, Action onCompleted, bool lockUntilCompleted)
        {
            if (currentCoroutine != null)
            {
                if (isCompletionLocked)
                {
                    // 關鍵過場正在銜接後續流程時，忽略一般提示以保證狀態不被中斷。
                    return;
                }

                // 新提示應覆蓋舊提示，避免時間到時仍被玩家切換動畫卡住。
                StopCoroutine(currentCoroutine);
            }

            isCompletionLocked = lockUntilCompleted;
            currentCoroutine = StartCoroutine(PlayTransition(message, onCompleted));
        }

        /// <summary>
        /// 播放滑入、停留與滑出流程，並在收尾後通知呼叫端。
        /// </summary>
        /// <param name="message">本次過場要顯示的文字。</param>
        /// <param name="onCompleted">動畫完成後執行的回呼。</param>
        /// <returns>回傳 Unity Coroutine 逐幀執行流程。</returns>
        private IEnumerator PlayTransition(string message, Action onCompleted)
        {
            // 每次播放前重新計算距離，避免解析度或裝置方向改變後仍使用舊版型座標。
            UpdateDynamicScreenPositions();

            // 設置文字內容
            messageText.text = message;

            // 滑入並淡入
            yield return SlideAndFade(panelTransform, offScreenLeft, centerScreen, 0f, 1f, slideDuration);

            // 停留顯示
            yield return new WaitForSeconds(displayDuration);

            // 滑出並淡出
            yield return SlideAndFade(panelTransform, centerScreen, offScreenRight, 1f, 0f, slideDuration);

            // 重置位置和透明度
            panelTransform.anchoredPosition = offScreenLeft;
            canvasGroup.alpha = 0f;
            currentCoroutine = null;
            isCompletionLocked = false;

            // 結算畫面必須等過場完全消失後才開啟，避免兩層 UI 疊在一起。
            onCompleted?.Invoke();
        }

        /// <summary>
        /// 依照目前父層 RectTransform 與面板寬度，更新左右畫面外的滑動目標位置。
        /// </summary>
        /// <remarks>
        /// 參數：無。回傳值：無；此方法會直接覆寫執行期的 <c>offScreenLeft</c> 與 <c>offScreenRight</c>。
        /// </remarks>
        private void UpdateDynamicScreenPositions()
        {
            // Canvas 版面可能在本幀稍晚才完成更新，先強制刷新可避免讀到上一個解析度的尺寸。
            Canvas.ForceUpdateCanvases();

            // 以父層寬度作為目前可視區域，讓轉場距離跟著 Canvas Scaler 的結果走。
            RectTransform parentTransform = panelTransform.parent as RectTransform;
            float parentWidth = parentTransform != null ? parentTransform.rect.width : panelTransform.rect.width;

            // 使用面板寬度納入計算，避免未來面板不是滿版時只移動父層寬度仍殘留邊角。
            float panelWidth = panelTransform.rect.width;
            float dynamicDistance = (parentWidth + panelWidth) * 0.5f;

            // Inspector 原值保留為後備距離，避免版面尚未完成、寬度讀到 0 時面板停在畫面內。
            float fallbackDistance = Mathf.Max(
                Mathf.Abs(offScreenLeft.x - centerScreen.x),
                Mathf.Abs(offScreenRight.x - centerScreen.x));
            float offScreenDistance = Mathf.Max(dynamicDistance, fallbackDistance);

            // 以 centerScreen 為基準可保留既有 Y 軸偏移設定，只動態修正水平滑動距離。
            offScreenLeft = centerScreen + Vector2.left * offScreenDistance;
            offScreenRight = centerScreen + Vector2.right * offScreenDistance;
        }

        /// <summary>
        /// 同步滑動和透明度變化
        /// </summary>
        private IEnumerator SlideAndFade(RectTransform rectTransform, Vector2 startPos, Vector2 endPos,
            float startAlpha, float endAlpha, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                // 計算插值進度
                float t = elapsed / duration;

                // 使用動畫曲線計算平滑進度
                float curveValue = slideCurve.Evaluate(t);

                // 更新位置
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, curveValue);

                // 更新透明度
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, curveValue);

                yield return null;
            }

            // 確保最終位置和透明度準確
            rectTransform.anchoredPosition = endPos;
            canvasGroup.alpha = endAlpha;
        }
    }
}
