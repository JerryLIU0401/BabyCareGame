# Game/Data 腳本文檔

對應資料夾：`Assets/Scripts/Game/Data`

## `GameData`

- 路徑：`Assets/Scripts/Game/Data/GameData.cs`
- 類型：`[System.Serializable]` 純資料類別
- 主要職責：保存單局遊戲的基本狀態。

### 主要欄位

| 欄位 | 說明 |
| --- | --- |
| `isSetting` | 表示是否完成初始設定。 |
| `isTech` | 表示是否需要教學關卡或教學面板。 |
| `playerCount` | 玩家人數。 |
| `currentPlayer` | 目前玩家編號，使用 1-based；`0` 代表尚未選擇玩家。 |

### 建構子

| 建構子 | 說明 |
| --- | --- |
| `GameData(bool gameStart, int player, int initialPlayer)` | 初始化設定狀態、玩家人數與目前玩家。 |

### 企劃對應狀態

| 項目 | 狀態 | 說明 |
| --- | --- | --- |
| 玩家人數 | 部分實作 | 可保存人數，但目前允許 2-6 人，企劃為 2-4 人。 |
| 目前玩家 | 目前已實作 | 可表示目前回合玩家，但尚未管理完整回合階段。 |
| 20 分鐘倒數 | 尚未實作 | `GameData` 未保存剩餘時間或計時狀態。 |
| 情境卡進度 | 尚未實作 | 未保存場上情境卡、步驟進度或完成數量。 |

### 注意事項

- `currentPlayer == 0` 是重要邊界狀態，任何分數、掃描或 UI 更新前都需要防護。

## `PlayerData`

- 路徑：`Assets/Scripts/Game/Data/PlayerData.cs`
- 類型：`[System.Serializable]` 純資料類別
- 主要職責：保存單一玩家的名稱、頭像、分數與少量遊戲狀態。

### 主要欄位

| 欄位 | 說明 |
| --- | --- |
| `playerName` | 玩家顯示名稱。 |
| `playerSprite` | 玩家頭像。 |
| `score` | 玩家目前分數。 |
| `scanCount` | 掃描次數或卡牌次數預留欄位，目前使用較少。 |
| `isWin` | 是否達成勝利。 |

### 建構子

| 建構子 | 說明 |
| --- | --- |
| `PlayerData(string name, Sprite sprite, int initialScore, int scanCardCount)` | 初始化玩家名稱、頭像、分數與掃描次數。 |

### 企劃對應狀態

| 項目 | 狀態 | 說明 |
| --- | --- | --- |
| 玩家分數 | 目前已實作 | 可保存分數並由 AR 掃描加分。 |
| 玩家手牌 | 尚未實作 | 企劃要求每位玩家持有手牌，目前沒有手牌集合。 |
| 角色頭像 | 目前已實作 | 可保存玩家 Sprite。 |
| 玩家順位 | 部分實作 | 目前依玩家編號與 UI 點擊決定，沒有猜拳順位資料。 |

### 注意事項

- 若要實作企劃中的手牌，建議不要把所有卡牌規則直接塞入 `PlayerData`；可新增手牌容器或玩家狀態模型。
