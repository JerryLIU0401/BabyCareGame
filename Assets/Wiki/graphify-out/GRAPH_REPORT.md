# Graph Report - C:\Users\jerry\OneDrive\Documents\GitHub\BabyCareGame\Assets\Scripts  (2026-06-17)

## Corpus Check
- Corpus is ~3,825 words - fits in a single context window. You may not need a graph.

## Summary
- 246 nodes · 297 edges · 17 communities
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- [[_COMMUNITY_AR 圖像追蹤|AR 圖像追蹤]]
- [[_COMMUNITY_遊戲核心管理|遊戲核心管理]]
- [[_COMMUNITY_共用 Unity 型別|共用 Unity 型別]]
- [[_COMMUNITY_命運轉盤系統|命運轉盤系統]]
- [[_COMMUNITY_玩家 UI 管理|玩家 UI 管理]]
- [[_COMMUNITY_轉場動畫控制|轉場動畫控制]]
- [[_COMMUNITY_AR 卡牌套用|AR 卡牌套用]]
- [[_COMMUNITY_玩家分數控制|玩家分數控制]]
- [[_COMMUNITY_遊戲單元選擇|遊戲單元選擇]]
- [[_COMMUNITY_結算結果顯示|結算結果顯示]]
- [[_COMMUNITY_玩家人數選擇|玩家人數選擇]]
- [[_COMMUNITY_遊戲按鈕控制|遊戲按鈕控制]]
- [[_COMMUNITY_場景淡入切換|場景淡入切換]]
- [[_COMMUNITY_標題閃爍效果|標題閃爍效果]]
- [[_COMMUNITY_教學面板控制|教學面板控制]]
- [[_COMMUNITY_玩家資料模型|玩家資料模型]]
- [[_COMMUNITY_遊戲資料模型|遊戲資料模型]]

## God Nodes (most connected - your core abstractions)
1. `GameManager` - 24 edges
2. `ImageTrackingController` - 23 edges
3. `PlayerUIManager` - 16 edges
4. `SpinFateManager` - 16 edges
5. `ARCardManager` - 14 edges
6. `TransitionManager` - 13 edges
7. `SelectGameUnit` - 12 edges
8. `SelectPlayerCount` - 11 edges
9. `UserBtnController` - 11 edges
10. `GameResultDisplay` - 11 edges

## Surprising Connections (you probably didn't know these)
- `ARCardManager` --inherits--> `MonoBehaviour`  [EXTRACTED]
  C:/Users/jerry/OneDrive/Documents/GitHub/BabyCareGame/Assets/Scripts/AR ImageTracking/ARCardManager.cs →   _Bridges community 6 → community 2_
- `ImageTrackingController` --inherits--> `MonoBehaviour`  [EXTRACTED]
  C:/Users/jerry/OneDrive/Documents/GitHub/BabyCareGame/Assets/Scripts/AR ImageTracking/ImageTrackingController.cs →   _Bridges community 2 → community 0_
- `SelectGameUnit` --inherits--> `MonoBehaviour`  [EXTRACTED]
  C:/Users/jerry/OneDrive/Documents/GitHub/BabyCareGame/Assets/Scripts/Game/Function/SelectGameUnit.cs →   _Bridges community 2 → community 8_
- `SelectPlayerCount` --inherits--> `MonoBehaviour`  [EXTRACTED]
  C:/Users/jerry/OneDrive/Documents/GitHub/BabyCareGame/Assets/Scripts/Game/Function/SelectPlayerCount.cs →   _Bridges community 2 → community 10_
- `SpinFateManager` --inherits--> `MonoBehaviour`  [EXTRACTED]
  C:/Users/jerry/OneDrive/Documents/GitHub/BabyCareGame/Assets/Scripts/Game/SpinFateManager.cs →   _Bridges community 2 → community 3_

## Import Cycles
- None detected.

## Communities (17 total, 0 thin omitted)

### Community 0 - "AR 圖像追蹤"
Cohesion: 0.09
Nodes (13): ImageTrackingController, ARTrackedImage, ARTrackedImageManager, ARTrackedImagesChangedEventArgs, bool, float, GameObject, int (+5 more)

