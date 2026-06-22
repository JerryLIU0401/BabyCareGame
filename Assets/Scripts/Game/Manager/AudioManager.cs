using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Manager
{
    /// <summary>
    /// 定義遊戲中可由 AudioManager 統一切換的背景音樂種類。
    /// </summary>
    public enum AudioTrack
    {
        /// <summary>
        /// 開始畫面背景音樂，僅在玩家尚未進入 Game 場景前播放。
        /// </summary>
        StartScene,

        /// <summary>
        /// 遊戲進行中的背景音樂，掃描卡牌場景會延續此音樂。
        /// </summary>
        Gameplay,

        /// <summary>
        /// 結算畫面音樂，會取代遊戲進行中的背景音樂。
        /// </summary>
        GameResult
    }

    /// <summary>
    /// 定義遊戲中可由 AudioManager 統一播放的一次性音效。
    /// </summary>
    public enum SoundEffect
    {
        /// <summary>
        /// 玩家點擊開始畫面並進入遊戲前播放的音效。
        /// </summary>
        StartGame,

        /// <summary>
        /// 玩家確認使用卡牌並套用分數時播放的音效。
        /// </summary>
        Score,

        /// <summary>
        /// 一般 UI 按鈕點擊時播放的回饋音效。
        /// </summary>
        Button
    }

    /// <summary>
    /// 統一管理遊戲背景音樂、一次性音效與音量設定。
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager instance;

        // 背景音樂與音效分離播放，避免短音效覆蓋正在循環的音樂。
        private AudioSource musicSource;
        private AudioSource soundEffectSource;

        // 使用旗標記錄目前音樂，讓掃描場景返回 Game 時不會重播同一首背景音樂。
        private AudioTrack currentTrack;
        private bool hasCurrentTrack;

        [Header("音樂素材")]
        // 開始畫面音樂獨立於開始點擊音效，避免短音效被誤用為循環背景。
        [SerializeField] private AudioClip startSceneMusic;
        // 開始遊戲音效需要跨過轉場淡出，因此由 DontDestroyOnLoad 的管理器播放。
        [SerializeField] private AudioClip startGameEffect;
        // 遊戲音樂會跨 Game 與 AR 場景延續，讓掃描流程不打斷局內節奏。
        [SerializeField] private AudioClip gameplayMusic;
        // 結算音樂會取代遊戲音樂，讓玩家清楚感知遊戲已進入結果階段。
        [SerializeField] private AudioClip gameResultMusic;
        // 分數音效會在回到 Game 場景前播放，因此必須由跨場景物件承接。
        [SerializeField] private AudioClip scoreEffect;
        // 一般按鈕音效集中在 AudioManager，避免每個 Button 都需要各自綁 AudioClip。
        [SerializeField] private AudioClip buttonEffect;

        [Header("音量設定")]
        // 主音量保留全域控制，日後接設定 UI 時不需要分別改動每個來源。
        [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
        // 音樂音量獨立調整，避免背景音樂壓過教學或卡牌音效。
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.6f;
        // 音效音量獨立調整，讓按鈕與計分回饋可以比背景音樂更清楚。
        [SerializeField, Range(0f, 1f)] private float soundEffectVolume = 1f;

        /// <summary>
        /// 取得目前跨場景保留的 AudioManager 實例。
        /// </summary>
        /// <returns>回傳目前有效的 AudioManager；若場景尚未建立則回傳 null。</returns>
        public static AudioManager Instance
        {
            get { return instance; }
        }

        /// <summary>
        /// 建立跨場景音效管理器，並避免每個場景中的備援物件重複播放音樂。
        /// </summary>
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                // 三個主要場景都會放置 AudioManager 方便直接測試，但正式流程只保留第一個實例。
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            // AudioManager 會跨場景保留，因此只由有效實例監聽場景載入並替新場景按鈕補音效。
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            EnsureAudioSources();
            ApplyVolumeSettings();
        }

        /// <summary>
        /// 在場景物件完成 Awake 後替目前場景既有按鈕補上音效元件。
        /// </summary>
        private void Start()
        {
            // 初始場景不一定會觸發 sceneLoaded，因此 Start 需要補掃一次目前已載入的按鈕。
            RegisterButtonsInLoadedScenes();
        }

        /// <summary>
        /// 當管理器被銷毀時清除 Singleton 參考，避免下一次回到開始畫面時拿到失效物件。
        /// </summary>
        private void OnDestroy()
        {
            if (instance == this)
            {
                // 解除場景事件可避免回開始畫面後舊管理器仍嘗試處理新場景按鈕。
                SceneManager.sceneLoaded -= HandleSceneLoaded;
                instance = null;
            }
        }

        /// <summary>
        /// 在 Inspector 修改音量時立即套用設定，讓設計端測試音量不用重新進場。
        /// </summary>
        private void OnValidate()
        {
            masterVolume = Mathf.Clamp01(masterVolume);
            musicVolume = Mathf.Clamp01(musicVolume);
            soundEffectVolume = Mathf.Clamp01(soundEffectVolume);

            if (Application.isPlaying)
            {
                ApplyVolumeSettings();
            }
        }

        /// <summary>
        /// 嘗試透過目前實例播放指定背景音樂。
        /// </summary>
        /// <param name="audioTrack">要播放的背景音樂種類，型別為 AudioTrack。</param>
        /// <returns>若成功找到 AudioManager 並送出播放請求則回傳 true；否則回傳 false。</returns>
        public static bool TryPlayMusic(AudioTrack audioTrack)
        {
            if (instance == null)
            {
                Debug.LogWarning("[AudioManager] 場景中找不到 AudioManager，無法播放背景音樂。");
                return false;
            }

            instance.PlayMusic(audioTrack);
            return true;
        }

        /// <summary>
        /// 嘗試透過目前實例播放指定一次性音效。
        /// </summary>
        /// <param name="soundEffect">要播放的一次性音效種類，型別為 SoundEffect。</param>
        /// <returns>若成功找到 AudioManager 並送出播放請求則回傳 true；否則回傳 false。</returns>
        public static bool TryPlaySoundEffect(SoundEffect soundEffect)
        {
            if (instance == null)
            {
                Debug.LogWarning("[AudioManager] 場景中找不到 AudioManager，無法播放音效。");
                return false;
            }

            instance.PlaySoundEffect(soundEffect);
            return true;
        }

        /// <summary>
        /// 嘗試替指定物件底下的所有 Button 補上一般按鈕音效。
        /// </summary>
        /// <param name="root">要掃描的根物件，型別為 GameObject。</param>
        /// <returns>若成功找到 AudioManager 並完成註冊則回傳 true；否則回傳 false。</returns>
        public static bool TryRegisterButtonsInChildren(GameObject root)
        {
            if (instance == null)
            {
                Debug.LogWarning("[AudioManager] 場景中找不到 AudioManager，無法註冊按鈕音效。");
                return false;
            }

            instance.RegisterButtonsInChildren(root);
            return true;
        }

        /// <summary>
        /// 播放指定背景音樂，並在同一首仍在播放時避免重新開始。
        /// </summary>
        /// <param name="audioTrack">要播放的背景音樂種類，型別為 AudioTrack。</param>
        public void PlayMusic(AudioTrack audioTrack)
        {
            EnsureAudioSources();

            AudioClip clip = GetMusicClip(audioTrack);
            if (clip == null)
            {
                // 音檔綁定錯誤時只略過播放，避免遊戲流程因非核心資源缺漏而中斷。
                Debug.LogWarning($"[AudioManager] 尚未綁定背景音樂：{audioTrack}", this);
                return;
            }

            if (hasCurrentTrack && currentTrack == audioTrack && musicSource.clip == clip && musicSource.isPlaying)
            {
                // 掃描卡牌返回 Game 場景時仍是同一首音樂，不重播可保留連續聽感。
                ApplyVolumeSettings();
                return;
            }

            currentTrack = audioTrack;
            hasCurrentTrack = true;
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }

        /// <summary>
        /// 停止目前背景音樂，供未來需要完全靜音的流程使用。
        /// </summary>
        public void StopMusic()
        {
            EnsureAudioSources();

            // 停止後清空目前曲目，下一次進入場景時可以正常重新播放指定音樂。
            musicSource.Stop();
            musicSource.clip = null;
            hasCurrentTrack = false;
        }

        /// <summary>
        /// 播放指定一次性音效，並保留目前背景音樂。
        /// </summary>
        /// <param name="soundEffect">要播放的一次性音效種類，型別為 SoundEffect。</param>
        public void PlaySoundEffect(SoundEffect soundEffect)
        {
            EnsureAudioSources();

            AudioClip clip = GetSoundEffectClip(soundEffect);
            if (clip == null)
            {
                // 音效缺漏只影響操作回饋，不應阻擋開始遊戲或計分流程。
                Debug.LogWarning($"[AudioManager] 尚未綁定音效：{soundEffect}", this);
                return;
            }

            soundEffectSource.PlayOneShot(clip);
        }

        /// <summary>
        /// 替目前已載入場景中的所有 Button 補上一般按鈕音效元件。
        /// </summary>
        public void RegisterButtonsInLoadedScenes()
        {
            Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (Button button in buttons)
            {
                RegisterButton(button);
            }
        }

        /// <summary>
        /// 替指定物件底下的所有 Button 補上一般按鈕音效元件。
        /// </summary>
        /// <param name="root">要掃描的根物件，型別為 GameObject。</param>
        public void RegisterButtonsInChildren(GameObject root)
        {
            if (root == null)
            {
                // 動態 Prefab 建立失敗時不應因音效補註冊中斷主要 UI 流程。
                return;
            }

            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            foreach (Button button in buttons)
            {
                RegisterButton(button);
            }
        }

        /// <summary>
        /// 接收 Unity 場景載入事件，替新場景內的 Button 補上一般按鈕音效。
        /// </summary>
        /// <param name="scene">剛載入完成的場景，型別為 Scene。</param>
        /// <param name="loadSceneMode">場景載入模式，型別為 LoadSceneMode。</param>
        private void HandleSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            // 場景內 Button 可能分散在多個 Canvas 或 inactive 面板中，因此統一由 AudioManager 做一次全域掃描。
            RegisterButtonsInLoadedScenes();
        }

        /// <summary>
        /// 替單一 Button 補上 ButtonClickSoundPlayer，並避免重複加入元件。
        /// </summary>
        /// <param name="button">要註冊音效的 Button，型別為 Button。</param>
        private void RegisterButton(Button button)
        {
            if (button == null || button.GetComponent<ButtonClickSoundPlayer>() != null)
            {
                // 已註冊過的按鈕不重複加元件，避免同一次點擊播放多次音效。
                return;
            }

            button.gameObject.AddComponent<ButtonClickSoundPlayer>();
        }

        /// <summary>
        /// 設定全域主音量，會同時影響背景音樂與音效。
        /// </summary>
        /// <param name="volume">音量值，型別為 float，範圍會被限制在 0 到 1。</param>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }

        /// <summary>
        /// 設定背景音樂音量，不影響一次性音效。
        /// </summary>
        /// <param name="volume">音量值，型別為 float，範圍會被限制在 0 到 1。</param>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }

        /// <summary>
        /// 設定一次性音效音量，不影響背景音樂。
        /// </summary>
        /// <param name="volume">音量值，型別為 float，範圍會被限制在 0 到 1。</param>
        public void SetSoundEffectVolume(float volume)
        {
            soundEffectVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }

        /// <summary>
        /// 確保播放用 AudioSource 存在，支援場景物件只掛 AudioManager 腳本即可運作。
        /// </summary>
        private void EnsureAudioSources()
        {
            if (musicSource == null)
            {
                musicSource = CreateAudioSource(true);
            }

            if (soundEffectSource == null)
            {
                soundEffectSource = CreateAudioSource(false);
            }
        }

        /// <summary>
        /// 建立 2D AudioSource，確保音樂與音效不受場景中 Camera 位置影響。
        /// </summary>
        /// <param name="shouldLoop">是否循環播放，型別為 bool。</param>
        /// <returns>回傳新建立的 AudioSource。</returns>
        private AudioSource CreateAudioSource(bool shouldLoop)
        {
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();

            // 使用 2D 聲音可避免 AR Camera 或場景相機移動時改變背景音樂音量。
            audioSource.spatialBlend = 0f;
            audioSource.playOnAwake = false;
            audioSource.loop = shouldLoop;
            return audioSource;
        }

        /// <summary>
        /// 依目前音量設定同步兩個 AudioSource，讓 Inspector 與未來設定 UI 使用同一套規則。
        /// </summary>
        private void ApplyVolumeSettings()
        {
            EnsureAudioSources();

            musicSource.volume = masterVolume * musicVolume;
            soundEffectSource.volume = masterVolume * soundEffectVolume;
        }

        /// <summary>
        /// 依音樂種類取得對應 AudioClip。
        /// </summary>
        /// <param name="audioTrack">音樂種類，型別為 AudioTrack。</param>
        /// <returns>回傳對應的 AudioClip；若未支援則回傳 null。</returns>
        private AudioClip GetMusicClip(AudioTrack audioTrack)
        {
            switch (audioTrack)
            {
                case AudioTrack.StartScene:
                    return startSceneMusic;
                case AudioTrack.Gameplay:
                    return gameplayMusic;
                case AudioTrack.GameResult:
                    return gameResultMusic;
                default:
                    return null;
            }
        }

        /// <summary>
        /// 依音效種類取得對應 AudioClip。
        /// </summary>
        /// <param name="soundEffect">音效種類，型別為 SoundEffect。</param>
        /// <returns>回傳對應的 AudioClip；若未支援則回傳 null。</returns>
        private AudioClip GetSoundEffectClip(SoundEffect soundEffect)
        {
            switch (soundEffect)
            {
                case SoundEffect.StartGame:
                    return startGameEffect;
                case SoundEffect.Score:
                    return scoreEffect;
                case SoundEffect.Button:
                    return buttonEffect;
                default:
                    return null;
            }
        }
    }
}
