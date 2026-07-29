using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// 控制說明書的面板切換、圖片翻頁、邊界按鈕與選配的倒數暫停。
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

        // GameSetting 不需要計時器；Game 場景可選配此參考，重用同一套說明書行為。
        [SerializeField] private GameTimer gameTimer;

        // 只恢復本次開啟前確實正在執行的倒數，避免誤啟動其他流程已暫停的計時器。
        private bool shouldResumeTimer;

        // 頁碼只屬於目前開啟的說明書，不需要跨場景或跨次開啟保存。
        private int currentPageIndex;

        /// <summary>
        /// 顯示說明書並從第一頁開始；若倒數正在執行則暫停。
        /// </summary>
        public void OpenManual()
        {
            currentPageIndex = 0;
            shouldResumeTimer = gameTimer != null && gameTimer.IsRunning;

            if (shouldResumeTimer)
            {
                // 只暫停局內倒數，不使用 timeScale 影響其他遊戲系統。
                gameTimer.PauseTimer();
            }

            settingPanel.SetActive(false);
            manualPanel.SetActive(true);
            RefreshPage();
        }

        /// <summary>
        /// 關閉說明書、恢復來源畫面，並視開啟前狀態繼續倒數。
        /// </summary>
        public void CloseManual()
        {
            manualPanel.SetActive(false);
            settingPanel.SetActive(true);

            bool resumeTimer = shouldResumeTimer;
            shouldResumeTimer = false;

            if (resumeTimer && gameTimer != null)
            {
                // 同一場景的 GameTimer 已保留剩餘秒數，不需要建立另一份時間狀態。
                gameTimer.ResumeTimer(gameTimer.GetRemainingSeconds());
            }
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
