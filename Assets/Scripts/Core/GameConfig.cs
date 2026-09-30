using UnityEngine;

namespace DrinkAlcoholic
{
    [CreateAssetMenu(menuName = "DrinkAlcoholic/GameConfig", fileName = "GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("飲ませる")]
        [Tooltip("1秒あたりに飲ませる量(ml)。酒の種類によらず固定")]
        public float pourRateMlPerSec = 30f;

        [Header("アルコールゲージ")]
        public float gaugeMax = 10f;
        [Tooltip("開始からゲージを表示しておく秒数")]
        public float gaugeVisibleSeconds = 10f;

        [Header("ゲージ減少（毎秒 min〜max の間をなめらかに変化）")]
        public float decayMinPerSec = 0f;
        public float decayMaxPerSec = 2f;
        [Tooltip("小さいほどゆっくり変化する")]
        public float decayNoiseFrequency = 0.3f;
    }
}
