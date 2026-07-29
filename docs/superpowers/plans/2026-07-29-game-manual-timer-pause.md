# Game 場景說明書暫停倒數實作計畫

> **代理執行要求：** 實作時必須使用 `superpowers:subagent-driven-development`（建議）或 `superpowers:executing-plans`，並依核取方塊逐步完成。

**目標：** 讓 `Game` 場景的說明書開啟時只暫停 20 分鐘倒數，返回後從原本剩餘時間繼續。

**架構：** 重用 `ManualPanelController` 與 `GameTimer`。控制器只在計時器原本正在倒數時暫停並記錄恢復責任；同場景關閉說明書時使用計時器保留的秒數繼續，不加入 `GameManager` 跨場景狀態。

**技術：** Unity 2022.3.62f2、C#、UGUI、TextMeshPro、Unity Test Framework。

## 全域限制

- 所有新增或修改的程式碼註解與 XML 文件字串使用繁體中文。
- 變數、函式、類別與檔名維持英文業界命名慣例。
- 不使用 `Time.timeScale`，不修改 `GameManager`。
- 不新增控制器、介面、工廠或外部依賴。
- 保留工作樹中既有 `Game.unity`、`GameSetting.prefab`、`ARImageTrackingScene.unity` 與美術資源修改。
- Unity 編譯、EditMode 測試與實際操作驗證由使用者執行；代理只進行差異與序列化靜態檢查。

---

## 檔案責任

- `Assets/Scripts/Game/UI/GameTimer.cs`：提供倒數是否正在執行的唯讀狀態。
- `Assets/Scripts/Game/UI/ManualPanelController.cs`：控制說明書 UI，並選配協調同場景倒數暫停與恢復。
- `Assets/Editor/ManualPanelControllerTests.cs`：留下可由使用者執行的最小狀態檢查。
- `Assets/Scenes/Game.unity`：連接既有 UI、說明圖片、按鈕事件與 `GameTimer`。

### Task 1：加入說明書倒數協調

**檔案：**

- 修改：`Assets/Scripts/Game/UI/GameTimer.cs:39-47`
- 修改：`Assets/Scripts/Game/UI/ManualPanelController.cs:6-44`
- 測試：`Assets/Editor/ManualPanelControllerTests.cs:114-129`

**介面：**

- 使用：`GameTimer.PauseTimer()`、`GameTimer.ResumeTimer(float)`、`GameTimer.GetRemainingSeconds()`。
- 產出：`GameTimer.IsRunning : bool`。
- 產出：`ManualPanelController` 的選配序列化欄位 `gameTimer : GameTimer`。

- [ ] **步驟 1：先加入會覆蓋新行為的最小測試**

在 `OpenWithoutPages_ClearsImageAndHidesNavigationButtons()` 後加入：

```csharp
/// <summary>
/// 驗證說明書只會恢復本次開啟前正在執行的倒數。
/// </summary>
[Test]
public void OpenAndCloseManual_PreservesTimerRunningState()
{
    GameObject timerObject = CreateChild("GameTimer");
    GameTimer gameTimer = timerObject.AddComponent<GameTimer>();
    AssignReference("gameTimer", gameTimer);

    controller.OpenManual();
    controller.CloseManual();

    // 尚未開始的倒數不可因關閉說明書而被誤啟動。
    Assert.That(gameTimer.HasStarted, Is.False);
    Assert.That(gameTimer.IsRunning, Is.False);

    gameTimer.StartTimer();
    float remainingSeconds = gameTimer.GetRemainingSeconds();

    controller.OpenManual();

    Assert.That(gameTimer.IsRunning, Is.False);
    Assert.That(gameTimer.GetRemainingSeconds(), Is.EqualTo(remainingSeconds));

    controller.CloseManual();

    Assert.That(gameTimer.IsRunning, Is.True);
    Assert.That(gameTimer.GetRemainingSeconds(), Is.EqualTo(remainingSeconds));
}
```

- [ ] **步驟 2：由使用者確認測試在實作前無法編譯**

