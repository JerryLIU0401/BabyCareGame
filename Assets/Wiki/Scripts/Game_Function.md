# Game/Function 腳本文檔

對應資料夾：`Assets/Scripts/Game/Function`

## `SelectPlayerCount`

- 路徑：`Assets/Scripts/Game/Function/SelectPlayerCount.cs`
- 命名空間：`Function.Select`
- 類型：`MonoBehaviour`
- 主要職責：在設定面板中選擇玩家人數，並透過事件通知 `GameManager`。

### 主要欄位

| 欄位 | 說明 |
| --- | --- |
| `count` | 目前選擇的人數。 |
| `minCount` | 最少玩家數，預設 2。 |
| `maxCount` | 最多玩家數，目前為 6。 |
| `countText` | 顯示人數的文字。 |
| `rightBtnImage` / `leftBtnImage` | 左右按鈕圖片。 |

### 主要事件與方法

| 成員 | 說明 |
| --- | --- |
| `OnPlayerCountConfirmed` | 確認人數後傳出玩家數。 |
| `AddPlayer()` | 增加玩家數。 |
| `RemovePlayer()` | 減少玩家數。 |
| `ConfirmPlayerCount()` | 觸發人數確認事件。 |
| `UpdateBtnImage(int number)` | 根據目前人數切換按鈕狀態。 |

### 企劃對應狀態

| 項目 | 狀態 | 說明 |
| --- | --- | --- |
| 選擇玩家人數 | 目前已實作 | 可透過 UI 選擇人數。 |
| 2-4 人限制 | 部分實作 | 最小值符合，最大值目前為 6，不符合企劃。 |

## `SelectGameUnit`

- 路徑：`Assets/Scripts/Game/Function/SelectGameUnit.cs`
- 命名空間：`Function.Select`
- 類型：`MonoBehaviour`
- 主要職責：選擇是否開啟教學內容，並透過事件通知 `GameManager`。

### 主要欄位

| 欄位 | 說明 |
| --- | --- |
| `unit` | 目前選擇的索引。 |
| `units` | 可選擇文字，例如開啟或關閉。 |
| `unitText` | 顯示目前選項。 |
| `rightBtnImage` / `leftBtnImage` | 左右按鈕圖片。 |

### 主要事件與方法

| 成員 | 說明 |
| --- | --- |
| `OnComfirmPlayTech` | 傳出是否開啟教學。 |
| `NextUnit()` | 切換下一個選項。 |
| `PreviousUnit()` | 切換上一個選項。 |
| `ComfirmPlayUnit()` | 根據文字是否為「開啟」傳出布林值。 |

### 企劃對應狀態

| 項目 | 狀態 | 說明 |
| --- | --- | --- |
| 教學或說明流程 | 部分實作 | 可開啟教學面板，但企劃中的完整規則教學與 AR 玩法說明仍需確認。 |

### 注意事項

- `ComfirmPlayUnit` 依 `unitText.text == "開啟"` 判斷，若 UI 文字改變會影響邏輯；後續可改以索引或 enum 判斷。
