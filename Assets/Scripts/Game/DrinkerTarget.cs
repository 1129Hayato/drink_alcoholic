using UnityEngine;

namespace DrinkAlcoholic
{
    /// <summary>左側：飲まされる人。</summary>
    public class DrinkerTarget : MonoBehaviour
    {
        public AlcoholGauge Gauge { get; private set; }

        public void Initialize(GameConfig config)
        {
            var decay = new PerlinDecayRate(
                config.decayMinPerSec,
                config.decayMaxPerSec,
                config.decayNoiseFrequency,
                Random.Range(0f, 1000f));
            Gauge = new AlcoholGauge(config.gaugeMax, decay);
        }

        public void Drink(float ml, float alcoholRatio) => Gauge.Add(ml * alcoholRatio);
    }
}
