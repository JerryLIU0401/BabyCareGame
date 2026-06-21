using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  CardModel
{
    /// <summary>
    /// 定義卡牌模型在遊戲規則中的處理類型。
    /// </summary>
    public enum ModelType
    {
        /// <summary>
        /// 一般得分卡會依模型分數更新玩家分數，並保留播放對應動畫的擴充點。
        /// </summary>
        Foundation,
    }
    
    /// <summary>
    /// 儲存 AR 卡牌模型的遊戲資料，讓掃描流程能以同一份資料更新顯示文字與計分結果。
    /// </summary>
    public class ModelInfo : MonoBehaviour
    {
        /// <summary>
        /// 卡牌模型的內部識別名稱，型別為 string。
        /// </summary>
        [Tooltip("卡牌模型的內部識別名稱，可保留既有 Prefab 或企劃命名。")]
        public string modelName;

        /// <summary>
        /// 玩家介面要顯示的中文名稱，型別為 string。
        /// </summary>
        [Tooltip("AR 掃描成功後顯示給玩家看的中文名稱；未填時會退回 modelName 或 Reference Image 名稱。")]
        public string chineseDisplayName;
        
        /// <summary>
        /// 卡牌模型對應的遊戲規則類型，型別為 ModelType。
        /// </summary>
        [Tooltip("決定卡牌使用時要套用的遊戲規則。")]
        public ModelType modelType;
        
        /// <summary>
        /// 模型可調整的分數，型別為 int，允許正分與負分。
        /// </summary>
        [Tooltip("卡牌使用後要加到玩家身上的分數；需要扣分時可填負值。")]
        public int score;
        
        /// <summary>
        /// 特殊卡牌的效果說明，型別為 string。
        /// </summary>
        [Tooltip("特殊卡牌的效果文字，供後續功能卡或命運卡擴充使用。")]
        public string specialEffect;
    }
}

