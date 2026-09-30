using UnityEngine;

namespace DrinkAlcoholic
{
    public class MainGameController : MonoBehaviour
    {
        [SerializeField] GameConfig config;
        [SerializeField] PlayerController player;
        [SerializeField] DrinkerTarget target;
        [SerializeField] AlcoholGaugeView gaugeView;
        [SerializeField] MainGameHud hud;
        [SerializeField] GameOverPanel gameOverPanel;

        [Header("Audio（任意）")]
        [SerializeField] AudioClip bgm;
        [SerializeField] AudioClip startSe;
        [SerializeField] AudioClip bottleEmptySe;
        [SerializeField] AudioClip gameOverSe;

        readonly ScoreCounter score = new ScoreCounter();

        public GameState State { get; private set; }

        void Awake()
        {
            hud.OnStartClicked += StartParty;
            score.OnChanged += hud.SetScore;
            player.OnBottleEmptied += HandleBottleEmptied;
        }

        void Start()
        {
            gameOverPanel.Hide();
            score.Reset();
            SetState(GameState.Ready);
            AudioManager.Instance.PlayBgm(bgm);
        }

        public void StartParty()
        {
            if (State != GameState.Ready) return;

            target.Initialize(config);
            gaugeView.Bind(target.Gauge);
            gaugeView.ShowFor(config.gaugeVisibleSeconds);
            score.Reset();
            player.TakeNewBottle();

            AudioManager.Instance.PlaySe(startSe);
            SetState(GameState.Playing);
        }

        void Update()
        {
            if (State != GameState.Playing) return;

            float dt = Time.deltaTime;
            if (hud.IsPouring) player.PourTo(target, config.pourRateMlPerSec, dt);
            target.Gauge.Tick(dt);

            if (target.Gauge.IsOverflow) HandleGameOver();
        }

        void HandleBottleEmptied()
        {
            // ゲージを超えたのと同じフレームで飲み切った1本もカウントする
            score.AddBottle();
            AudioManager.Instance.PlaySe(bottleEmptySe);
            player.TakeNewBottle();
        }

        void HandleGameOver()
        {
            SetState(GameState.GameOver);
            AudioManager.Instance.PlaySe(gameOverSe);
            gameOverPanel.Show(score.BottleCount);
        }

        void SetState(GameState state)
        {
            State = state;
            hud.SetState(state);
        }
    }
}
