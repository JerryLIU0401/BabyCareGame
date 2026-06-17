# AR 與 ImageLibrary 流程文件

本文件說明目前 AR 掃描實作、新企劃中的 AR 定位，以及 `Assets/Old/` 舊資產的使用限制。

## 目前 AR 實作

| 腳本 | 職責 |
| --- | --- |
| `ImageTrackingController` | 管理 `ARTrackedImageManager`、輪替 `XRReferenceImageLibrary`、根據辨識圖名稱生成 Prefab。 |
| `ARCardManager` | 訂閱卡牌使用事件，讀取 `ModelInfo` 後更新目前玩家分數，並返回 `Game` 場景。 |
| `ModelInfo` | 掛在 AR Prefab 上，提供模型名稱、類型、分數與特殊效果文字。 |
| `SwitchScene` | 在 AR 場景返回遊戲場景時提供淡入淡出轉場。 |

## 目前掃描流程

1. `UserBtnController.ScanCardBtn` 確認目前玩家不是 `0`。
2. 透過 `SwitchScene` 載入 `ARImageTrackingScene`。
3. `ImageTrackingController` 啟用 `trackedImagesChanged` 事件。
4. 若一定時間內沒有辨識到圖片，會切換到下一個 `imageLibraries`。
5. 辨識成功後，依 Reference Image 名稱尋找同名 Prefab。
6. 生成 Prefab 並顯示使用按鈕。
7. 玩家按下使用後，觸發 `OnUseCardAction`。
8. `ARCardManager` 依 `ModelInfo.modelType` 處理卡牌效果。
9. 目前只有 `Foundation` 類型，會把 `ModelInfo.score` 加到目前玩家分數。
10. 呼叫 `CleanupARBeforeSceneChange` 清理 AR 狀態，再返回 `Game` 場景。

## 新企劃中的 AR 定位

企劃文件描述的 AR 不是任意掃卡加分，而是在玩家完成情境卡最後一步後，掃描該情境卡與步驟卡，播放完整急救 AR 動畫。也就是說，AR 應該是「情境完成驗證與展示」的一部分。

## 與企劃差異

| 項目 | 目前實作 | 企劃要求 |
| --- | --- | --- |
| 掃描時機 | 玩家選定後可進入掃描 | 情境卡步驟完成後掃描 |
| 掃描內容 | 單一 Reference Image 對應 Prefab | 情境卡與步驟卡組合 |
| AR 結果 | 依 `ModelInfo.score` 加分 | 播放完整急救 AR 動畫並依情境步驟數得分 |
| 卡牌類型 | `Foundation` | 情境卡、步驟卡、功能卡、命運卡 |
| ImageLibrary 來源 | 目前使用 `Assets/ImageLibrary` | 新版內容需依 `Assets/Reference` 規劃 |

## `Assets/Old` 使用限制

`Assets/Old/ImageLibrary` 可協助理解 Unity `XRReferenceImageLibrary` 資產如何保存多張辨識圖，以及舊版卡牌如何被分組。但它不應直接決定新版卡牌內容。

可參考：

- `.asset` 與 `.meta` 的保存方式。
- ImageLibrary 分組命名方式。
- Reference Image 與圖片資產的關聯概念。

不可直接沿用：

- 舊版卡牌文字與圖片。
- 舊版卡牌分數或效果。
- 舊版 UI 圖片。
- 舊版 ImageLibrary 的實際卡牌清單。

## 新版 ImageLibrary 建議規格

- 依企劃卡牌類型建立清楚分組，例如情境卡、步驟卡、功能卡、命運卡。
- Reference Image 名稱需能穩定對應遊戲資料，不應只依 Prefab 名稱隱式推斷。
- 若同一情境需要多張步驟卡組合，應建立資料表或 ScriptableObject 管理情境與步驟關係。
- AR Prefab 應保存展示模型與動畫資料，卡牌規則應由資料層或規則服務判定。
- 切換 ImageLibrary 前仍需清理已生成模型與追蹤狀態，避免舊模型殘留。
