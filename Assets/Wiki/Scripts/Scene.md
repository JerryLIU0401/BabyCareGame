# Scene 腳本文檔

對應資料夾：`Assets/Scripts/Scene`

## `SwitchScene`

- 路徑：`Assets/Scripts/Scene/SwitchScene.cs`
- 命名空間：無
- 類型：`MonoBehaviour`
- 主要職責：提供跨場景淡入淡出與場景載入流程。

### 主要欄位

| 欄位 | 說明 |
| --- | --- |
| `canvasGroup` | 控制轉場畫面的透明度。 |
| `fadeInDuration` | 淡入時間。 |
| `fadeOutDuration` | 淡出時間。 |

### 主要方法

| 方法 | 說明 |
| --- | --- |
| `Awake()` | 取得 `CanvasGroup` 並將轉場物件設為 `DontDestroyOnLoad`。 |
| `loadFadeOutInScenes(string sceneName)` | 先淡出、載入場景，再淡入。 |
| `FadeOutInScenes()` | 只執行淡出與淡入，不切換場景。 |
| `FadeOut(float time)` | 將 `canvasGroup.alpha` 增加至 1。 |
| `FadeIn(float time)` | 將 `canvasGroup.alpha` 降至 0，完成後銷毀轉場物件。 |
| `loadScenes(string sceneName)` | 呼叫 `SceneManager.LoadScene` 載入指定場景。 |

### 依賴關係

- `CanvasGroup`
- `UnityEngine.SceneManagement.SceneManager`
- 使用者：`UserBtnController`、`ARCardManager`、`GameResultDisplay`、`MenuController.GoGameScene`

### 企劃對應狀態

| 項目 | 狀態 | 說明 |
| --- | --- | --- |
| 場景切換 | 目前已實作 | 可支援 `Game`、`ARImageTrackingScene`、`Start` 間的轉場。 |
| AR 掃描後返回遊戲 | 目前已實作 | `ARCardManager` 會使用此腳本返回 `Game`。 |

### 注意事項

- `FadeIn` 使用 `while (canvasGroup.alpha != 0)`，浮點數比較可能造成邊界風險；若未來修改程式，建議改用 `> 0` 並在結尾強制設為 0。
- 場景名稱目前以字串傳入，若擴充場景建議集中定義常數。
