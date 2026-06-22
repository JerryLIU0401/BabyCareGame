using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Manager
{
    /// <summary>
    /// 讓單一 Unity Button 在被點擊時播放共用按鈕音效。
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ButtonClickSoundPlayer : MonoBehaviour, IPointerDownHandler, ISubmitHandler
    {
        // 快取 Button 參考，避免每次點擊都重新查找元件。
        private Button targetButton;

        /// <summary>
        /// 初始化按鈕參考，讓後續啟用流程可以安全訂閱點擊事件。
        /// </summary>
        private void Awake()
        {
            targetButton = GetComponent<Button>();
        }

        /// <summary>
        /// 接收滑鼠或觸控按下事件，並在 Button 原本 onClick 流程前播放音效。
        /// </summary>
        /// <param name="eventData">本次指標事件資料，型別為 PointerEventData。</param>
        public void OnPointerDown(PointerEventData eventData)
        {
            // 按下瞬間播放可避免按鈕接著切換場景時，onClick listener 尚未執行就被場景卸載。
            PlayButtonClickSound();
        }

        /// <summary>
        /// 接收鍵盤或控制器提交事件，讓非觸控操作也能得到按鈕音效。
        /// </summary>
        /// <param name="eventData">本次提交事件資料，型別為 BaseEventData。</param>
        public void OnSubmit(BaseEventData eventData)
        {
            // Submit 不一定會經過 PointerDown，因此保留一條鍵盤與控制器路徑。
            PlayButtonClickSound();
        }

        /// <summary>
        /// 播放一般按鈕音效。
        /// </summary>
        private void PlayButtonClickSound()
        {
            if (targetButton == null)
            {
                // 支援由 AudioManager 動態 AddComponent 的情境，確保 Awake 時序異常也能補抓。
                targetButton = GetComponent<Button>();
            }

            if (targetButton == null)
            {
                // RequireComponent 理論上會保證存在，保留防護讓 Prefab 損壞時不影響主要按鈕行為。
                return;
            }

            if (!targetButton.IsInteractable())
            {
                // 不可互動按鈕不應產生點擊回饋，避免玩家誤以為操作有效。
                return;
            }

            AudioManager.TryPlaySoundEffect(SoundEffect.Button);
        }
    }
}
