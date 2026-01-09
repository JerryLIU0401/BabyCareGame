using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Function.Select
{
    //設定參加人數 利用按鈕來調整人數
    public class SelectPlayerCount : MonoBehaviour
    {
        private int count;
        [SerializeField]  int minCount = 2;
        [SerializeField]  int maxCount = 6;
        [SerializeField]  TMP_Text countText;
        
        //按鈕的Image主件
        [SerializeField] private Image rightBtnImage;
        [SerializeField] private Image leftBtnImage;
        
        //要更換的圖片樣式
        [SerializeField] private Sprite defaultRightImage;
        [SerializeField] private Sprite rightImage;
        [SerializeField] private Sprite defaultLeftImage;
        [SerializeField] private Sprite leftImage;
        
        // 定義事件，通知人數確定
        public event Action<int> OnPlayerCountConfirmed;
        private void Start()
        {
            count = minCount;
            countText.text = count.ToString();
            UpdateBtnImage(count);
        }

        //增加遊玩人數
        public void AddPlayer()
        {
            if(count>=maxCount)
                return;
            count++;
            countText.text = count.ToString();
            
            UpdateBtnImage(count);
            
        }
        
        //減少遊玩人數
        public void RemovePlayer()
        {
            if (count<=minCount)
                return;
            count--;
            countText.text = count.ToString();
            
            UpdateBtnImage(count);
        }

        private void UpdateBtnImage(int number)
        {
            if (number <= minCount)
            {
                leftBtnImage.sprite = defaultLeftImage;
            }
            else if (number >= maxCount)
            {
                rightBtnImage.sprite = defaultRightImage;
            }
            else
            {
                leftBtnImage.sprite = leftImage;
                rightBtnImage.sprite = rightImage;

            }
        }

        // 確認遊玩人數 按下確認按鈕後執行
        public void ConfirmPlayerCount()
        {
            Debug.Log($"玩家數量確定為: {count}");

            // 觸發事件，通知所有訂閱者
            OnPlayerCountConfirmed?.Invoke(count);
        }
    }
}