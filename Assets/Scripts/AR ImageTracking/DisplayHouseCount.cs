using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class DisplayHouseCount : MonoBehaviour
{
    // 玩家 UI 預製體
    [SerializeField] private GameObject housePrefab;

    // 存放玩家頭像的陣列
    [SerializeField] private Sprite[] houseSprites;
    private List<GameObject> houseUIObjects = new List<GameObject>();
    private RemoveAntherBuliding removeAntherBuliding;

    private void Awake()
    {
        
        removeAntherBuliding = GetComponent<RemoveAntherBuliding>();
    }

    private void OnEnable()
    {
        removeAntherBuliding.OnDisplayBuildCount += CreateBuildUI;
    }

    private void OnDisable()
    {
        removeAntherBuliding.OnDisplayBuildCount -= CreateBuildUI;
    }

    private void CreateBuildUI(Transform targetParent,List<bool> isHaveBuild,List<int> isCountBuild)
    {
        // 清除現有的 UI
        foreach (Transform child in targetParent)
        {
            Destroy(child.gameObject);
        }
        houseUIObjects.Clear();
        for (int i = 0;i<isHaveBuild.Count;i++) {
            
            if (isHaveBuild[i]){
                
                GameObject houseUI = Instantiate(housePrefab, targetParent);
                houseUIObjects.Add(houseUI);

                // 設定玩家頭像
                var houseIcon = houseUI.transform.GetComponentInChildren<Image>();
                var houseCountText = houseUI.transform.Find("number").GetComponent<TMP_Text>();

                if (houseIcon != null && houseCountText!= null &&houseSprites.Length > 0)
                {
                    houseIcon.sprite = houseSprites[i]; // 確保不超出陣列範圍
                    houseCountText.text = isCountBuild[i].ToString();
                }
            }

        }
    }
}
