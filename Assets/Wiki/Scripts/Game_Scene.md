# Game/Scene 腳本文檔

對應資料夾：`Assets/Scripts/Game/Scene`

## `SceneController`

- 路徑：`Assets/Scripts/Game/Scene/SceneController.cs`
- 命名空間：`Manager`
- 類型：`MonoBehaviour`
- 主要職責：提供載入 `Game` 場景的簡單入口。

### 主要方法

| 方法 | 說明 |
| --- | --- |
| `GameStart()` | 呼叫 `SceneManager.LoadScene("Game")`。 |

### 企劃對應狀態

| 項目 | 狀態 | 說明 |
| --- | --- | --- |
| 開始遊戲 | 目前已實作 | 可載入 `Game` 場景。 |
| 倒數啟動 | 尚未實作 | 企劃要求開始遊戲後啟動 20 分鐘倒數。 |

### 注意事項

- 此腳本與 `MenuController` 都能進入 `Game` 場景，後續可確認是否需要保留兩者。
