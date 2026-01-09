using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Manager
{
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
        
        [Header("螢幕外左側位置")]
        [SerializeField] private Vector2 offScreenLeft = new Vector2(-800, 0);
        
        [Header("螢幕外右側位置")]
        [SerializeField] private Vector2 offScreenRight = new Vector2(800, 0);
        
        [Header("中間位置")]
        [SerializeField] private Vector2 centerScreen = new Vector2(0, 0); 

        private Coroutine currentCoroutine;

        private void Start()
        {
            // 初始化面板位置和透明度
            panelTransform.anchoredPosition = offScreenLeft;
            canvasGroup.alpha = 0f;
        }

        /// <summary>
        /// 顯示過場動畫
        /// </summary>
        /// <param name="message">要顯示的文字</param>
        public void ShowTransition(string message)
        {
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }

            currentCoroutine = StartCoroutine(PlayTransition(message));
        }

        private IEnumerator PlayTransition(string message)
        {
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