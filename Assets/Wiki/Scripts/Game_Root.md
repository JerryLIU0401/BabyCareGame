# Game 根目錄腳本文檔

對應資料夾：`Assets/Scripts/Game`

## `SpinOption`

- 路徑：`Assets/Scripts/Game/SpinFateManager.cs`
- 類型：`[System.Serializable]` 資料類別
- 主要職責：描述轉盤上的單一選項。

### 主要欄位

| 欄位 | 說明 |
| --- | --- |
| `itemName` | 選項名稱。 |
| `weight` | 權重，範圍 0-100。 |
| `themeColor` | 轉盤與圖例顏色。 |
| `startAngle` / `endAngle` | 依權重計算後的角度範圍。 |

### 企劃對應狀態

| 項目 | 狀態 | 說明 |
| --- | --- | --- |
| 命運或隨機事件 | 部分實作 | 有加權選項概念，但未對應企劃的命運卡效果。 |

## `SpinFateManager`

- 路徑：`Assets/Scripts/Game/SpinFateManager.cs`
- 類型：`MonoBehaviour`
- 主要職責：建立轉盤視覺、圖例，並依權重隨機決定結果。

### 主要欄位

| 欄位 | 說明 |
| --- | --- |
| `arrowImage` | 轉盤指針。 |
| `spinButton` | 啟動轉盤按鈕。 |
| `options` | 轉盤選項清單。 |
| `spinDuration` | 指針旋轉時間。 |
| `pieSegmentPrefab` / `pieParent` | 轉盤切片 Prefab 與父物件。 |
| `legendItemPrefab` / `legendParent` | 圖例 Prefab 與父物件。 |
| `isSpinning` | 防止重複旋轉。 |

### 主要方法

| 方法 | 說明 |
| --- | --- |
| `Start()` | 重新整理轉盤。 |
| `RefreshWheel()` | 更新角度並重建圖例。 |
| `CreateLegend()` | 建立圖例與彩色切片。 |
| `ClearChildren(Transform parent)` | 清除指定父物件底下的舊項目。 |
| `UpdateWheelAngles()` | 依權重計算每個選項的角度範圍。 |
| `SpinFate()` | 執行加權隨機，計算目標角度並啟動旋轉。 |
| `SpinRoutine(float totalDegree, SpinOption result)` | 播放指針旋轉動畫並輸出結果。 |

### 企劃對應狀態

| 項目 | 狀態 | 說明 |
| --- | --- | --- |
| 命運卡 | 部分實作 | 企劃是抽到命運卡後立即執行效果，目前是轉盤抽選結果。 |
| 卡牌效果 | 尚未實作 | 沒有套用情況加劇、奇蹟、獨樂樂不如眾樂樂、小補償等效果。 |

### 注意事項

- `visualOffset` 是為了配合指針圖片方向的視覺補償，若更換指針圖需要重新校正。
- `weight` 顯示為百分比，但 `UpdateWheelAngles` 會用總權重正規化；選項總和不一定要剛好 100。
