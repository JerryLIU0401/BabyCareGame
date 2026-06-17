# AGENTS.md

本檔案提供 Codex 與其他自動化代理在 `BabyCareGame` 專案中工作的共同規範。請優先遵守本檔，再參考一般 Unity/C# 慣例。

## 1. 溝通與工作流程

- 所有討論、計畫、回報與程式碼註解一律使用繁體中文。
- 變數、函式、類別、命名空間與檔名維持英文業界慣例，例如 PascalCase、camelCase 與 snake_case。
- 任何修改程式碼或資源前，必須先完成需求分析並提出《修改計畫書》。
- 修改計畫需列出預計影響的檔案、核心邏輯變更、風險與驗證方式。
- 未取得明確確認前，不得建立、修改或刪除專案檔案。
- 實作完成後，需回報實際變更、未驗證項目與後續建議。

## 2. 專案概覽

- 專案類型：Unity 遊戲專案。
- Unity 版本：`2022.3.62f2`。
- 主要語言：C#。
- 主要解決方案：`BabyCareGame.sln`。
- 主要 Unity 套件：
  - Universal Render Pipeline：`com.unity.render-pipelines.universal` `16.0.5`
  - TextMeshPro：`com.unity.textmeshpro` `3.0.9`
  - Unity UI：`com.unity.ugui` `2.0.0`
  - AR Foundation：`com.unity.xr.arfoundation` `5.2.0`
  - ARCore：`com.unity.xr.arcore` `5.2.0`
  - ARKit：`com.unity.xr.arkit` `5.2.0`
  - XR Simulation Environments：`com.unity.xr-content.xr-sim-environments`
  - Unity Test Framework：`com.unity.test-framework` `1.3.9`

## 3. 重要目錄

- `Assets/Scripts`：專案主要 C# 腳本，應優先在此進行遊戲邏輯修改。
- `Assets/Scripts/Game/Data`：遊戲資料模型，例如 `PlayerData`、`GameData`。
- `Assets/Scripts/Game/Manager`：遊戲流程、UI 管理、轉場與按鈕控制。
- `Assets/Scripts/Game/UI`：遊戲 UI 顯示與版面調整。
- `Assets/Scripts/Game/Player`：玩家分數與玩家狀態顯示。
- `Assets/Scripts/Game/Card`：卡牌與模型資訊。
- `Assets/Scripts/AR ImageTracking`：AR 圖像追蹤、卡牌使用與掃描流程。
- `Assets/Scripts/Menu`：開始畫面互動。
- `Assets/Scripts/Scene` 與 `Assets/Scripts/Game/Scene`：場景切換與場景進入點。
- `Assets/Scenes`：Unity 場景，目前 Build Settings 啟用 `Start`、`Game`、`ARImageTrackingScene`。
- `Assets/Prefabs`：主要遊戲 Prefab，包含 UI、Game、AR、Spin、Building、Model。
- `Assets/prefab`：小寫目錄，包含另一批既有 Prefab，修改前需先確認其用途與場景引用。
- `Assets/ImageLibrary`：AR Reference Image Library 與卡牌辨識資料。
- `Assets/Wiki`：工程文件與 graphify 程式碼圖譜輸出位置，實際圖譜位於 `Assets/Wiki/graphify-out`。
- `Assets/Reference`：新企劃、玩法規格與需求比對的主要依據，目前包含《哈姆的健康生活》AR 版說明書。
- `Assets/Old`：舊有資產保存區，僅可作為新版 `ImageLibrary` 的 Unity 資料格式、命名方式與目錄結構參考。
- `Assets/XR`：ARCore、ARKit、XR Simulation 與 XR Loader 設定。
- `Assets/URP`：URP 管線設定與 Unity 範例資訊。
- `ContentPackages`：XR Simulation 本機套件來源。
- `Packages`：Unity Package Manager 設定。
- `ProjectSettings`：Unity 專案設定。

## 4. 不應直接修改的範圍

- 不要手動修改 `Library`、`Temp`、`Logs`、`UserSettings`，這些是 Unity 或本機環境輸出。
- 不要直接修改自動產生的 `.csproj`、`.sln`，除非需求明確要求處理 IDE/專案檔。
- 不要刪除或忽略 `.meta` 檔，Unity GUID 依賴 `.meta` 維持場景、Prefab 與資源引用。
- 不要隨意修改 `Assets/UnityXRContent`、`Assets/Package/M Studio`、`Assets/URP/TutorialInfo` 等第三方或 Unity 範例內容。
- 不要直接搬移、覆蓋、復用或刪除 `Assets/Old` 內的舊資產，除非需求明確指定；該目錄不是新企劃的功能或美術來源。
- 不要把 `Assets/Old` 的舊版卡牌、UI 或 ImageLibrary 視為現行玩法需求；若需參考，只能用於理解 Unity `XRReferenceImageLibrary` 資產格式與舊資料組織方式。
- 若工作樹已有使用者變更，必須保留並避開不相關檔案，不得重置或覆蓋。

