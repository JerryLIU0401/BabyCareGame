using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SpinFateManager : MonoBehaviour
{
    [SerializeField] private Button spinButton;
    [SerializeField] private RectTransform arrowImage;
    
    
    //--------------------- 命運轉盤功能 ----------------------------
    public void SpinFate()
    {
        if (arrowImage == null) return;
        arrowImage.localRotation = Quaternion.identity;
        float randomDegree = Random.Range(1080f, 1800f);
        StartCoroutine(SpinRoutine(randomDegree));
    }
        
    //呼叫協程
    private IEnumerator SpinRoutine(float totalDegree)
    {
        float duration = 3.0f;
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float curve = 1 - Mathf.Pow(1 - t, 3);
            float angle = curve * totalDegree;
            arrowImage.localRotation = Quaternion.Euler(0, 0, -angle);
            yield return null;
        }
        arrowImage.localRotation = Quaternion.Euler(0, 0, -totalDegree);
        //--------------------- 下方填寫轉盤轉動到的內容 ----------------------------
    }
}
