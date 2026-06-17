# Menu 腳本文檔

對應資料夾：`Assets/Scripts/Menu`

## `MenuController`

- 路徑：`Assets/Scripts/Menu/MenuController.cs`
- 命名空間：無
- 類型：`MonoBehaviour`
- 主要職責：偵測開始畫面的觸控輸入，並載入 `Game` 場景。

### 主要方法

| 方法 | 說明 |
| --- | --- |
| `Update()` | 每幀檢查觸控數量，當第一個觸控階段為 `TouchPhase.Ended` 時載入 `Game`。 |
| `GoGameScene()` | 透過 `SwitchScene` Prefab 執行淡入淡出轉場，目前未被 `Update()` 使用。 |

### 依賴關係

- `UnityEngine.SceneManagement.SceneManager`
- `SwitchScene`
- Inspector 欄位：`switchScenePrefab`

### 企劃對應狀態

| 項目 | 狀態 | 說明 |
| --- | --- | --- |
| 開啟 APP 後進入遊戲 | 目前已實作 | 可由觸控進入 `Game` 場景。 |
| 依猜拳順位選角 | 尚未實作 | 目前只負責場景切換，沒有順位或選角流程。 |
| 20 分鐘倒數啟動 | 尚未實作 | 企劃要求點擊開始遊戲後啟動倒數，目前未看到計時器。 |

### 注意事項

- `Update()` 目前直接使用 `SceneManager.LoadScene("Game")`，因此不會走 `SwitchScene` 淡入淡出流程。
- 若未來要支援滑鼠或 Editor 測試輸入，需要補充非觸控輸入分支。

## `TitleBlink`

- 路徑：`Assets/Scripts/Menu/TitleBlink.cs`
- 命名空間：`Menu`
- 類型：`MonoBehaviour`
- 主要職責：控制開始畫面提示文字的淡入淡出閃爍效果。

### 主要方法

| 方法 | 說明 |
| --- | --- |
| `Start()` | 取得 `TMP_Text` 並啟動 `InvokeRepeating`。 |
| `ToggleTextVisibility()` | 依目前狀態啟動淡入或淡出 Coroutine。 |
| `FadeText(bool fadeIn)` | 使用 `Mathf.Lerp` 平滑調整文字透明度。 |

### 依賴關係

- `TextMeshPro.TMP_Text`
- `UnityEngine.Color`

### 企劃對應狀態

| 項目 | 狀態 | 說明 |
| --- | --- | --- |
| 開始畫面提示 | 目前已實作 | 屬於 UI 表現，不影響企劃規則。 |

### 注意事項

- `InvokeRepeating` 可能在淡入淡出尚未結束時再次啟動 Coroutine，若未來發生閃爍不穩，可改為單一循環 Coroutine。
