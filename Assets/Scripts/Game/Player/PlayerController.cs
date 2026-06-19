using UnityEngine;


namespace Player
{
    /// <summary>
    /// 保留舊版玩家分數控制器元件，避免既有場景引用遺失造成 Unity 顯示 Missing Script。
    /// </summary>
    /// <remarks>
    /// 目前玩家分數已統一交由 PlayerUIManager 顯示在各玩家區塊內；此類別不再寫入任何文字元件，
    /// 以避免舊場景綁定誤將分數輸出到倒數計時文字。
    /// </remarks>
    public class PlayerController : MonoBehaviour
    {
        /// <summary>
        /// 接收舊版分數更新呼叫，但不再更新 UI 文字。
        /// </summary>
        /// <param name="score">玩家目前分數；保留參數是為了維持既有呼叫端相容性。</param>
        /// <returns>此方法不回傳資料。</returns>
        public void EditPlayerScoreData(int score)
        {
            // 分數顯示已移至玩家卡片內，這裡刻意不處理文字，避免舊綁定覆蓋計時器。
        }
    }
}
