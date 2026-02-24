using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Function.Select
{


    public class SelectGameUnit : MonoBehaviour
    {
        private int unit = 0;
        [SerializeField] private string[] units;
        [SerializeField] TMP_Text unitText;
        
        //按鈕的Image主件
        [SerializeField] private Image rightBtnImage;
        [SerializeField] private Image leftBtnImage;
        
        //要更換的圖片樣式
        [SerializeField] private Sprite defaultRightImage;
        [SerializeField] private Sprite rightImage;
        [SerializeField] private Sprite defaultLeftImage;
        [SerializeField] private Sprite leftImage;

        // 定義事件，通知遊玩單元
        public event Action<bool> OnComfirmPlayTech;
        
        private void Start()
        {
            unitText.text = units[0];
            UpdateBtnImage(unit);
        }

        //顯示下一個單元內容
        public void NextUnit()
        {
            if (unit >= units.Length - 1)
                return;
            unit++;
            unitText.text = units[unit];
            UpdateBtnImage(unit);
        }

        //顯示上一個單元內容
        public void PreviousUnit()
        {
            if (unit <= 0)
                 return;
            unit--;
            unitText.text = units[unit];
            UpdateBtnImage(unit);
        }

        //顯示當前Btn狀態
        private void UpdateBtnImage(int currentUnit)
        {
            if (currentUnit <= 0)
            {
                leftBtnImage.sprite = defaultLeftImage;
                rightBtnImage.sprite = rightImage;
            }
            else if (currentUnit >= units.Length - 1)
            {
                rightBtnImage.sprite = defaultRightImage;
                leftBtnImage.sprite = leftImage;
            }
            else
            {
                leftBtnImage.sprite = leftImage;
                rightBtnImage.sprite = rightImage;
            }
        }
        
        //觸發事件，傳送選擇的遊玩單元
        public void ComfirmPlayUnit()
        {
            if (unitText.text == "開啟")
            {
                OnComfirmPlayTech?.Invoke(true);
            }
            else
            {
                OnComfirmPlayTech?.Invoke(false);
            }
            print($"教學關卡：{unitText.text}");
        }
    }
}