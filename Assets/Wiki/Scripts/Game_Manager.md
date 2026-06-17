# Game/Manager 腳本文檔

對應資料夾：`Assets/Scripts/Game/Manager`

## `GameManager`

- 路徑：`Assets/Scripts/Game/Manager/GameManager.cs`
- 命名空間：`Manager`
- 類型：`MonoBehaviour`
- 主要職責：管理玩家資料、遊戲資料、事件發布、勝利判定與教學面板建立。

### 主要欄位

| 欄位 | 說明 |
| --- | --- |
| `playersData` | 保存所有玩家資料。 |
| `gameData` | 保存目前遊戲資料，目前使用清單但實際只取第 0 筆。 |
| `settingPanel` | 初始設定面板 Prefab。 |
| `techPanel` | 教學面板 Prefab。 |
| `playerSprites` | 玩家頭像陣列。 |

### 主要事件

| 事件 | 說明 |
| --- | --- |
| `OnPlayerDataGenerated` | 玩家資料建立完成後通知 UI。 |
| `ChangePlayerUI` | 目前玩家改變時通知 UI。 |
| `OnGameVictory` | 遊戲結束時傳出玩家資料。 |

### 主要方法

| 方法 | 說明 |
| --- | --- |
| `Awake()` | 建立設定面板、尋找選擇元件、維持單例生命週期。 |
| `OnEnable()` / `OnDisable()` | 訂閱與取消設定面板事件。 |
| `Update()` | 若回到 `Start` 場景，銷毀自身。 |
| `ChosePlayerBtn(int index)` | 選擇目前玩家，顯示過場文字並觸發 UI 更新。 |
| `GameVictory(int index)` | 判斷玩家分數是否達到 30 分。 |
| `TriggerGameVictory()` | 開啟結算面板並觸發勝利事件。 |
| `GeneratePlayerData(int playerCount)` | 依玩家數生成 `PlayerData` 與 `GameData`。 |
| `GetPlayerData()` | 取得目前玩家資料。 |
| `GetAllPlayerData()` | 取得所有玩家資料。 |
| `UpdatePlayerData(PlayerData currentPlayerData)` | 更新目前玩家資料。 |
| `UpdateAllPlayerData(List<PlayerData> currentPlayerDatas)` | 更新全部玩家資料。 |
| `GameDataGameTechPanel(bool isOpen)` | 設定是否開啟教學面板。 |
| `GetGameData()` / `UpdateGameData(GameData newGameData)` | 讀寫目前遊戲資料。 |

### 企劃對應狀態

| 項目 | 狀態 | 說明 |
| --- | --- | --- |
| 玩家資料建立 | 目前已實作 | 可依玩家數建立名稱、頭像、分數與掃描次數。 |
| 回合目前玩家 | 目前已實作 | 透過 `currentPlayer` 記錄目前玩家。 |
| 勝利條件 | 部分實作 | 目前固定 30 分，不符合企劃依玩家數變動。 |
| 牌庫與手牌 | 尚未實作 | 未保存中央牌庫、玩家手牌與棄牌區。 |
| 情境卡進度 | 尚未實作 | 未保存場上 4 張情境卡與步驟填入狀態。 |
| 20 分鐘倒數 | 尚未實作 | 未保存或觸發時間限制。 |

### 注意事項

- `GameManager` 目前承擔多種責任，後續實作企劃卡牌規則時建議拆分服務。
- `OnEnable()` 假設 `selectPlayerCount` 與 `selectGameUnit` 一定存在；若場景物件生命週期改變，需要增加空值保護。
- `GetPlayerData()` 已保護 `currentPlayer <= 0`，呼叫端仍應處理回傳 `null`。

## `PlayerUIManager`

- 路徑：`Assets/Scripts/Game/Manager/PlayerUIManager.cs`
- 命名空間：`Manager`
- 類型：`MonoBehaviour`
- 主要職責：生成玩家 UI、綁定玩家按鈕、更新目前玩家顯示與開啟結算面板。

### 主要欄位

| 欄位 | 說明 |
| --- | --- |
| `playerUIPrefab` | 玩家 UI Prefab。 |
| `uiParent` | 玩家 UI 的父物件。 |
| `playerUIObjects` | 已生成玩家 UI 物件清單。 |
| `bgSprites` | 玩家背景圖。 |
| `playerSprites` | 玩家頭像圖。 |
| `gameOverPanel` | 結算面板。 |

