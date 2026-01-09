using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//調整顯示樣貌的數值
namespace  UI
{
    public class PlayerGridLayoutAdjust : MonoBehaviour
    {

        [SerializeField] private GridLayoutGroup gridLayoutGroup; // Grid Layout Group 組件
        [SerializeField] private Vector2 minSpacing = new Vector2(15, 0); // 最小間距
        [SerializeField] private Vector2 maxSpacing = new Vector2(60, 0); // 最大間距
        [SerializeField] private Vector2 minCellSize = new Vector2(100, 40); // 最小單元格大小
        [SerializeField] private Vector2 maxCellSize = new Vector2(130, 40); // 最大單元格大小
        [SerializeField] private int maxPlayers = 6; // 最大玩家數量

        public void AdjustGrid(int playerCount)
        {
            if (gridLayoutGroup == null)
            {
                Debug.LogError("未設置 Grid Layout Group！");
                return;
            }

            // 動態計算間距和單元格大小
            float t = (float)playerCount / maxPlayers; // 正規化玩家數量 (0 到 1)
            Vector2 adjustedSpacing = Vector2.Lerp(maxSpacing, minSpacing, t); // 根據玩家數量插值計算間距
            Vector2 adjustedCellSize = Vector2.Lerp(maxCellSize, minCellSize, t); // 根據玩家數量插值計算單元格大小

            // 設置 Grid Layout Group 的屬性
            gridLayoutGroup.spacing = adjustedSpacing;
            gridLayoutGroup.cellSize = adjustedCellSize;

            // 設置約束為固定行數（顯示一排）
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            gridLayoutGroup.constraintCount = 1; // 行數固定為 1

            Debug.Log($"調整 Grid Layout：玩家數量 = {playerCount}, 間距 = {adjustedSpacing}, 單元格大小 = {adjustedCellSize}");

        }
    }
}
