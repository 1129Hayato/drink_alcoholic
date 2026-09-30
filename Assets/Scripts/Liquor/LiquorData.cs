using UnityEngine;

namespace DrinkAlcoholic
{
    [CreateAssetMenu(menuName = "DrinkAlcoholic/LiquorData", fileName = "Liquor")]
    public class LiquorData : ScriptableObject
    {
        public string liquorName = "ビール";
        [Tooltip("内容量(ml)")]
        public float volumeMl = 350f;
        [Tooltip("アルコール度数(%)。5 なら 5%")]
        public float alcoholPercent = 5f;
        public Sprite sprite;
        [Tooltip("スプライト未設定時の仮表示色")]
        public Color color = new Color(0.95f, 0.75f, 0.2f);

        public float AlcoholRatio => alcoholPercent / 100f;
    }
}