## 4.1 企劃文件與需求比對

- `Assets/Reference/《哈姆的健康生活》說明書AR版.pdf` 是目前新企劃的主要玩法依據。
- 修改遊戲規則、卡牌流程、玩家人數、勝利條件、AR 掃描流程或 UI 流程前，必須先比對 `Assets/Reference` 內的企劃規格。
- 撰寫 Wiki 或工程文件時，需區分「目前腳本已實作」、「企劃要求」、「差異與待補」三種狀態。
- 若腳本邏輯與企劃文件衝突，不得直接推定任一方正確；需在修改計畫中列出差異並等待確認。
- 目前已知企劃重點包含 2-4 人遊玩、20 分鐘倒數、情境卡與步驟卡配對、功能卡與命運卡效果、完成情境後掃描 AR 動畫、依玩家人數不同的勝利分數。
- `Assets/Old/ImageLibrary` 可用來參考新版 ImageLibrary 的 Unity 資產格式，但新版卡牌內容、辨識圖、分數與流程仍需以 `Assets/Reference` 的新企劃為準。

## 4.2 程式碼圖譜

- `Assets/Scripts` 的 graphify 圖譜實際位置為 `Assets/Wiki/graphify-out`。
- 互動式圖譜入口為 `Assets/Wiki/graphify-out/graph.html`，可直接用瀏覽器開啟。
- 原始圖資料為 `Assets/Wiki/graphify-out/graph.json`，可供 graphify 查詢與其他工具讀取。
- 圖譜稽核報告為 `Assets/Wiki/graphify-out/GRAPH_REPORT.md`，用於快速檢視核心節點、社群與建議問題。
- agent 可讀 wiki 入口為 `Assets/Wiki/graphify-out/wiki/index.md`，後續分析程式碼關係時應優先查閱此目錄。

## 5. 核心遊戲流程

- `Start` 場景由 `MenuController` 偵測觸控後進入 `Game` 場景。
- `GameManager` 是遊戲資料與回合流程的核心，會透過 `DontDestroyOnLoad` 跨場景保留。
- `SelectPlayerCount` 透過 `OnPlayerCountConfirmed` 通知 `GameManager` 建立玩家資料。
- `SelectGameUnit` 透過 `OnComfirmPlayTech` 通知是否開啟教學面板。
- `GameManager` 透過 `OnPlayerDataGenerated`、`ChangePlayerUI`、`OnGameVictory` 對外發布資料變更。
- `PlayerUIManager` 訂閱玩家資料與目前玩家事件，建立玩家 UI 並更新回合顯示。
- `PlayerController` 訂閱玩家資料事件，顯示目前玩家分數。
- `UserBtnController` 負責遊戲中按鈕行為，例如開啟轉盤與進入 AR 掃描場景。
- `SpinFateManager` 負責加權轉盤、圖例與指針動畫。
- `ImageTrackingController` 負責 AR 圖像庫輪替、Prefab 生成、追蹤狀態更新與卡牌使用事件。
- `ARCardManager` 接收卡牌使用事件，依 `ModelInfo` 更新玩家分數，並返回 `Game` 場景。
- `GameResultDisplay` 接收勝利事件後排序玩家分數並顯示結算 UI。

## 6. 關鍵資料規則

- `GameData.currentPlayer` 使用 1-based 玩家編號；`0` 代表尚未選擇玩家。
- 修改玩家切換、UI 更新或分數邏輯時，必須保護 `currentPlayer == 0` 的未選擇狀態。
- `PlayerData.score` 是目前核心勝利依據；現有勝利條件為分數達到 `30`。
- `PlayerData.scanCount` 已存在但目前使用較少，擴充掃卡次數前需先確認既有設計。
- `ModelInfo.modelName`、`modelType`、`score`、`specialEffect` 是 AR 卡牌模型資料來源。
- AR Prefab 名稱需與 Reference Image 名稱相符，否則 `ImageTrackingController` 無法從字典找到對應模型。

## 7. C# 與 Unity 程式碼規範

