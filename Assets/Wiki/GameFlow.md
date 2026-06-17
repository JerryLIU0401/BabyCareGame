# 遊戲流程文件

本文件說明目前腳本流程與企劃文件流程的差異，協助後續開發判斷要新增哪些系統。

## 目前腳本流程

1. `Start` 場景啟動。
2. `MenuController` 偵測觸控結束後載入 `Game` 場景。
3. `GameManager` 在 `Game` 場景中建立並透過 `DontDestroyOnLoad` 保留。
4. 若尚未建立 `GameData`，`GameManager` 生成設定面板。
5. `SelectPlayerCount` 確認人數後觸發 `OnPlayerCountConfirmed`。
6. `GameManager.GeneratePlayerData` 建立 `PlayerData` 與 `GameData`。
7. `PlayerUIManager` 訂閱事件並生成玩家 UI。
8. 玩家點擊自己的玩家 UI 後，`GameManager.ChosePlayerBtn` 更新目前玩家。
9. `UserBtnController` 允許目前玩家開啟轉盤或進入 AR 掃描場景。
10. `ImageTrackingController` 掃描 Reference Image 並生成對應 Prefab。
11. `ARCardManager` 取得 `ModelInfo.score` 後更新目前玩家分數。
12. 返回 `Game` 場景後，UI 重新讀取 `GameManager` 的玩家資料。
13. `GameManager.TriggerGameVictory` 或分數達標時觸發結算。

## 企劃文件流程

1. 玩家猜拳決定順序並選擇角色頭像。
2. 每位玩家獲得 4 張初始手牌。
3. 將命運卡洗入中央牌庫。
4. 場上抽出 4 張情境卡，剩餘情境卡作為情境牌堆。
5. 遊戲 APP 開始 20 分鐘倒數並記錄分數。
6. 回合開始時，目前玩家抽 1 張牌。
7. 玩家選擇出 1-2 張牌或 PASS。
8. 若出步驟卡，需依情境卡色塊順序放置。
9. 若出功能卡或命運卡，直接執行卡牌效果。
10. 回合結束時依手牌數補牌或棄牌。
11. 情境卡最後步驟完成時，玩家排序並朗讀解決方法。
12. 使用平板掃描情境卡與步驟卡，播放完整急救 AR 動畫。
13. 完成情境的玩家依步驟數得分，並補上新的情境卡。
14. 達到玩家人數對應分數、時間到或 10 張情境卡完成時結束遊戲。

## 主要流程差異

| 流程區塊 | 目前腳本 | 企劃要求 |
| --- | --- | --- |
| 起始順位 | 玩家點擊 UI 選擇目前玩家 | 猜拳決定順序，依序選角 |
| 手牌 | 尚未實作 | 每位玩家持有手牌，需抽牌、出牌、補牌、棄牌 |
| 情境卡 | 尚未實作 | 場上維持 4 張情境卡 |
| AR 掃描時機 | 玩家可直接進入 AR 掃描 | 完成情境卡後才掃描 |
| 計分 | 掃到模型後直接依 `ModelInfo.score` 加分 | 依完成情境的步驟數加分 |
| 結束條件 | 固定 30 分或手動觸發 | 依玩家人數分數、20 分鐘倒數或 10 張情境完成 |

## 建議重構方向

- 建立獨立的 `TurnService` 或相近模組管理回合階段，避免按鈕直接決定所有流程。
- 建立牌庫與手牌資料模型，讓 `PlayerData` 不再只保存分數與掃描次數。
- 建立情境卡進度管理，處理色塊順序、步驟填入與完成事件。
- 將勝利判定從 `GameManager` 拆出，支援不同玩家人數與時間限制。
- 讓 AR 掃描接收「已完成情境」資訊，而不是只讀取單一 `ModelInfo.score`。
