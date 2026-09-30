using System;
using UnityEngine;
using UnityEngine.UI;

namespace DrinkAlcoholic
{
    /// <summary>右側：飲ませる人。ランダムな酒を1本持つ。</summary>
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] LiquorFactory factory;
        [SerializeField] Image bottleImage;
        [Tooltip("残量表示。anchorMax.y を残量比率に合わせる")]
        [SerializeField] RectTransform bottleFill;
        [SerializeField] Text liquorLabel;

        public Liquor CurrentLiquor { get; private set; }

        public event Action OnBottleEmptied;

        public void TakeNewBottle()
        {
            CurrentLiquor = factory.CreateRandom();
            var data = CurrentLiquor.Data;
            if (data.sprite != null)
            {
                bottleImage.sprite = data.sprite;
                bottleImage.color = Color.white;
            }
            else
            {
                bottleImage.sprite = null;
                bottleImage.color = data.color;
            }
            liquorLabel.text = $"{data.liquorName}\n{data.alcoholPercent:0.#}%  {data.volumeMl:0}ml";
            RefreshBottle();
        }

        public void PourTo(DrinkerTarget target, float mlPerSec, float deltaTime)
        {
            if (CurrentLiquor == null) return;
            float ml = CurrentLiquor.Pour(mlPerSec * deltaTime);
            target.Drink(ml, CurrentLiquor.Data.AlcoholRatio);
            RefreshBottle();
            if (CurrentLiquor.IsEmpty) OnBottleEmptied?.Invoke();
        }

        void RefreshBottle()
        {
            var anchorMax = bottleFill.anchorMax;
            anchorMax.y = CurrentLiquor.RemainingRatio;
            bottleFill.anchorMax = anchorMax;
        }
    }
}