### Community 1 - "遊戲核心管理"
Cohesion: 0.11
Nodes (9): GameData, GameObject, List, PlayerData, Sprite, GameManager, Manager, SelectGameUnit (+1 more)

### Community 2 - "共用 Unity 型別"
Cohesion: 0.09
Nodes (15): int, string, int, Vector2, SwitchScene, CardModel, ModelInfo, GridLayoutGroup (+7 more)

### Community 3 - "命運轉盤系統"
Cohesion: 0.15
Nodes (12): bool, Button, float, GameObject, IEnumerator, List, RectTransform, string (+4 more)

### Community 4 - "玩家 UI 管理"
Cohesion: 0.12
Nodes (11): GameData, GameManager, GameObject, int, List, PlayerData, Sprite, Transform (+3 more)

### Community 5 - "轉場動畫控制"
Cohesion: 0.18
Nodes (10): AnimationCurve, CanvasGroup, float, IEnumerator, RectTransform, TMP_Text, Vector2, Coroutine (+2 more)

### Community 6 - "AR 卡牌套用"
Cohesion: 0.17
Nodes (7): ARCardManager, GameManager, GameObject, SwitchScene, Text, ImageTrackingController, ModelInfo

### Community 7 - "玩家分數控制"
Cohesion: 0.14
Nodes (7): GameData, GameManager, List, PlayerData, TMP_Text, Player, PlayerController

### Community 8 - "遊戲單元選擇"
Cohesion: 0.19
Nodes (7): Image, int, Sprite, string, TMP_Text, Function.Select, SelectGameUnit

### Community 9 - "結算結果顯示"
Cohesion: 0.15
Nodes (7): GameManager, GameObject, Image, List, PlayerData, SwitchScene, GameResultDisplay

### Community 10 - "玩家人數選擇"
Cohesion: 0.21
Nodes (6): Image, int, Sprite, TMP_Text, Function.Select, SelectPlayerCount

### Community 11 - "遊戲按鈕控制"
Cohesion: 0.18
Nodes (6): Button, GameManager, GameObject, SwitchScene, Manager, UserBtnController

### Community 12 - "場景淡入切換"
Cohesion: 0.35
Nodes (4): CanvasGroup, float, IEnumerator, SwitchScene

### Community 13 - "標題閃爍效果"
Cohesion: 0.22
Nodes (6): bool, float, IEnumerator, TMP_Text, Menu, TitleBlink

### Community 14 - "教學面板控制"
Cohesion: 0.31
Nodes (4): Image, int, Sprite, TeachPanelControl

### Community 15 - "玩家資料模型"
Cohesion: 0.33
Nodes (5): bool, int, Sprite, string, PlayerData

### Community 16 - "遊戲資料模型"
Cohesion: 0.50
Nodes (3): bool, int, GameData

## Knowledge Gaps
- **98 isolated node(s):** `GameManager`, `ImageTrackingController`, `GameObject`, `Text`, `SwitchScene` (+93 more)
  These have ≤1 connection - possible missing edges or undocumented components.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `ImageTrackingController` connect `AR 圖像追蹤` to `共用 Unity 型別`?**
  _High betweenness centrality (0.178) - this node is a cross-community bridge._
- **Why does `GameManager` connect `遊戲核心管理` to `共用 Unity 型別`?**
  _High betweenness centrality (0.178) - this node is a cross-community bridge._
- **Why does `SpinFateManager` connect `命運轉盤系統` to `共用 Unity 型別`?**
  _High betweenness centrality (0.142) - this node is a cross-community bridge._
- **What connects `GameManager`, `ImageTrackingController`, `GameObject` to the rest of the system?**
  _98 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `AR 圖像追蹤` be split into smaller, more focused modules?**
  _Cohesion score 0.09333333333333334 - nodes in this community are weakly interconnected._
- **Should `遊戲核心管理` be split into smaller, more focused modules?**
  _Cohesion score 0.10666666666666667 - nodes in this community are weakly interconnected._
- **Should `共用 Unity 型別` be split into smaller, more focused modules?**
  _Cohesion score 0.08695652173913043 - nodes in this community are weakly interconnected._