### 主要方法

| 方法 | 說明 |
| --- | --- |
| `Start()` | 回到 `Game` 場景時讀取既有玩家資料並重建 UI。 |
| `GeneratePlayerUI(List<PlayerData> players)` | 依玩家數建立 UI、設定頭像、背景、分數與按鈕事件。 |
| `DisplayCurrentPlayer(GameData gameData)` | 顯示目前玩家標記並更新分數。 |
| `OpenGameResultPanel()` | 開啟結算面板。 |

### 企劃對應狀態

| 項目 | 狀態 | 說明 |
| --- | --- | --- |
| 玩家 UI | 目前已實作 | 可依玩家數生成 UI。 |
| 目前回合顯示 | 目前已實作 | 可顯示第幾位玩家回合。 |
| 玩家手牌 UI | 尚未實作 | 企劃需要手牌流程，目前沒有手牌顯示。 |
| 情境卡 UI | 尚未實作 | 企劃要求場上 4 張情境卡，目前未管理。 |

### 注意事項

- 程式依賴 Prefab 子路徑 `"DispalyUI"`，修改前必須同步檢查 Prefab。
- `Start()` 中直接使用 `gameManager.GetPlayerData().score`，若回場景時尚未選玩家可能有空值風險。

## `TransitionManager`

- 路徑：`Assets/Scripts/Game/Manager/TransitionManager.cs`
- 命名空間：`Manager`
- 類型：`MonoBehaviour`
- 主要職責：顯示滑入滑出的文字過場，例如提示目前玩家開始。

### 主要欄位

| 欄位 | 說明 |
| --- | --- |
| `panelTransform` | 過場面板的位置控制。 |
| `canvasGroup` | 過場面板透明度。 |
| `messageText` | 顯示訊息文字。 |
| `slideDuration` | 滑入滑出時間。 |
| `displayDuration` | 停留時間。 |
| `slideCurve` | 平滑動畫曲線。 |

### 主要方法

| 方法 | 說明 |
| --- | --- |
| `ShowTransition(string message)` | 停止舊動畫並播放新的過場訊息。 |
| `PlayTransition(string message)` | 執行滑入、停留、滑出流程。 |
| `SlideAndFade(...)` | 同步處理位置與透明度插值。 |

### 企劃對應狀態

| 項目 | 狀態 | 說明 |
| --- | --- | --- |
| 回合提示 | 目前已實作 | 可提示某位玩家開始。 |
| 回合階段提示 | 部分實作 | 尚未提示抽牌、出牌、PASS、補牌等企劃階段。 |

## `UserBtnController`

- 路徑：`Assets/Scripts/Game/Manager/UserBtnController.cs`
- 命名空間：`Manager`
- 類型：`MonoBehaviour`
- 主要職責：控制遊戲中操作按鈕，例如開啟轉盤、進入 AR 掃描與觸發結算。

### 主要欄位

| 欄位 | 說明 |
| --- | --- |
| `spinPanelButton` | 轉盤按鈕。 |
| `resultBtn` | 結算按鈕。 |
| `scanButton` | 掃描按鈕。 |
| `spinPanel` | 轉盤面板。 |
| `switchScenePrefab` | 場景轉場 Prefab。 |

### 主要方法

| 方法 | 說明 |
| --- | --- |
| `Start()` | 綁定結算按鈕到 `GameManager.TriggerGameVictory`。 |
| `OpenSpin()` | 若已選玩家，開啟轉盤面板。 |
| `ScanCardBtn()` | 若已選玩家，停用掃描按鈕並進入 AR 場景。 |
| `GoARScene()` | 透過 `SwitchScene` 載入 `ARImageTrackingScene`。 |

### 企劃對應狀態

| 項目 | 狀態 | 說明 |
| --- | --- | --- |
| 掃描按鈕 | 目前已實作 | 可進入 AR 場景。 |
| 出牌/PASS | 尚未實作 | 企劃要求行動選擇，目前沒有對應按鈕邏輯。 |
| 抽牌/補牌/棄牌 | 尚未實作 | 企劃中的牌庫流程尚未出現。 |
| 結算按鈕 | 部分實作 | 可手動觸發結算，但企劃應由勝利或時間條件觸發。 |

### 注意事項

- `OpenSpin()` 與 `ScanCardBtn()` 都會保護 `currentPlayer == 0`，這是避免未選玩家操作的重要邊界。
