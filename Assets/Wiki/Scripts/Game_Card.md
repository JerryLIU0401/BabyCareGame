# Game/Card 腳本文檔

對應資料夾：`Assets/Scripts/Game/Card`

## `ModelType`

- 路徑：`Assets/Scripts/Game/Card/ModelInfo.cs`
- 命名空間：`CardModel`
- 類型：`enum`
- 主要職責：定義目前 AR 模型或卡牌的類型。

### 目前列舉值

| 值 | 說明 |
| --- | --- |
| `Foundation` | 代表可直接獲得分數並播放動畫的基礎卡。 |

### 企劃對應狀態

| 項目 | 狀態 | 說明 |
| --- | --- | --- |
| 情境卡 | 尚未實作 | 企劃需要情境卡資料與完成狀態。 |
| 步驟卡 | 尚未實作 | 企劃需要步驟順序與情境對應。 |
| 功能卡 | 尚未實作 | 企劃包含萬用牌、獎勵卡、爆炸卡。 |
| 命運卡 | 尚未實作 | 企劃包含四種命運效果。 |

## `ModelInfo`

- 路徑：`Assets/Scripts/Game/Card/ModelInfo.cs`
- 命名空間：`CardModel`
- 類型：`MonoBehaviour`
- 主要職責：掛在 AR Prefab 上，提供掃描後可讀取的模型或卡牌資料。

### 主要欄位

| 欄位 | 說明 |
| --- | --- |
| `modelName` | 卡片或模型名稱。 |
| `modelType` | 模型類型，目前只有 `Foundation`。 |
| `score` | 掃描後可獲得或扣除的分數。 |
| `specialEffect` | 特殊卡效果文字，目前尚未看到完整使用。 |

### 依賴關係

- 使用者：`ARCardManager`
- 與 `ImageTrackingController` 生成出的 Prefab 搭配。

### 企劃對應狀態

| 項目 | 狀態 | 說明 |
| --- | --- | --- |
| AR 掃描資料 | 目前已實作 | 可讓 AR 掃描結果攜帶分數。 |
| 企劃卡牌資料 | 部分實作 | 目前資料欄位不足以描述情境卡、步驟卡與特殊卡效果。 |

### 注意事項

- 新企劃若要支援完整桌遊卡牌，建議新增 ScriptableObject 或資料表，不要只依靠 Prefab 上的 `ModelInfo`。
