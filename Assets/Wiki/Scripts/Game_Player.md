# Game/Player 腳本文檔

對應資料夾：`Assets/Scripts/Game/Player`

## `PlayerController`

- 路徑：`Assets/Scripts/Game/Player/PlayerController.cs`
- 命名空間：`Player`
- 類型：`MonoBehaviour`
- 主要職責：顯示目前玩家的分數，並在玩家資料事件發生時更新 UI。

### 主要欄位

| 欄位 | 說明 |
| --- | --- |
| `gameManager` | 遊戲資料來源。 |
| `playScore` | 顯示目前玩家分數的 `TMP_Text`。 |

### 主要方法

| 方法 | 說明 |
| --- | --- |
| `Awake()` | 透過 `FindFirstObjectByType<GameManager>()` 取得管理器。 |
| `OnEnable()` | 訂閱 `ChangePlayerUI` 與 `OnPlayerDataGenerated`。 |
| `OnDisable()` | 取消事件訂閱。 |
| `InitialPlayerScore(List<PlayerData> playerData)` | 玩家資料生成時將顯示分數設為 0。 |
| `ChangUI(GameData currentGameDataData)` | 切換目前玩家時顯示該玩家分數。 |
| `EditPlayerScoreData(int score)` | 外部直接指定分數顯示。 |

### 企劃對應狀態

| 項目 | 狀態 | 說明 |
| --- | --- | --- |
| 顯示玩家分數 | 目前已實作 | 可顯示目前玩家分數。 |
| 顯示手牌數 | 尚未實作 | 企劃有手牌管理，但目前此腳本只顯示分數。 |
| 顯示回合階段 | 尚未實作 | 抽牌、出牌、PASS、補牌階段尚未有 UI 顯示。 |

### 注意事項

- `ChangUI` 直接呼叫 `gameManager.GetPlayerData().score`，若目前玩家為 0 會有空值風險；呼叫鏈需確保已選玩家。
