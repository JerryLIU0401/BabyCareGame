using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{

    public class TitleBlink : MonoBehaviour
    {
        public float blinkInterval = 0.5f; // 閃爍間隔（秒）
        public float fadeDuration = 0.5f; // 淡入淡出持續時間（秒）
        private TMP_Text tipText;
        private bool isFadingOut = false;

        void Start()
        {
            tipText = GetComponent<TMP_Text>();
            // 啟動閃爍效果
            //每隔幾秒調用一次這個方法,第一次執行不用間隔
            InvokeRepeating("ToggleTextVisibility", 0, blinkInterval);
        }

        void ToggleTextVisibility()
        {

            if (isFadingOut)
            {
                StartCoroutine(FadeText(false)); // 淡出文字
            }
            else
            {
                StartCoroutine(FadeText(true)); // 淡入文字
            }
        }

        IEnumerator FadeText(bool fadeIn)
        {
            isFadingOut = !isFadingOut;
            Color targetColor = tipText.color;
            targetColor.a = fadeIn ? 1 : 0; // 目標透明度
            float timer = 0;

            while (timer < fadeDuration)
            {
                //讓透明度可以隨時間漸增或漸減，不會突然關閉變得奇怪
                float alpha = Mathf.Lerp(tipText.color.a, targetColor.a, timer / fadeDuration);
                //保留原有的顏色紙更改透明度
                tipText.color = new Color(tipText.color.r, tipText.color.g, tipText.color.b, alpha);
                timer += Time.deltaTime;
                yield return null;
            }

            tipText.color = targetColor; // 確保最終設置正確的顏色
        }
    }
}
