using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 控制教學面板的圖片翻頁，並在玩家完成或跳過教學後啟動正式遊戲倒數。
/// </summary>
public class TeachPanelControl : MonoBehaviour
{
   // 教學圖片索引由此類別單獨管理，避免按鈕直接碰觸陣列造成越界風險。
   private int index = 0;
   
   // 透過 Inspector 維護教學圖片順序，讓企劃可在不改程式的情況下調整教學頁面。
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


   /// <summary>
   /// 初始化教學第一頁與翻頁按鈕狀態。
   /// </summary>
   private void Start()
   {
      if (teachSprites == null || teachSprites.Length == 0)
      {
         Debug.LogWarning("[TeachPanelControl] teachSprites 未設定，將直接結束教學並開始遊戲倒數。", this);
         CompleteTeachAndStartGame();
         return;
      }

      displayTechSprite.sprite = teachSprites[index];
      UpdateBtnImage(index);
   }
   
   /// <summary>
   /// 顯示下一張教學圖片；若已在最後一張，代表玩家已閱讀完教學。
   /// </summary>
   public void NextImage()
   {
      if (index >= teachSprites.Length - 1)
      {
         // 最後一頁再次按下一步時才啟動倒數，讓玩家有時間閱讀最後一張教學圖。
         CompleteTeachAndStartGame();
         return;
      }

      index++;
      displayTechSprite.sprite = teachSprites[index];
      UpdateBtnImage(index);
   }

   /// <summary>
   /// 顯示上一張教學圖片。
   /// </summary>
   public void PreviousImage()
   {
      if (index <= 0)
         return;
      index--;
      displayTechSprite.sprite = teachSprites[index];
      UpdateBtnImage(index);
   }

   /// <summary>
   /// 結束教學面板並啟動遊戲倒數，供跳過教學與閱讀完成共用。
   /// </summary>
   public void CompleteTeachAndStartGame()
   {
      // 教學面板只負責宣告流程完成，倒數啟動與時間到事件綁定統一交給 GameManager。
      GameManager gameManager = FindFirstObjectByType<GameManager>();
      if (gameManager == null)
      {
         Debug.LogWarning("[TeachPanelControl] 找不到 GameManager，無法啟動正式遊戲倒數。", this);
      }
      else
      {
         gameManager.StartGameTimerAfterSetup();
      }

      gameObject.SetActive(false);
   }

   /// <summary>
   /// 依目前頁面切換左右翻頁按鈕的視覺狀態。
   /// </summary>
   /// <param name="currentUnit">目前教學圖片索引。</param>
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