- 新增或修改的核心類別與函式必須加入 XML Docstring，說明目的、參數型別與回傳值。
- 新增或修改的程式碼必須附繁體中文註解，註解重點應說明設計理由、業務規則與邊界條件。
- `MonoBehaviour` 應主要負責 Unity 生命週期、Inspector 綁定與事件轉接；複雜業務邏輯應抽到純 C# 類別。
- 事件訂閱應放在 `OnEnable`，取消訂閱應放在 `OnDisable`，避免跨場景或物件銷毀後殘留事件。
- Inspector 綁定欄位優先使用 `[SerializeField] private`，並用註解說明該欄位存在的設計原因。
- 修改 UI 子物件路徑前需同步檢查 Prefab；例如既有程式碼使用 `"DispalyUI"`，不可只在程式端修正拼字而不更新 Prefab。
- 避免新增硬編碼場景名稱；若需要擴充，優先集中定義常數並同步檢查 Build Settings。
- 避免新增不必要的全域狀態或 Singleton。若因 Unity 場景生命週期需要保留狀態，必須說明生命週期與釋放條件。
- 儘量減少 `FindFirstObjectByType` 的新增使用；新功能優先透過 Inspector 注入、事件或明確依賴傳遞。

## 8. 架構原則

- 遵守單一職責原則：資料模型、遊戲規則、UI 顯示與場景控制應分層處理。
- 遵守關注點分離：資料層不直接操作 UI，UI 層不直接持有複雜遊戲規則。
- 優先使用事件或介面隔離模組互動，避免管理器彼此直接深度耦合。
- 若需要重構既有管理器，應小步調整，先保留既有 public API 與場景綁定，再逐步抽離邏輯。
- AR 流程需維持黑盒邊界：辨識、卡牌資料讀取、分數套用與場景切換應可分別替換。

## 9. AR/XR 修改注意事項

- 修改 `ImageTrackingController` 前需檢查 `trackedImageManager`、`imageLibraries`、`prefabs`、`searchText`、`useBtnGameObject` 的 Inspector 綁定。
- `imageLibraries` 不可為空，否則圖庫輪替會發生索引錯誤。
- 切換 AR 圖庫時需清理已生成模型與追蹤狀態，避免舊模型殘留。
- 離開 AR 場景前應呼叫 `CleanupARBeforeSceneChange`，確保 AR Session、XR Origin 與生成物件被正確處理。
- 修改 AR 場景時需檢查 Main Camera Tag、XR Origin、AR Session、ARTrackedImageManager 與 Reference Image Library。

## 10. UI 修改注意事項

- UI 使用 Unity UGUI、TextMeshPro 與 Prefab 綁定。
- 調整玩家 UI 時需同步檢查 `PlayerUIManager`、`PlayerGridLayoutAdjust`、玩家 UI Prefab 與背景/頭像 Sprite 陣列。
- 調整教學流程時需檢查 `TeachPanelControl`、教學 Sprite 陣列與 `GameManager.GameDataGameTechPanel`。
- 調整轉場時需檢查 `SwitchScene` 與 `TransitionManager`，避免跨場景物件提前銷毀或 CanvasGroup 狀態錯誤。
- UI 文字若涉及玩家可見內容，需維持繁體中文並避免破壞既有教學語境。

## 11. 驗證方式

- C# 修改後至少需確認 Unity 編譯不報錯。
- 涉及場景切換時，需在 Unity Editor 中測試 `Start -> Game -> ARImageTrackingScene -> Game`。
- 涉及 AR 時，需在 XR Simulation 或目標裝置測試 Reference Image 辨識、Prefab 生成、使用卡牌與返回遊戲場景。
- 涉及 UI 時，需在 Unity Editor 檢查 Inspector 綁定、Prefab 子物件路徑與不同玩家數量下的顯示。
- 涉及資料邏輯時，應補 Unity Test Framework 測試或可重複的手動驗證步驟。
- 若無法執行 Unity Editor 或測試，回報時必須明確說明未驗證項目。

## 12. Git 與本機環境注意事項

- 此工作區可能因 Windows 使用者擁有權造成 Git `dubious ownership` 警告；查詢狀態可使用 `git -c safe.directory=C:/Users/jerry/OneDrive/Documents/GitHub/BabyCareGame status --short`。
- 不要為了消除警告而擅自修改使用者全域 Git 設定。
- `.gitignore` 已忽略 Unity 產生目錄、Build 輸出、IDE 快取與自動產生專案檔。
- 若需要新增大型二進位資源，需先確認是否應納入版本控制與 Unity `.meta` 是否同步產生。
