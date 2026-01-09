using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  CardModel
{
    // 定義模型類型
    public enum ModelType
    {
        Foundation, //獲得點數
        Building, //建造建築物並且得到點數
        Remove, //移除別人的建築物,並扣掉該建築物的環保值
        Reduce, //減少別人的環保值
        Special, //功能卡牌有特殊能力
        Location,//地域卡
    }
    
    //紀錄每一個模型的分數數值 後續再掃描模型時才可以得知該回合的得分為何
    public class ModelInfo : MonoBehaviour
    {
        //卡片的名稱
        public string modelName;
        //選擇卡片的種類
        public ModelType modelType;
        //卡片顏色
        public  BuildingColor colorType;
        //紀錄模型可獲的的分數，就算是扣的分數也可以記錄
        public int score;
        //special card 的效果文字
        public string specialEffect;
    }
}