使用者可在 Unity Test Runner 執行 `ManualPanelControllerTests.OpenAndCloseManual_PreservesTimerRunningState`。

預期：因 `GameTimer.IsRunning` 與 `ManualPanelController.gameTimer` 尚不存在而無法通過編譯或測試。

代理不執行此步驟，只回報未驗證。

- [ ] **步驟 3：公開計時器唯讀執行狀態**

在 `GameTimer.HasExpired` 後加入：

```csharp
/// <summary>
/// 取得倒數目前是否正在扣減，讓同場景 UI 能保留開啟前的執行狀態。
/// </summary>
public bool IsRunning => isRunning;
```

同步將 `PauseTimer()` 與 `ResumeTimer(float)` 的 XML 文件字串與內部註解改為同時涵蓋同場景 UI 與跨場景流程，避免文件仍把共用方法限定為 AR 用途；方法簽章與既有行為不變。

- [ ] **步驟 4：讓說明書控制器選配協調倒數**

先將類別摘要改為：

```csharp
/// <summary>
/// 控制說明書的面板切換、圖片翻頁、邊界按鈕與選配的倒數暫停。
/// </summary>
```

在 `nextButton` 欄位後加入：

```csharp
// GameSetting 不需要計時器；Game 場景可選配此參考，重用同一套說明書行為。
[SerializeField] private GameTimer gameTimer;

// 只恢復本次開啟前確實正在執行的倒數，避免誤啟動其他流程已暫停的計時器。
private bool shouldResumeTimer;
```

將 `OpenManual()` 改為：

```csharp
/// <summary>
/// 顯示說明書並從第一頁開始；若倒數正在執行則暫停。
/// </summary>
public void OpenManual()
{
    currentPageIndex = 0;
    shouldResumeTimer = gameTimer != null && gameTimer.IsRunning;

    if (shouldResumeTimer)
    {
        // 只暫停局內倒數，不使用 timeScale 影響其他遊戲系統。
        gameTimer.PauseTimer();
    }

    settingPanel.SetActive(false);
    manualPanel.SetActive(true);
    RefreshPage();
}
```

將 `CloseManual()` 改為：

```csharp
/// <summary>
/// 關閉說明書、恢復來源畫面，並視開啟前狀態繼續倒數。
/// </summary>
public void CloseManual()
{
    manualPanel.SetActive(false);
    settingPanel.SetActive(true);

    bool resumeTimer = shouldResumeTimer;
    shouldResumeTimer = false;

    if (resumeTimer && gameTimer != null)
    {
        // 同一場景的 GameTimer 已保留剩餘秒數，不需要建立另一份時間狀態。
        gameTimer.ResumeTimer(gameTimer.GetRemainingSeconds());
    }
}
```

- [ ] **步驟 5：保留測試給使用者執行**

使用者可執行全部 `ManualPanelControllerTests`。

預期：既有翻頁與空頁測試通過，新測試確認倒數只在原本執行時恢復。

代理不執行 Unity 測試，只執行：

```powershell
git diff --check -- Assets/Scripts/Game/UI/GameTimer.cs Assets/Scripts/Game/UI/ManualPanelController.cs Assets/Editor/ManualPanelControllerTests.cs
```

- [ ] **步驟 6：提交不與使用者修改重疊的腳本與測試**

```powershell
git add -- Assets/Scripts/Game/UI/GameTimer.cs Assets/Scripts/Game/UI/ManualPanelController.cs Assets/Editor/ManualPanelControllerTests.cs
git commit -m "feat: pause game timer while manual is open"
```

### Task 2：配置 Game 場景既有 UI

**檔案：**

- 修改：`Assets/Scenes/Game.unity`

**介面：**

- 使用：`ManualPanelController.OpenManual()`、`CloseManual()`、`ShowPreviousPage()`、`ShowNextPage()`。
- 使用：`GameTimer` 元件 `fileID 2012846653`。
- 產出：掛在持續啟用 `Canvas` 上的 `ManualPanelController` 元件。

