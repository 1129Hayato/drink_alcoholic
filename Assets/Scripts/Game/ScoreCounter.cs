using System;

namespace DrinkAlcoholic
{
    public class ScoreCounter
    {
        public int BottleCount { get; private set; }

        public event Action<int> OnChanged;

        public void AddBottle()
        {
            BottleCount++;
            OnChanged?.Invoke(BottleCount);
        }

        public void Reset()
        {
            BottleCount = 0;
            OnChanged?.Invoke(BottleCount);
        }
    }
}
