# GameSetting 說明書介面實作計畫

> **代理執行規範：** 本計畫採同一工作階段逐步執行；每個步驟都必須先驗證再前進。

**目標：** 讓玩家從 `GameSetting` 開啟說明書、以 Inspector 指派的圖片翻頁，並在頁面邊界隱藏無效按鈕。

**架構：** 新增一個只負責說明書 UI 狀態的 `ManualPanelController`，掛在持續啟用的 `GameSetting` 根物件。頁面使用序列化的 `Sprite[]`，既有按鈕透過 Prefab 的持久化 `OnClick` 事件呼叫控制器，不新增全域狀態或額外依賴。

**技術：** Unity 2022.3.62f2、C#、UGUI、Unity Editor 自動化檢查。

## 全域限制

- 所有新增程式碼註解與 XML 文件字串使用繁體中文。
- 類別與成員維持英文命名慣例。
- 保留 `GameSetting.prefab` 與其他檔案的既有未提交修改。
- 不新增說明書圖片；圖片由使用者日後透過 Inspector 指派。
- `ManualPanel` 開啟時隱藏 `SettingPanel`，返回時恢復。
- 第一頁隱藏 Previous，最後一頁隱藏 Next；零頁時兩者都隱藏。

---

### 工作 1：說明書 UI 控制與 Prefab 綁定

**檔案：**

- 新增：`Assets/Scripts/Game/UI/ManualPanelController.cs`
- 新增：`Assets/Scripts/Game/UI/ManualPanelController.cs.meta`
- 修改：`Assets/Prefabs/UI/GameSetting.prefab`
- 驗證：`Assets/Editor/ManualPanelControllerTests.cs`

**介面：**

- 輸入：Inspector 指派的 `Sprite[] manualPages` 與既有五個 UI 物件參考。
- 輸出：`OpenManual()`、`CloseManual()`、`ShowNextPage()`、`ShowPreviousPage()` 四個按鈕事件入口。

- [x] **步驟 1：建立會失敗的 Unity Editor 行為檢查**

  建立可保留的 Editor 測試，驗證零頁、開啟、下一頁、上一頁、重新開啟重設與返回狀態；隔離的最小 Unity 專案負責在不關閉使用者 Editor 的情況下執行 RED／GREEN。

- [x] **步驟 2：執行檢查並確認 RED**

  使用已連線的 Unity Editor 執行檢查；預期因 `UI.ManualPanelController` 尚不存在而失敗。

- [x] **步驟 3：新增最小控制器**

  控制器只保存目前頁碼、切換兩個面板、更新顯示圖片，以及根據頁碼設定兩個翻頁按鈕的 `active` 狀態。

- [x] **步驟 4：綁定 Prefab**

  在 `GameSetting` 根物件加入控制器，序列化欄位分別指向 `SettingPanel`、`ManualPanel`、頁面 `Image`、`PreviousBtn` 與 `NextBtn`。將 `ManualPanel` 預設設為 inactive，並把四個既有按鈕的 `OnClick` 連到對應公開方法。

- [x] **步驟 5：執行檢查並確認 GREEN**

  再次於 Unity Editor 執行相同檢查；預期所有狀態斷言通過。

- [x] **步驟 6：驗證 Unity 編譯與 Prefab 序列化**

  使用 Unity 2022.3.62f2 的隔離 EditMode 測試確認控制器與測試可編譯，並以唯讀腳本確認 Prefab 的控制器欄位、四個事件方法及 `ManualPanel` 預設狀態均已序列化。

- [x] **步驟 7：檢查最終差異**

  對新增 C#、`.meta` 與計畫檔執行 `git diff --check`；Prefab 則以限定新增區塊的結構與行尾檢查避開既有 Unity YAML 空值格式，確認沒有無關重排或覆蓋使用者現有修改。
