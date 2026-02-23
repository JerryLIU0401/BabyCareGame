using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

[System.Serializable]
public class SpinOption
{
    public string itemName;
    [Range(0, 100)] public float weight;
    public Color themeColor = Color.white;
    
    [HideInInspector] public float startAngle;
    [HideInInspector] public float endAngle;
}

public class SpinFateManager : MonoBehaviour
{
    [Header("UI 組件連結")]
    [SerializeField] private RectTransform arrowImage;
    [SerializeField] private Button spinButton;

    [Header("轉盤參數設定")]
    [SerializeField] private List<SpinOption> options = new List<SpinOption>();
    [SerializeField] private float spinDuration = 3.0f;
    
    [Header("轉盤分區視覺 (Prefab)")]
    [SerializeField] private GameObject pieSegmentPrefab; 
    [SerializeField] private Transform pieParent;

    [Header("側邊圖例視覺 (Prefab)")]
    [SerializeField] private GameObject legendItemPrefab; 
    [SerializeField] private Transform legendParent;      

    private bool isSpinning = false;

    void Start()
    {
        RefreshWheel();
    }

    public void RefreshWheel()
    {
        UpdateWheelAngles();
        CreateLegend();
    }

    private void CreateLegend()
    {
        if (pieParent == null || legendParent == null) return;

        ClearChildren(legendParent);
        ClearChildren(pieParent);

        UpdateWheelAngles(); 

        // 提醒：currentRotationZ 用於設定每個扇形在圓盤上的「起始旋轉位置」。
        float currentRotationZ = 0f; 

        foreach (var option in options)
        {
            // 1. 生成圖例條目
            GameObject item = Instantiate(legendItemPrefab, legendParent);
            item.transform.Find("ColorBox").GetComponent<Image>().color = option.themeColor;
            item.transform.Find("Label").GetComponent<TMP_Text>().text = $"{option.itemName} ({option.weight}%)";

            // 2. 生成轉盤彩色切片
            GameObject segment = Instantiate(pieSegmentPrefab, pieParent);
            Image segmentImg = segment.GetComponent<Image>();
        
            segmentImg.color = option.themeColor;
            segmentImg.fillAmount = option.weight / 100f; 
        
            // 提醒：此處使用「負號」旋轉分區，使其從 12 點鐘方向開始「順時針」排列。
            segment.transform.localRotation = Quaternion.Euler(0, 0, -currentRotationZ);
        
            // 提醒：累加角度，為下一個分區做準備。
            currentRotationZ += (option.weight / 100f) * 360f;

            RectTransform rt = segment.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = pieParent.GetComponent<RectTransform>().sizeDelta;
            rt.localScale = Vector3.one;
        }
    }

    private void ClearChildren(Transform parent)
    {
        if (parent == null) return;
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in parent) children.Add(child.gameObject);
        foreach (GameObject child in children)
        {
            if (Application.isPlaying) Destroy(child);
            else DestroyImmediate(child);
        }
    }

    private void UpdateWheelAngles()
    {
        float totalWeight = options.Sum(x => x.weight);
        float currentAngle = 0;
        foreach (var option in options)
        {
            float percentage = option.weight / totalWeight;
            option.startAngle = currentAngle;
            option.endAngle = currentAngle + (percentage * 360f);
            currentAngle = option.endAngle;
        }
    }

    public void SpinFate()
    {
        if (isSpinning || options.Count == 0) return;
    
        UpdateWheelAngles();

        // 1. 加權隨機選擇結果
        float totalWeight = options.Sum(x => x.weight);
        float randomValue = Random.Range(0, totalWeight);
        float tempSum = 0;
        SpinOption selectedOption = null;

        foreach (var option in options)
        {
            tempSum += option.weight;
            if (randomValue <= tempSum)
            {
                selectedOption = option;
                break;
            }
        }

        // 2. 計算目標角度並加入隨機偏移
        float safePadding = 2.0f; 
        float randomTargetAngle = Random.Range(selectedOption.startAngle + safePadding, 
            selectedOption.endAngle - safePadding);
    
        // --- 大師級修正區 ---
    
        // 提醒：如果指針用「尾巴」對準，代表差了 180 度。
        // 如果指針圖片本身是橫的 (Z=90)，則總共需要偏移量。
        // 我們嘗試將補償值從 90f 改為 270f (即 90 + 180)，這通常能解決尾巴對準的問題。
        float visualOffset = 270f; 
    
        float totalRotation = (360f * 5f) + randomTargetAngle + visualOffset;

        StartCoroutine(SpinRoutine(totalRotation, selectedOption));
    }

    private IEnumerator SpinRoutine(float totalDegree, SpinOption result)
    {
        isSpinning = true;
        spinButton.interactable = false;

        float elapsed = 0;
        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / spinDuration;
            float curve = 1 - Mathf.Pow(1 - t, 3); 
            float currentAngle = curve * totalDegree;
            // 提醒：負號旋轉確保指針是「順時針」轉動。
            arrowImage.localRotation = Quaternion.Euler(0, 0, -currentAngle);
            yield return null;
        }

        arrowImage.localRotation = Quaternion.Euler(0, 0, -totalDegree);
        isSpinning = false;
        spinButton.interactable = true;
        
        Debug.Log($"<color=#{ColorUtility.ToHtmlStringRGB(result.themeColor)}>中獎結果：{result.itemName}</color>");
    }
}