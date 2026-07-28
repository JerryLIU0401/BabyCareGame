using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// 控制遊戲設定畫面中的說明書開關、圖片翻頁與邊界按鈕顯示。
    /// </summary>
    public sealed class ManualPanelController : MonoBehaviour
    {
        // 設定面板與說明書互斥顯示，可避免玩家閱讀時誤觸底層設定按鈕。
        [SerializeField] private GameObject settingPanel;
        [SerializeField] private GameObject manualPanel;

        // 頁面圖片由 Inspector 維護順序，讓內容更新不需要修改程式碼。
        [SerializeField] private Sprite[] manualPages;
        [SerializeField] private Image pageImage;

        // 邊界頁面必須直接隱藏無效按鈕，而不只是停用互動。
        [SerializeField] private GameObject previousButton;
        [SerializeField] private GameObject nextButton;

        // 頁碼只屬於目前開啟的說明書，不需要跨場景或跨次開啟保存。
        private int currentPageIndex;

        /// <summary>
        /// 顯示說明書並從第一頁開始。
        /// </summary>
        public void OpenManual()
        {
            currentPageIndex = 0;
            settingPanel.SetActive(false);
            manualPanel.SetActive(true);
            RefreshPage();
        }

        /// <summary>
        /// 關閉說明書並恢復遊戲設定畫面。
        /// </summary>
        public void CloseManual()
        {
            manualPanel.SetActive(false);
            settingPanel.SetActive(true);
        }

        /// <summary>
        /// 顯示下一頁；已在最後一頁或沒有圖片時維持原狀。
        /// </summary>
        public void ShowNextPage()
        {
            if (manualPages == null || currentPageIndex >= manualPages.Length - 1)
            {
                return;
            }

            currentPageIndex++;
            RefreshPage();
        }

        /// <summary>
        /// 顯示上一頁；已在第一頁或沒有圖片時維持原狀。
        /// </summary>
        public void ShowPreviousPage()
        {
            if (manualPages == null || currentPageIndex <= 0)
            {
                return;
            }

            currentPageIndex--;
            RefreshPage();
        }

        /// <summary>
        /// 套用目前頁面圖片，並依邊界決定是否顯示翻頁按鈕。
        /// </summary>
        private void RefreshPage()
        {
            bool hasPages = manualPages != null && manualPages.Length > 0;

            pageImage.sprite = hasPages ? manualPages[currentPageIndex] : null;
            previousButton.SetActive(hasPages && currentPageIndex > 0);
            nextButton.SetActive(hasPages && currentPageIndex < manualPages.Length - 1);
        }
    }
}
