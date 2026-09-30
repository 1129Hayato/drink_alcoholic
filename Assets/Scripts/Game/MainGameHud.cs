using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DrinkAlcoholic
{
    public class MainGameHud : MonoBehaviour
    {
        [SerializeField] Button startButton;
        [SerializeField] PourButton pourButton;
        [SerializeField] Text scoreText;

        public event Action OnStartClicked;

        /// <summary>「飲ませる」ボタン長押し中、またはスペースキー押下中。</summary>
        public bool IsPouring =>
            pourButton.isActiveAndEnabled &&
            (pourButton.IsPressed || (Keyboard.current != null && Keyboard.current.spaceKey.isPressed));

        void Awake() => startButton.onClick.AddListener(() => OnStartClicked?.Invoke());

        public void SetScore(int count) => scoreText.text = $"飲ませた本数：{count} 本";

        public void SetState(GameState state)
        {
            startButton.gameObject.SetActive(state == GameState.Ready);
            pourButton.gameObject.SetActive(state == GameState.Playing);
        }
    }
}
