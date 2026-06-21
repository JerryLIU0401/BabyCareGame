# BabyCareGame

《哈姆的健康生活》AR 桌遊輔助專案。這是一個以 Unity 製作的互動式遊戲專案，結合桌遊流程、玩家計分、命運轉盤、AR 圖像辨識與卡牌模型展示，讓玩家在實體卡牌與平板裝置之間完成健康生活與急救情境的遊戲體驗。

## 專案狀態

目前專案已具備 Unity 遊戲主流程、玩家資料建立、回合玩家顯示、倒數計時、AR 掃描與結算顯示等基礎功能。完整桌遊規則仍在逐步補完中，包含手牌、情境卡、步驟卡配對與特殊卡效果等內容。

## 主要功能

- 2-4 人玩家設定與玩家資料初始化。
- 玩家頭像選擇與目前回合玩家提示。
- 20 分鐘遊戲倒數，時間到後進入結算流程。
- 依玩家人數套用勝利分數門檻：
  - 2 人：22 分
  - 3 人：19 分
  - 4 人：15 分
- 命運轉盤與圖例顯示。
- AR Reference Image 圖庫輪替與卡牌辨識。
- 掃描成功後生成對應 Prefab 模型。
- 使用 AR 卡牌後依 `ModelInfo` 更新目前玩家分數。
- 掃描流程結束後返回 `Game` 場景並接續倒數。
- 結算畫面依玩家分數排序顯示結果。

## 技術資訊

- 遊戲引擎：Unity `2022.3.62f2`
- 主要語言：C#
- 渲染管線：Universal Render Pipeline `16.0.5`
- UI：Unity UI `2.0.0`、TextMeshPro `3.0.9`
- AR：AR Foundation `5.2.0`
- 平台 AR 支援：ARCore `5.2.0`、ARKit `5.2.0`
- 測試框架：Unity Test Framework `1.3.9`
- XR 模擬：XR Simulation Environments

## 場景流程

Build Settings 目前啟用下列場景：

| 場景 | 路徑 | 用途 |
| --- | --- | --- |
| Start | `Assets/Scenes/Start.unity` | 開始畫面與進入遊戲入口 |
| Game | `Assets/Scenes/Game.unity` | 主要遊戲流程、玩家 UI、轉盤與結算 |
| ARImageTrackingScene | `Assets/Scenes/ARImageTrackingScene.unity` | AR 卡牌掃描與模型生成 |

主要流程：

```text
Start -> Game -> ARImageTrackingScene -> Game -> Result
```

## 專案結構

```text
Assets/
  Scripts/
    Menu/                 開始畫面互動
    Scene/                場景切換工具
    Game/
      Data/               玩家與遊戲資料模型
      Manager/            遊戲流程、UI 管理與按鈕控制
      UI/                 倒數、教學、玩家版面與結算顯示
      Player/             玩家分數與狀態顯示
      Card/               AR 卡牌模型資訊
      Function/           玩家人數與遊戲單元選擇
    AR ImageTracking/     AR 圖像追蹤與卡牌使用流程
  Scenes/                 Unity 場景
  Prefabs/                主要 UI、Game、AR、Spin、Model 預製物
  ImageLibrary/           AR Reference Image Library 與辨識資料
  Reference/              企劃文件與規格補完紀錄
  Wiki/graphify-out/      程式碼圖譜與架構分析輸出
Packages/                 Unity Package Manager 設定
ProjectSettings/          Unity 專案設定
ContentPackages/          XR Simulation 本機套件來源
```

## 核心腳本

| 腳本 | 主要職責 |
| --- | --- |
| `GameManager` | 管理玩家資料、遊戲流程、倒數狀態、勝利判定與結算觸發 |
| `PlayerUIManager` | 依玩家資料建立玩家 UI，並更新目前玩家顯示 |
| `GameTimer` | 控制遊戲倒數、暫停、恢復與時間到事件 |
| `UserBtnController` | 控制遊戲中按鈕行為，例如開啟轉盤與進入 AR 掃描 |
| `SpinFateManager` | 管理命運轉盤權重、指針動畫與結果顯示 |
| `ImageTrackingController` | 管理 AR 圖庫輪替、圖像追蹤、模型生成與重新偵測 |
| `ARCardManager` | 接收 AR 卡牌使用事件，更新玩家分數並返回遊戲場景 |
| `GameResultDisplay` | 依分數排序玩家並顯示結算結果 |
| `TransitionManager` | 管理場景內過場提示與動畫 |

## 開發環境設定

1. 使用 Unity Hub 開啟本專案根目錄。
2. 確認 Unity 版本為 `2022.3.62f2`。
3. 等待 Unity Package Manager 還原套件。
4. 確認 Build Settings 內包含：
   - `Assets/Scenes/Start.unity`
   - `Assets/Scenes/Game.unity`
   - `Assets/Scenes/ARImageTrackingScene.unity`
5. 從 `Start` 場景執行，測試完整遊戲進入流程。

## AR 測試注意事項

- `ImageTrackingController` 的 `imageLibraries` 不可為空。
- AR Prefab 名稱需與 Reference Image 名稱一致，否則無法從字典找到對應模型。
- AR 場景需確認 `ARSession`、`XR Origin`、`ARTrackedImageManager` 與 Main Camera Tag 設定正確。
- Editor 可搭配 XR Simulation 測試流程；實機測試需確認 Android 或 iOS 的 AR 支援與權限。
- 離開 AR 場景前需完成 AR Session、追蹤事件與生成模型清理，避免返回遊戲場景後殘留狀態。

## 企劃補完方向

依 `Assets/Reference` 內的企劃文件，目前後續仍可補完：

- 完整手牌、牌庫、棄牌區與回合流程。
- 10 張情境卡與 42 張步驟卡的配對規則。
- 功能卡與命運卡的實際效果。
- 情境完成後才允許 AR 掃描的流程限制。
- 10 張情境卡全部完成後的結束條件。
- 結算畫面區分分數達標、時間到與情境全完成等結束原因。

## 驗證建議

- Unity 編譯無錯誤。
- 以 2、3、4 人各執行一次開局流程。
- 測試 `Start -> Game -> ARImageTrackingScene -> Game` 場景切換。
- 測試倒數暫停、返回後恢復與時間到結算。
- 測試玩家分數達到不同人數門檻後是否進入結算。
- 測試 AR Reference Image 辨識、Prefab 生成、使用卡牌與返回遊戲場景。
- 測試結算畫面是否依玩家分數正確排序。

## 開發規範

本專案的代理與開發流程請參考根目錄 `AGENTS.md`。重要原則包含：

- 文件、討論與程式碼註解使用繁體中文。
- C# 類別、函式、變數與檔名維持英文命名慣例。
- 修改程式碼或資源前需先提出修改計畫。
- 新增或修改核心類別與函式時需補齊 XML Docstring 與繁體中文註解。
- Unity `.meta` 檔不可刪除，避免破壞資源 GUID 引用。
