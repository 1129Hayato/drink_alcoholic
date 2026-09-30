using UnityEngine.SceneManagement;

namespace DrinkAlcoholic
{
    public static class SceneLoader
    {
        public const string TitleScene = "Title";
        public const string MainGameScene = "MainGame";

        public static void LoadTitle() => SceneManager.LoadScene(TitleScene);
        public static void LoadMainGame() => SceneManager.LoadScene(MainGameScene);
    }
}
