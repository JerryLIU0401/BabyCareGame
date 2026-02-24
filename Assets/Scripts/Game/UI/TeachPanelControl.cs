using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TeachPanelControl : MonoBehaviour
{
   private int index = 0;
   
   [SerializeField] private Sprite[] teachSprites;
   [SerializeField] private Image displayTechSprite;
   
   //按鈕的Image主件
   [SerializeField] private Image rightBtnImage;
   [SerializeField] private Image leftBtnImage;
        
   //要更換的圖片樣式
   [SerializeField] private Sprite defaultRightImage;
   [SerializeField] private Sprite rightImage;
   [SerializeField] private Sprite defaultLeftImage;
   [SerializeField] private Sprite leftImage;


   private void Start()
   {
      displayTechSprite.sprite = teachSprites[index];
      UpdateBtnImage(index);
   }
   
   //顯示下一個單元內容
   public void NextImage()
   {
      if (index >= teachSprites.Length - 1)
         return;
      index++;
      displayTechSprite.sprite = teachSprites[index];
      UpdateBtnImage(index);
   }

   //顯示上一個單元內容
   public void PreviousImage()
   {
      if (index <= 0)
         return;
      index--;
      displayTechSprite.sprite = teachSprites[index];
      UpdateBtnImage(index);
   }

   //顯示當前Btn狀態
   private void UpdateBtnImage(int currentUnit)
   {
      if (currentUnit <= 0)
      {
         leftBtnImage.sprite = defaultLeftImage;
         rightBtnImage.sprite = rightImage;
      }
      else if (currentUnit >= teachSprites.Length - 1)
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
}
