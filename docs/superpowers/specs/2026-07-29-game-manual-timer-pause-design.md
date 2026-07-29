# Game 場景說明書暫停倒數設計

## 目標

讓玩家在 `Game` 場景按下 `ManualBtn` 後開啟既有 `ManualPanel`，沿用 `GameSetting.prefab` 的翻頁與返回功能。說明書開啟期間只暫停畫面上的 20 分鐘倒數；關閉後從原本剩餘時間繼續倒數。

## 範圍

- 重用既有 `ManualPanelController` 與 `GameTimer`，不建立第二套說明書控制器。
- 不使用 `Time.timeScale`，避免影響轉盤、動畫、物理與其他遊戲流程。
- 不修改 `GameManager` 的 AR 跨場景暫停狀態。
- 保持 `GameSetting.prefab` 未指派 `GameTimer` 時的既有行為。

## 元件責任

### `ManualPanelController`

- 維持目前的開啟、關閉、翻頁與頁面邊界顯示責任。
- 新增可選的 `GameTimer` Inspector 參考。
- 開啟說明書時記錄計時器是否正在倒數；只有正在倒數時才暫停。
- 關閉說明書時，只有本次開啟曾暫停倒數才恢復，避免誤啟動尚未開始、已結束或原本已暫停的計時器。

### `GameTimer`

- 保留既有 `PauseTimer()`、`ResumeTimer(float)` 與剩餘秒數管理。
- 新增唯讀的 `IsRunning` 狀態，讓 UI 控制器能判斷是否應在關閉後恢復。

## 操作流程

1. 玩家按下 `ManualBtn`。
2. `ManualPanelController.OpenManual()` 判斷 `GameTimer.IsRunning`。
3. 若倒數正在執行，控制器呼叫 `PauseTimer()` 並記錄本次需要恢復。
4. 控制器隱藏 `UserUI`、顯示 `ManualPanel`，並重設到說明書第一頁。
5. 玩家可使用上一頁與下一頁按鈕，行為與 `GameSetting.prefab` 相同。
6. 玩家按下返回按鈕。
7. 控制器隱藏 `ManualPanel`、恢復 `UserUI`。
8. 若本次開啟曾暫停倒數，控制器使用計時器目前保存的剩餘秒數繼續倒數。

## Game 場景 Inspector 配置

`ManualPanelController` 掛在持續啟用的 `Canvas`，避免 `UserUI` 或 `ManualPanel` 切換 Active 狀態後失去事件入口。

- `settingPanel`：指派 `UserUI`。
- `manualPanel`：指派 `ManualPanel`。
- `manualPages`：依 `GameSetting.prefab` 的順序指派相同說明圖片。
- `pageImage`：指派說明書頁面圖片元件。
- `previousButton`：指派 `PreviousBtn`。
- `nextButton`：指派 `NextBtn`。
- `gameTimer`：指派 `Time` 物件上的 `GameTimer`。

按鈕事件：

- `ManualBtn` → `ManualPanelController.OpenManual`
- `BackBtn` → `ManualPanelController.CloseManual`
- `PreviousBtn` → `ManualPanelController.ShowPreviousPage`
- `NextBtn` → `ManualPanelController.ShowNextPage`

`ManualPanel` 預設為 inactive。正式局長若為整整 20 分鐘，`GameTimer` 應設定為 `initialMinutes = 20`、`initialSeconds = 0`。

## 邊界條件

- 倒數尚未開始時開啟與關閉說明書，不得啟動倒數。
- 倒數已歸零時開啟與關閉說明書，不得重新啟動倒數。
- 倒數原本已因其他流程暫停時，不得由說明書關閉流程擅自恢復。
- `GameSetting.prefab` 未指派 `GameTimer` 時，只執行既有面板切換與翻頁功能。
- 說明圖片為空時，維持既有空白頁及隱藏翻頁按鈕的行為。

## 驗證方式

依使用者要求，Unity 編譯、EditMode 測試與實際操作驗證皆由使用者執行。代理只留下可執行的最小檢查案例，並進行差異與場景序列化的靜態檢查，不宣稱功能已在 Unity 中通過。

- 擴充既有 `ManualPanelControllerTests`，供使用者驗證正在倒數時開啟會暫停、關閉會恢復。
- 由使用者驗證尚未開始的計時器經過開啟與關閉後仍未開始。
- 由使用者執行 Unity EditMode 測試並確認 C# 編譯無錯誤。
- 由使用者在 Unity Editor 確認倒數期間開啟說明書，等待數秒後數值不變，返回後從原數值繼續。
- 由使用者確認 `GameSetting.prefab` 的說明書開啟、翻頁、返回功能未受影響。

## 預計修改檔案

- `Assets/Scripts/Game/UI/ManualPanelController.cs`
- `Assets/Scripts/Game/UI/GameTimer.cs`
- `Assets/Editor/ManualPanelControllerTests.cs`
- `Assets/Scenes/Game.unity`

既有未提交的 `Game.unity`、`GameSetting.prefab`、`ARImageTrackingScene.unity` 與美術資源變更必須保留，不得重置或覆蓋。
