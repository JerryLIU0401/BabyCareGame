# AR ImageTracking 腳本文檔

對應資料夾：`Assets/Scripts/AR ImageTracking`

## `ImageTrackingController`

- 路徑：`Assets/Scripts/AR ImageTracking/ImageTrackingController.cs`
- 類型：`MonoBehaviour`
- 主要職責：管理 AR 圖像追蹤、ImageLibrary 輪替、Prefab 生成、追蹤狀態與卡牌使用事件。

### 主要欄位

| 欄位 | 說明 |
| --- | --- |
| `trackedImageManager` | AR Foundation 的圖像追蹤管理器。 |
| `imageLibraries` | 可輪替的 Reference Image Library 陣列。 |
| `prefabs` | Reference Image 名稱對應的 Prefab 陣列。 |
| `prefabDictionary` | 以 Prefab 名稱建立的查找字典。 |
| `spawnedPrefabs` | 已生成的模型物件。 |
| `trackingStates` | 記錄每張圖最近追蹤狀態，避免重複切換顯示。 |
| `currentImageName` | 目前辨識到的圖片名稱。 |
| `checkInterval` | 無辨識時切換圖庫的間隔。 |
| `searchText` | 顯示辨識狀態或卡牌名稱。 |
| `useBtnGameObject` | 掃描成功後顯示的使用按鈕。 |

### 主要事件與方法

| 成員 | 說明 |
| --- | --- |
| `OnUseCardAction` | 玩家按下使用卡牌時傳出 `ModelInfo`。 |
| `Awake()` | 將 Prefab 陣列建立為名稱查找字典。 |
| `Start()` | 檢查 AR 相機、`trackedImageManager` 與 Main Camera 狀態。 |
| `OnEnable()` / `OnDisable()` | 訂閱與取消 `trackedImagesChanged`。 |
| `Update()` | 無辨識時輪替 ImageLibrary；有辨識時顯示使用按鈕。 |
| `OnTrackedImagesChanged(...)` | 處理新增、更新與移除的追蹤圖像。 |
| `HandleTrackedImage(ARTrackedImage trackedImage)` | 首次辨識到圖片時生成對應 Prefab。 |
| `UpdateModelTransform(ARTrackedImage trackedImage)` | 根據追蹤狀態顯示或隱藏模型。 |
| `SwitchToNextLibrary()` | 切換到下一個 ImageLibrary 並清理生成物。 |
| `UseCardBtn()` | 取得目前模型上的 `ModelInfo` 並觸發事件。 |
| `CleanupARBeforeSceneChange()` | 離開 AR 場景前清理追蹤事件、AR Session、生成物與 XR Origin。 |

### 企劃對應狀態

| 項目 | 狀態 | 說明 |
| --- | --- | --- |
| Reference Image 掃描 | 目前已實作 | 可透過 AR Foundation 辨識圖片。 |
| ImageLibrary 輪替 | 目前已實作 | 無辨識時會切換下一個圖庫。 |
| Prefab 生成 | 目前已實作 | Reference Image 名稱需與 Prefab 名稱一致。 |
| 掃描情境卡與步驟卡組合 | 尚未實作 | 目前一次處理單一圖片名稱。 |
| 播放完整急救 AR 動畫 | 部分實作 | 可生成 Prefab，但未看到企劃中的情境完成動畫流程。 |

### 注意事項

- `imageLibraries` 不可為空，否則 `currentLibraryIndex` 存取會出錯。
- `prefabs` 名稱必須與 Reference Image 名稱一致。
- 離開 AR 場景前應呼叫 `CleanupARBeforeSceneChange()`，避免 AR Session 或生成物殘留。
- `Assets/Old/ImageLibrary` 僅可作為格式參考，新版辨識內容需依 `Assets/Reference` 重新規劃。

## `ARCardManager`

- 路徑：`Assets/Scripts/AR ImageTracking/ARCardManager.cs`
- 類型：`MonoBehaviour`
- 主要職責：接收 AR 卡牌使用事件，根據 `ModelInfo` 更新玩家分數，並返回遊戲場景。

### 主要欄位

| 欄位 | 說明 |
| --- | --- |
| `gameManager` | 目前玩家資料來源。 |
| `imageTrackingController` | AR 掃描事件來源。 |
| `specialCardView` | 特殊卡顯示區，目前未看到使用。 |
| `specialCardText` | 特殊卡文字，目前未看到使用。 |
| `switchScenePrefab` | 返回 `Game` 場景的轉場 Prefab。 |

### 主要方法

| 方法 | 說明 |
| --- | --- |
| `Awake()` | 尋找 `GameManager` 與 `ImageTrackingController`。 |
| `OnEnable()` / `OnDisable()` | 訂閱與取消 `OnUseCardAction`。 |
| `CardState(ModelInfo currentModelInfo)` | 依 `modelType` 分派卡牌效果。 |
| `GetFoundationCardScore(ModelInfo buildModelInfo)` | 將 `score` 加到目前玩家，然後返回遊戲場景。 |
| `GoGameScene()` | 清理 AR 並透過 `SwitchScene` 返回 `Game`。 |
| `BackGameScene()` | 不加分，更新遊戲資料後返回 `Game`。 |

### 企劃對應狀態

| 項目 | 狀態 | 說明 |
| --- | --- | --- |
| 掃描後加分 | 目前已實作 | 可依 `ModelInfo.score` 加到目前玩家。 |
| 情境完成計分 | 部分實作 | 企劃應依情境步驟數得分，目前由 Prefab 分數決定。 |
| 功能卡與命運卡效果 | 尚未實作 | `CardState` 目前只處理 `Foundation`。 |
| AR 動畫完成驗證 | 尚未實作 | 未看到完成動畫後才結算或返回的狀態。 |

### 注意事項

- `GetFoundationCardScore` 假設目前玩家存在；若 `currentPlayer == 0`，`gameManager.GetPlayerData()` 會回傳 `null`。
- 若未來加入特殊卡，建議將效果邏輯抽到卡牌效果服務，避免 `ARCardManager` 過度膨脹。
