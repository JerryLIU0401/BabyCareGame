# UI 流程文件

本文件整理目前 UI 腳本責任，以及它們與遊戲流程、企劃需求之間的關係。

## 目前 UI 組成

| UI 區塊 | 相關腳本 | 說明 |
| --- | --- | --- |
| 開始畫面 | `MenuController`、`TitleBlink` | 偵測觸控進入遊戲，並顯示閃爍提示文字。 |
| 遊戲設定 | `SelectPlayerCount`、`SelectGameUnit` | 選擇玩家人數與是否開啟教學。 |
| 玩家資訊 | `PlayerUIManager`、`PlayerController`、`PlayerGridLayoutAdjust` | 生成玩家 UI、顯示目前玩家與分數。 |
| 教學面板 | `TeachPanelControl` | 切換教學圖片與左右按鈕狀態。 |
| 遊戲操作按鈕 | `UserBtnController` | 控制轉盤、掃描與結算按鈕。 |
| 轉盤 | `SpinFateManager`、`SpinOption` | 顯示加權選項、圖例與指針動畫。 |
| 過場提示 | `TransitionManager`、`SwitchScene` | 顯示玩家回合提示與場景淡入淡出。 |
| 結算畫面 | `GameResultDisplay` | 依分數排序玩家並顯示結果。 |

## UI 事件資料流

1. `SelectPlayerCount` 觸發 `OnPlayerCountConfirmed`。
2. `GameManager` 建立玩家資料並觸發 `OnPlayerDataGenerated`。
3. `PlayerUIManager` 生成玩家 UI。
4. 玩家點擊玩家 UI 後呼叫 `GameManager.ChosePlayerBtn`。
5. `GameManager` 觸發 `ChangePlayerUI`。
6. `PlayerUIManager` 更新目前玩家顯示。
7. `PlayerController` 更新目前玩家分數文字。
8. 結束時 `GameManager.OnGameVictory` 通知 `GameResultDisplay` 顯示結果。

## 與企劃差異

| UI 需求 | 目前狀態 | 差異 |
| --- | --- | --- |
| 選角順位 | 可點擊玩家 UI 切換目前玩家 | 企劃是猜拳決定順序並依序選角。 |
| 20 分鐘倒數 | 尚未看到倒數 UI | 需要新增倒數顯示與時間到結算。 |
| 手牌顯示 | 尚未看到手牌 UI | 企劃需要顯示或至少管理玩家手牌。 |
| 情境卡場面 | 尚未看到情境卡 UI | 企劃要求場上維持 4 張情境卡。 |
| 出牌/PASS | 目前主要是掃卡與轉盤按鈕 | 需要新增出牌、PASS、補牌、棄牌流程 UI。 |
| AR 完成展示 | 可進入 AR 掃描 | 需要和完成情境流程整合。 |

## UI 維護注意事項

- `PlayerUIManager` 使用 `"DispalyUI"` 子物件路徑，拼字雖然疑似錯誤，但需與 Prefab 同步檢查後才能修改。
- `GameManager.GetPlayerData()` 在 `currentPlayer == 0` 時會回傳 `null`，UI 呼叫前需保護未選玩家狀態。
- `SelectPlayerCount` 目前允許到 6 人，若依企劃改成 4 人，需要同步檢查玩家頭像、背景與 Grid Layout。
- 結算 UI 目前以玩家圖片為主，若要顯示分數、排名文字或最高分玩家，需要擴充 `GameResultDisplay`。
