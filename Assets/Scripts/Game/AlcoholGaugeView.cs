using UnityEngine;

namespace DrinkAlcoholic
{
    /// <summary>ゲージ表示。指定秒数だけ見せて、その後は非表示にする（値は裏で更新され続ける）。</summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class AlcoholGaugeView : MonoBehaviour
    {
        [Tooltip("中身。anchorMax.y を 0〜1 に動かして伸縮させる")]
        [SerializeField] RectTransform fill;

        CanvasGroup canvasGroup;
        AlcoholGauge gauge;
        float visibleTimer;

        void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            SetVisible(false);
        }

        public void Bind(AlcoholGauge newGauge)
        {
            Unbind();
            gauge = newGauge;
            gauge.OnChanged += Refresh;
            Refresh(gauge.Current, gauge.Max);
        }

        public void ShowFor(float seconds)
        {
            visibleTimer = seconds;
            SetVisible(true);
        }

        void Update()
        {
            if (visibleTimer <= 0f) return;
            visibleTimer -= Time.deltaTime;
            if (visibleTimer <= 0f) SetVisible(false);
        }

        void OnDestroy() => Unbind();

        void Unbind()
        {
            if (gauge != null) gauge.OnChanged -= Refresh;
            gauge = null;
        }

        void Refresh(float current, float max)
        {
            var anchorMax = fill.anchorMax;
            anchorMax.y = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            fill.anchorMax = anchorMax;
        }

        void SetVisible(bool visible) => canvasGroup.alpha = visible ? 1f : 0f;
    }
}
