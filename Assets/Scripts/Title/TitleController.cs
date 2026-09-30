using UnityEngine;
using UnityEngine.UI;

namespace DrinkAlcoholic
{
    public class TitleController : MonoBehaviour
    {
        [SerializeField] Button startButton;
        [SerializeField] Button settingsButton;
        [SerializeField] SettingsPanel settingsPanel;
        [SerializeField] AudioClip bgm;
        [SerializeField] AudioClip clickSe;

        void Awake()
        {
            startButton.onClick.AddListener(OnClickStart);
            settingsButton.onClick.AddListener(OnClickSettings);
            settingsPanel.Close();
        }

        void Start() => AudioManager.Instance.PlayBgm(bgm);

        public void OnClickStart()
        {
            AudioManager.Instance.PlaySe(clickSe);
            SceneLoader.LoadMainGame();
        }

        public void OnClickSettings()
        {
            AudioManager.Instance.PlaySe(clickSe);
            settingsPanel.Open();
        }
    }
}
