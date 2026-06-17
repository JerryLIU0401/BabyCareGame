# Game/UI 腳本文檔

對應資料夾：`Assets/Scripts/Game/UI`

## `TeachPanelControl`

- 路徑：`Assets/Scripts/Game/UI/TeachPanelControl.cs`
- 類型：`MonoBehaviour`
- 主要職責：控制教學圖片切換與左右按鈕狀態。

### 主要欄位

| 欄位 | 說明 |
| --- | --- |
| `index` | 目前教學圖片索引。 |
| `teachSprites` | 教學圖片陣列。 |
| `displayTechSprite` | 顯示目前教學圖片的 Image。 |
| `rightBtnImage` / `leftBtnImage` | 左右按鈕圖。 |

### 主要方法

| 方法 | 說明 |
| --- | --- |
| `Start()` | 顯示第一張教學圖片並更新按鈕狀態。 |
| `NextImage()` | 切換下一張教學圖片。 |
| `PreviousImage()` | 切換上一張教學圖片。 |
| `UpdateBtnImage(int currentUnit)` | 依索引切換按鈕可用視覺狀態。 |

### 企劃對應狀態

| 項目 | 狀態 | 說明 |
| --- | --- | --- |
| 教學說明 | 部分實作 | 可播放圖片式教學，但內容是否符合新企劃需檢查 Sprite。 |

## `PlayerGridLayoutAdjust`

- 路徑：`Assets/Scripts/Game/UI/PlayerGridLayoutAdjust.cs`
- 命名空間：`UI`
- 類型：`MonoBehaviour`
- 主要職責：依玩家數調整玩家 UI 的 Grid Layout 間距與大小。

### 主要欄位

| 欄位 | 說明 |
| --- | --- |
| `gridLayoutGroup` | 要調整的 Grid Layout Group。 |
| `minSpacing` / `maxSpacing` | 最小與最大間距。 |
| `minCellSize` / `maxCellSize` | 最小與最大格子大小。 |
| `maxPlayers` | 用於正規化計算的最大玩家數。 |

### 主要方法

| 方法 | 說明 |
| --- | --- |
| `AdjustGrid(int playerCount)` | 依玩家數插值計算間距與格子大小。 |

### 企劃對應狀態

| 項目 | 狀態 | 說明 |
| --- | --- | --- |
| 玩家 UI 版面 | 目前已實作 | 可依玩家數調整。 |
| 2-4 人版面 | 部分實作 | 目前 `maxPlayers` 為 6，若依企劃改為 4 人需同步調整。 |

## `GameResultDisplay`

- 路徑：`Assets/Scripts/Game/UI/GameResultDisplay.cs`
- 類型：`MonoBehaviour`
- 主要職責：接收勝利事件後排序玩家並顯示結算結果。

### 主要欄位

| 欄位 | 說明 |
| --- | --- |
| `gameManager` | 勝利事件來源。 |
| `numberBlock` | 排名區塊 UI 陣列。 |
| `playerImages` | 顯示玩家頭像的 Image 陣列。 |
| `switchScenePrefab` | 返回開始畫面的轉場 Prefab。 |

### 主要方法

| 方法 | 說明 |
| --- | --- |
| `OnEnable()` / `OnDisable()` | 訂閱與取消 `OnGameVictory`。 |
| `GameResultUIDisplay(List<PlayerData> players)` | 依分數排序玩家並顯示排名圖像。 |
| `BackToMenu()` | 透過 `SwitchScene` 返回 `Start` 場景。 |

### 企劃對應狀態

| 項目 | 狀態 | 說明 |
| --- | --- | --- |
| 結算排序 | 目前已實作 | 可依分數排序玩家。 |
| 顯示最高分贏家 | 部分實作 | 顯示頭像，但未看到分數或勝利原因文字。 |
| 時間到結算 | 尚未實作 | 尚無倒數時間事件。 |
| 10 張情境完成結算 | 尚未實作 | 尚無情境卡完成計數。 |

### 注意事項

- `players.Sort` 會直接改變傳入清單順序，也就是會影響 `GameManager` 保存的玩家資料順序；若後續仍需要原玩家順序，應改用複製清單排序。
