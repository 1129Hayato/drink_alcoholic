using UnityEngine;
using UnityEngine.UI;

namespace DrinkAlcoholic
{
    public class GameOverPanel : MonoBehaviour
    {
        [SerializeField] Text messageText;
        [SerializeField] Text resultText;
        [SerializeField] Button retryButton;
        [SerializeField] Button titleButton;
        [SerializeField] string message = "文化祭短縮…";

        void Awake()
        {
            retryButton.onClick.AddListener(SceneLoader.LoadMainGame);
            titleButton.onClick.AddListener(SceneLoader.LoadTitle);
        }

        public void Show(int bottleCount)
        {
            messageText.text = message;
            resultText.text = $"急性アルコール中毒発生\n飲ませた本数：{bottleCount} 本";
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
