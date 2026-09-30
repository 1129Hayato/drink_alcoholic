using System;
using UnityEngine;

namespace DrinkAlcoholic
{
    public class AlcoholGauge
    {
        readonly IDecayRate decayRate;
        float elapsed;

        public float Current { get; private set; }
        public float Max { get; }
        public bool IsOverflow => Current > Max;

        /// <summary>(current, max)</summary>
        public event Action<float, float> OnChanged;
        public event Action OnOverflow;

        public AlcoholGauge(float max, IDecayRate decayRate)
        {
            Max = max;
            this.decayRate = decayRate;
        }

        public void Add(float amount)
        {
            if (amount <= 0f || IsOverflow) return;
            Current += amount;
            OnChanged?.Invoke(Current, Max);
            if (IsOverflow) OnOverflow?.Invoke();
        }

        public void Tick(float deltaTime)
        {
            elapsed += deltaTime;
            if (IsOverflow || Current <= 0f) return;
            Current = Mathf.Max(0f, Current - decayRate.Evaluate(elapsed) * deltaTime);
            OnChanged?.Invoke(Current, Max);
        }
    }
}
