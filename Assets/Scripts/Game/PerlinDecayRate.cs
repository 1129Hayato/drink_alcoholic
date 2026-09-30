using UnityEngine;

namespace DrinkAlcoholic
{
    /// <summary>Perlin ノイズで min〜max の間を連続的に変化する減少量。</summary>
    public class PerlinDecayRate : IDecayRate
    {
        readonly float min;
        readonly float max;
        readonly float frequency;
        readonly float seed;

        public PerlinDecayRate(float min, float max, float frequency, float seed)
        {
            this.min = min;
            this.max = max;
            this.frequency = frequency;
            this.seed = seed;
        }

        public float Evaluate(float time)
        {
            // PerlinNoise は 0〜1 をわずかにはみ出すことがあるので丸める
            float n = Mathf.Clamp01(Mathf.PerlinNoise(time * frequency, seed));
            return Mathf.Lerp(min, max, n);
        }
    }
}
