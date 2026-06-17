# BabyCareGame Wiki

本 Wiki 用於整理 `BabyCareGame` 的工程結構、腳本責任、企劃比對與 AR 資產規格。文件撰寫以目前專案狀態為準，並同步參考 `Assets/Reference/《哈姆的健康生活》說明書AR版.pdf` 的新企劃內容。

## 文件定位

- `Assets/Reference/` 是新企劃與玩法規格的主要依據。
- `Assets/Old/` 是舊有資產保存區，只能作為新版 `ImageLibrary` 的 Unity 資料格式與目錄結構參考。
- `Assets/Scripts/` 是目前遊戲邏輯的主要實作來源。
- `Assets/Wiki/` 用於記錄工程現況、企劃差異與後續調整方向。

## 文件索引

| 文件 | 用途 |
| --- | --- |
| [GameDesignComparison.md](GameDesignComparison.md) | 比對企劃文件與目前腳本邏輯的差異。 |
| [GameFlow.md](GameFlow.md) | 說明目前遊戲流程與企劃流程的對照。 |
| [ARFlow.md](ARFlow.md) | 說明 AR 掃描、ImageLibrary 與舊資產參考規則。 |
| [UIFlow.md](UIFlow.md) | 說明 UI、轉場、教學、玩家資訊與結算畫面。 |
| [Scripts/README.md](Scripts/README.md) | 依 `Assets/Scripts/` 資料夾分類的腳本文檔入口。 |

## 企劃比對狀態定義

| 狀態 | 意義 |
| --- | --- |
| 目前已實作 | 腳本中已有對應功能，且可從現有程式碼看出用途。 |
| 部分實作 | 腳本已有相近概念，但未完整符合企劃規格。 |
| 尚未實作 | 企劃有明確需求，但目前腳本尚未看到對應邏輯。 |
| 需確認 | 腳本與企劃存在落差，或需要場景、Prefab、Inspector 綁定才能判斷。 |

## 維護原則

- 修改遊戲規則前，需先更新或檢查 [GameDesignComparison.md](GameDesignComparison.md)。
- 修改任何腳本後，需同步更新 `Scripts/` 下對應分類文件。
- 新增 AR ImageLibrary 或辨識圖時，只能把 `Assets/Old/ImageLibrary` 當作格式參考，內容仍需依新企劃設計。
- 若企劃文件與腳本邏輯衝突，需先列出差異並等待需求確認，不應直接覆蓋現有實作。
