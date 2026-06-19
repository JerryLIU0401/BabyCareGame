using System;
using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// 管理遊戲局內倒數計時，並將剩餘時間以固定文字格式顯示在 UI 上。
    /// </summary>
    public class GameTimer : MonoBehaviour
    {
        // 統一集中顯示前綴，避免場景文字與程式格式各自維護後出現不一致。
        private const string DisplayPrefix = "剩餘時間 : ";

        // 透過 Inspector 指定文字元件，讓場景版面可以自行決定顯示位置與樣式。
        [SerializeField] private TMP_Text timerText;

        // 使用分鐘與秒數拆分設定，讓非工程人員可直接在 Inspector 調整預設局長。
        [SerializeField] [Min(0)] private int initialMinutes = 20;
        [SerializeField] [Range(0, 59)] private int initialSeconds = 0;

        // 用秒數保存內部狀態，避免每幀換算分鐘秒數時累積格式判斷的複雜度。
        private float remainingSeconds;

        // 計時器必須等玩家完成設定或教學後才啟動，因此預設只顯示、不倒數。
        private bool isRunning;

        // 防止設定面板與教學面板連點造成重新歸零，讓同一局只啟動一次倒數。
        private bool hasStarted;

        /// <summary>
        /// 時間歸零時通知外部流程，讓計時器不直接依賴結算 UI。
        /// </summary>
        public event Action OnTimerExpired;

        // 確保時間到流程只觸發一次，避免 Update 在 00:00 時重複派送事件。
        private bool hasExpired;

        /// <summary>
        /// 取得本局倒數是否已經啟動，供跨場景流程判斷是否需要保存時間。
        /// </summary>
        public bool HasStarted => hasStarted;

        /// <summary>
        /// 取得本局倒數是否已經歸零，供回到 Game 場景時避免重複恢復已結束的時間。
        /// </summary>
        public bool HasExpired => hasExpired;

        /// <summary>
        /// 初始化文字顯示，讓玩家在倒數開始前仍能看到預設遊戲時間。
        /// </summary>
        private void Awake()
        {
            // 若場景漏綁欄位，嘗試從子物件取得文字元件，降低 UI 調整時的出錯成本。
            if (timerText == null)
            {
                timerText = GetComponentInChildren<TMP_Text>();
            }

            ResetTimerDisplay();
        }

        /// <summary>
        /// 每幀扣除剩餘秒數，並在歸零時停住顯示。
        /// </summary>
        private void Update()
        {
            if (!isRunning)
            {
                return;
            }

            // 使用 Time.deltaTime 讓倒數跟隨 Unity 遊戲時間，日後若暫停 Time.timeScale 也會一起停止。
            remainingSeconds = Mathf.Max(remainingSeconds - Time.deltaTime, 0f);
            UpdateTimerText();

            if (remainingSeconds <= 0f)
            {
                isRunning = false;
                TriggerTimerExpired();
            }
        }

        /// <summary>
        /// 從目前設定開始倒數；若本局已啟動過，會直接忽略重複呼叫。
        /// </summary>
        public void StartTimer()
        {
            if (hasStarted)
            {
                return;
            }

            hasStarted = true;
            hasExpired = false;
            remainingSeconds = GetInitialTotalSeconds();
            isRunning = remainingSeconds > 0f;
            UpdateTimerText();

            if (remainingSeconds <= 0f)
            {
                // 支援測試時把時間設為 0 秒，仍能走完整時間到流程。
                TriggerTimerExpired();
            }
        }

        /// <summary>
        /// 暫停目前倒數，讓 AR 掃描期間不消耗遊戲時間。
        /// </summary>
        /// <returns>回傳暫停當下剩餘秒數，提供跨場景管理者保存。</returns>
        public float PauseTimer()
        {
            // 掃描流程只需要暫停倒數，不應重置 hasStarted，否則回到 Game 會被誤判成新局。
            isRunning = false;
            UpdateTimerText();
            return GetRemainingSeconds();
        }

        /// <summary>
        /// 以指定剩餘秒數恢復倒數，供 Game 場景重新載入後接續原本時間。
        /// </summary>
        /// <param name="seconds">要恢復的剩餘秒數，型別為 float。</param>
        public void ResumeTimer(float seconds)
        {
            // 切回 Game 會生成新的場景 UI，因此必須用 GameManager 保存的秒數覆寫預設初始時間。
            remainingSeconds = Mathf.Max(seconds, 0f);
            hasStarted = true;
            hasExpired = false;
            isRunning = remainingSeconds > 0f;
            UpdateTimerText();

            if (remainingSeconds <= 0f)
            {
                // 若剛好在切場景邊界歸零，仍需補送時間到事件，避免結算流程被跳過。
                TriggerTimerExpired();
            }
        }

        /// <summary>
        /// 取得目前剩餘秒數。
        /// </summary>
        /// <returns>回傳不小於 0 的剩餘秒數，型別為 float。</returns>
        public float GetRemainingSeconds()
        {
            // 對外只暴露已限制的秒數，避免跨場景保存到浮點扣減後的負值。
            return Mathf.Max(remainingSeconds, 0f);
        }

        /// <summary>
        /// 停止倒數並回到 Inspector 設定的預設顯示。
        /// </summary>
        public void ResetTimerDisplay()
        {
            hasStarted = false;
            hasExpired = false;
            isRunning = false;
            remainingSeconds = GetInitialTotalSeconds();
            UpdateTimerText();
        }

        /// <summary>
        /// 觸發時間歸零事件。
        /// </summary>
        private void TriggerTimerExpired()
        {
            if (hasExpired)
            {
                return;
            }

            // 計時器只負責宣告時間到，實際結算與動畫銜接交給遊戲流程管理者。
            hasExpired = true;
            OnTimerExpired?.Invoke();
        }

        /// <summary>
        /// 依 Inspector 設定換算初始總秒數。
        /// </summary>
        /// <returns>回傳倒數起始秒數。</returns>
        private float GetInitialTotalSeconds()
        {
            // 秒數欄位已限制在 0-59，此處仍集中換算，避免其他方法自行組合時間規則。
            return Mathf.Max(0, initialMinutes * 60 + initialSeconds);
        }

        /// <summary>
        /// 將內部秒數轉換為「剩餘時間 : XX:XX」格式。
        /// </summary>
        private void UpdateTimerText()
        {
            if (timerText == null)
            {
                return;
            }

            // 使用 CeilToInt 避免開始後第一幀立刻顯示少一秒，讓玩家感受符合完整倒數秒數。
            int totalSeconds = Mathf.CeilToInt(remainingSeconds);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            timerText.text = $"{DisplayPrefix}{minutes:00}:{seconds:00}";
        }
    }
}
