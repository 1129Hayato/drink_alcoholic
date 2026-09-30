using UnityEngine;
using UnityEngine.EventSystems;

namespace DrinkAlcoholic
{
    /// <summary>長押し中だけ IsPressed が true になるボタン。</summary>
    public class PourButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public bool IsPressed { get; private set; }

        public void OnPointerDown(PointerEventData eventData) => IsPressed = true;
        public void OnPointerUp(PointerEventData eventData) => IsPressed = false;
        public void OnPointerExit(PointerEventData eventData) => IsPressed = false;

        void OnDisable() => IsPressed = false;
    }
}
