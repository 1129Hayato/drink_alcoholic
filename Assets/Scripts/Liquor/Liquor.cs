using UnityEngine;

namespace DrinkAlcoholic
{
    /// <summary>手に持っている1本。残量を管理する。</summary>
    public class Liquor
    {
        public LiquorData Data { get; }
        public float RemainingMl { get; private set; }
        public bool IsEmpty => RemainingMl <= 0f;
        public float RemainingRatio => Data.volumeMl > 0f ? RemainingMl / Data.volumeMl : 0f;

        public Liquor(LiquorData data)
        {
            Data = data;
            RemainingMl = data.volumeMl;
        }

        /// <summary>最大 requestMl 注ぎ、実際に注げた量を返す。</summary>
        public float Pour(float requestMl)
        {
            float ml = Mathf.Min(requestMl, RemainingMl);
            RemainingMl -= ml;
            return ml;
        }
    }
}