- [ ] **步驟 1：在 Canvas 註冊控制器元件**

於 `Canvas`（GameObject `fileID 1923239883`）的 `m_Component` 加入：

```yaml
- component: {fileID: 1923239888}
```

新增元件資料：

```yaml
--- !u!114 &1923239888
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 1923239883}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: dd13d0ad0f154917a7aac5e78aa8b9b2, type: 3}
  m_Name:
  m_EditorClassIdentifier:
  settingPanel: {fileID: 1834040642}
  manualPanel: {fileID: 295167432}
  manualPages:
  - {fileID: 21300000, guid: db6dc84c59e8179439942a76f7b1fb56, type: 3}
  - {fileID: 21300000, guid: b4c30a6cc8c494548bba3fff1b2567d8, type: 3}
  pageImage: {fileID: 2084276705}
  previousButton: {fileID: 45682672}
  nextButton: {fileID: 236342175}
  gameTimer: {fileID: 2012846653}
```

- [ ] **步驟 2：連接四個既有按鈕**

將 `PreviousBtn`、`NextBtn`、`BackBtn` 既有事件的空 Target：

```yaml
m_Target: {fileID: 0}
```

各自改為：

```yaml
m_Target: {fileID: 1923239888}
```

保留原本方法名稱 `ShowPreviousPage`、`ShowNextPage`、`CloseManual`。

將 `ManualBtn` 的空事件清單改為：

```yaml
m_OnClick:
  m_PersistentCalls:
    m_Calls:
    - m_Target: {fileID: 1923239888}
      m_TargetAssemblyTypeName: UI.ManualPanelController, Assembly-CSharp
      m_MethodName: OpenManual
      m_Mode: 1
      m_Arguments:
        m_ObjectArgument: {fileID: 0}
        m_ObjectArgumentAssemblyTypeName: UnityEngine.Object, UnityEngine
        m_IntArgument: 0
        m_FloatArgument: 0
        m_StringArgument:
        m_BoolArgument: 0
      m_CallState: 2
```

- [ ] **步驟 3：設定初始顯示與正式倒數**

將 `ManualPanel`：

```yaml
m_IsActive: 1
```

改為：

```yaml
m_IsActive: 0
```

將 `GameTimer`：

```yaml
initialMinutes: 20
initialSeconds: 3
```

改為：

```yaml
initialMinutes: 20
initialSeconds: 0
```

- [ ] **步驟 4：只做場景序列化靜態檢查**

```powershell
rg -n -C 4 "dd13d0ad0f154917a7aac5e78aa8b9b2|m_MethodName: OpenManual|m_MethodName: CloseManual|m_MethodName: ShowPreviousPage|m_MethodName: ShowNextPage|initialSeconds:" Assets/Scenes/Game.unity
git diff --check -- Assets/Scenes/Game.unity
```

預期：

- 控制器只掛在 `Canvas` 一次。
- 四個事件都指向 `fileID 1923239888`。
- `ManualPanel` 預設關閉。
- `initialSeconds` 為 `0`。

代理不執行 Unity 編譯、Test Runner 或 Play Mode。

- [ ] **步驟 5：不提交含有使用者既有修改的場景**

`Assets/Scenes/Game.unity` 在本工作開始前已有未提交修改，而且本功能依賴其中新建的 `ManualBtn` 與 `ManualPanel`。不得整檔暫存或提交，以免將使用者變更併入代理提交；場景修改保留在工作樹，交由使用者審查與提交。

## 使用者驗證清單

1. 開啟 `Game` 場景，確認 Console 無編譯錯誤。
2. 進入遊戲並開始倒數，記錄剩餘時間。
3. 按 `ManualBtn`，等待數秒，確認倒數數值不變。
4. 測試上一頁、下一頁與頁面邊界按鈕。
5. 按返回，確認 `UserUI` 恢復且倒數從原數值繼續。
6. 確認尚未開始或已結束的倒數不會因開關說明書而重新啟動。
7. 開啟動態生成的 `GameSetting.prefab` 說明書，確認既有功能不變。
