using System.Collections.Generic;
using Manager;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 負責在遊戲結算時依玩家分數顯示排名結果。
/// </summary>
public class GameResultDisplay : MonoBehaviour
{
    /// <summary>
    /// 保存單一玩家的結算排名資料，讓排序邏輯不需要改動 GameManager 內的原始玩家順序。
    /// </summary>
    private class PlayerRankEntry
    {
        // 保存原始玩家資料引用，避免複製 Sprite 等 Unity 物件造成不必要的資源持有。
        public PlayerData Player { get; }

        // 原始索引用於平手時維持預設玩家順序，符合「平手按照默認排序」的規則。
        public int OriginalIndex { get; }

        /// <summary>
        /// 建立玩家排名資料。
        /// </summary>
        /// <param name="player">玩家資料，型別為 PlayerData。</param>
        /// <param name="originalIndex">玩家在原始清單中的索引，型別為 int。</param>
        /// <returns>建構函式不回傳值。</returns>
        public PlayerRankEntry(PlayerData player, int originalIndex)
        {
            Player = player;
            OriginalIndex = originalIndex;
        }
    }

    // 結算顯示只需要讀取 GameManager 事件，不直接修改遊戲資料狀態。
    private GameManager gameManager;
    
    // 每個名次區塊由場景排版決定，程式僅依玩家人數啟閉對應版位。
    [SerializeField] private GameObject[] numberBlock;

    // 玩家頭像位置需與 numberBlock 順序一致，未來名次文字或圖片也可沿用同一排序結果。
    [SerializeField] private Image[] playerImages;
    
    // 回主選單沿用既有轉場 Prefab，避免結算畫面直接切場造成視覺中斷。
    [SerializeField] private SwitchScene switchScenePrefab;

    /// <summary>
    /// 初始化結算顯示所需的 GameManager 依賴。
    /// </summary>
    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    /// <summary>
    /// 訂閱遊戲結算事件，讓面板啟用時能接收最終玩家資料。
    /// </summary>
    private void OnEnable()
    {
        if (gameManager == null)
        {
            // 場景或物件生命週期異常時保留防呆，避免結算面板啟用即中斷遊戲流程。
            gameManager = FindFirstObjectByType<GameManager>();
        }

        if (gameManager != null)
        {
            gameManager.OnGameVictory += GameResultUIDisplay;
        }
    }

    /// <summary>
    /// 解除遊戲結算事件訂閱，避免跨場景或物件銷毀後留下失效引用。
    /// </summary>
    private void OnDisable()
    {
        if (gameManager != null)
        {
            gameManager.OnGameVictory -= GameResultUIDisplay;
        }
    }
    
    /// <summary>
    /// 依玩家分數排序並顯示結算 UI。
    /// </summary>
    /// <param name="players">本局所有玩家資料，型別為 List&lt;PlayerData&gt;。</param>
    private void GameResultUIDisplay(List<PlayerData> players)
    {
        if (players == null || players.Count == 0)
        {
            // 沒有玩家資料代表設定流程尚未完成，結算畫面不應顯示空排名。
            Debug.LogWarning("[GameResultDisplay] 沒有可顯示的玩家資料。", this);
            HideAllResultBlocks();
            return;
        }

        HideAllResultBlocks();

        if (numberBlock == null || playerImages == null)
        {
            // 結算面板依賴場景中既有版位；陣列未綁定時只能停止顯示，避免空引用中斷遊戲。
            Debug.LogWarning("[GameResultDisplay] numberBlock 或 playerImages 尚未綁定。", this);
            return;
        }

        List<PlayerRankEntry> rankedPlayers = CreateRankedPlayers(players);
        int displayCount = Mathf.Min(rankedPlayers.Count, numberBlock.Length, playerImages.Length);

        if (displayCount < rankedPlayers.Count)
        {
            // 場景排版若尚未綁滿 2-4 人欄位，先顯示可用欄位並提示缺漏，避免整個結算流程崩潰。
            Debug.LogWarning($"[GameResultDisplay] 結算欄位不足，玩家數={rankedPlayers.Count}，可顯示欄位={displayCount}。", this);
        }

        for (int i = 0; i < displayCount; i++)
        {
            if (numberBlock[i] == null || playerImages[i] == null)
            {
                // 個別欄位缺綁時跳過，讓其他已完成排版的名次仍能正常顯示。
                Debug.LogWarning($"[GameResultDisplay] 第 {i + 1} 名欄位尚未完整綁定。", this);
                continue;
            }

            numberBlock[i].SetActive(true);
            playerImages[i].sprite = rankedPlayers[i].Player.playerSprite;
        }
    }

    /// <summary>
    /// 建立玩家排名清單，分數高者在前，平手時維持原本玩家順序。
    /// </summary>
    /// <param name="players">本局所有玩家資料，型別為 List&lt;PlayerData&gt;。</param>
    /// <returns>回傳依結算規則排序後的玩家排名清單，型別為 List&lt;PlayerRankEntry&gt;。</returns>
    private List<PlayerRankEntry> CreateRankedPlayers(List<PlayerData> players)
    {
        List<PlayerRankEntry> rankedPlayers = new List<PlayerRankEntry>();

        for (int i = 0; i < players.Count; i++)
        {
            // 使用原始索引保留玩家編號語意，避免排序後破壞 GameManager 的 1P、2P 對應關係。
            rankedPlayers.Add(new PlayerRankEntry(players[i], i));
        }

        rankedPlayers.Sort(ComparePlayerRank);
        return rankedPlayers;
    }

    /// <summary>
    /// 比較兩位玩家的結算排序優先順序。
    /// </summary>
    /// <param name="left">左側玩家排名資料，型別為 PlayerRankEntry。</param>
    /// <param name="right">右側玩家排名資料，型別為 PlayerRankEntry。</param>
    /// <returns>回傳排序比較值，型別為 int。</returns>
    private int ComparePlayerRank(PlayerRankEntry left, PlayerRankEntry right)
    {
        int scoreCompare = right.Player.score.CompareTo(left.Player.score);

        if (scoreCompare != 0)
        {
            // 分數是主要排名依據，因此高分必須排在低分之前。
            return scoreCompare;
        }

        // 平手不額外計算並列名次，先依需求維持預設玩家順序，未來可在此擴充同名次顯示。
        return left.OriginalIndex.CompareTo(right.OriginalIndex);
    }

    /// <summary>
    /// 關閉所有結算名次區塊。
    /// </summary>
    private void HideAllResultBlocks()
    {
        if (numberBlock == null)
        {
            // 尚未完成 Inspector 綁定時不進行任何 UI 操作，讓呼叫端可自行決定後續處理。
            return;
        }

        for (int i = 0; i < numberBlock.Length; i++)
        {
            if (numberBlock[i] == null)
            {
                // 場景仍在排版時允許空欄位存在，避免測試階段因未綁定而中斷。
                continue;
            }

            numberBlock[i].SetActive(false);
        }
    }

    /// <summary>
    /// 返回開始選單。
    /// </summary>
    public void BackToMenu()
    {
        SwitchScene switchScenes = Instantiate(switchScenePrefab);
        switchScenes.StartCoroutine(switchScenes.loadFadeOutInScenes("Start"));
    }
}
