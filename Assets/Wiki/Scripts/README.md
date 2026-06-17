# Scripts 工程文件索引

本目錄依照 `Assets/Scripts/` 的實際資料夾結構分類，逐一說明目前腳本責任與企劃對應狀態。

## 分類索引

| 文件 | 對應資料夾 | 說明 |
| --- | --- | --- |
| [Menu.md](Menu.md) | `Assets/Scripts/Menu` | 開始畫面與標題提示。 |
| [Scene.md](Scene.md) | `Assets/Scripts/Scene` | 共用場景轉場工具。 |
| [Game_Data.md](Game_Data.md) | `Assets/Scripts/Game/Data` | 遊戲與玩家資料模型。 |
| [Game_Manager.md](Game_Manager.md) | `Assets/Scripts/Game/Manager` | 遊戲資料核心、玩家 UI、按鈕與轉場提示。 |
| [Game_UI.md](Game_UI.md) | `Assets/Scripts/Game/UI` | 教學、玩家格線與結算 UI。 |
| [Game_Player.md](Game_Player.md) | `Assets/Scripts/Game/Player` | 目前玩家分數顯示。 |
| [Game_Function.md](Game_Function.md) | `Assets/Scripts/Game/Function` | 遊戲設定選擇流程。 |
| [Game_Card.md](Game_Card.md) | `Assets/Scripts/Game/Card` | AR 模型與卡牌資料。 |
| [Game_Scene.md](Game_Scene.md) | `Assets/Scripts/Game/Scene` | 遊戲場景進入點。 |
| [Game_Root.md](Game_Root.md) | `Assets/Scripts/Game` | `Game` 根目錄腳本，例如轉盤。 |
| [AR_ImageTracking.md](AR_ImageTracking.md) | `Assets/Scripts/AR ImageTracking` | AR 圖像追蹤與卡牌使用。 |

## 閱讀方式

- 若要理解整體遊戲流程，先讀 [../GameFlow.md](../GameFlow.md)。
- 若要理解企劃差異，先讀 [../GameDesignComparison.md](../GameDesignComparison.md)。
- 若要修改 AR 或 ImageLibrary，先讀 [../ARFlow.md](../ARFlow.md)。
- 若要修改單一腳本，讀取對應分類文件後，再檢查場景與 Prefab 綁定。
