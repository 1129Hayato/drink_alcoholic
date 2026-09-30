using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace DrinkAlcoholic.EditorTools
{
    /// <summary>
    /// 仮グラフィックでシーンとデータアセットを一括生成する。
    /// メニュー「DrinkAlcoholic/シーンとデータを生成」から実行。
    /// データアセット(Assets/Data)は既存なら上書きしない。シーンは作り直す。
    /// </summary>
    public static class ProjectBuilder
    {
        const string DataDir = "Assets/Data";
        const string SceneDir = "Assets/Scenes";
        static readonly string TitlePath = $"{SceneDir}/{SceneLoader.TitleScene}.unity";
        static readonly string MainGamePath = $"{SceneDir}/{SceneLoader.MainGameScene}.unity";

        static Font font;
        static Sprite circle;

        [MenuItem("DrinkAlcoholic/シーンとデータを生成")]
        public static void BuildAll()
        {
            bool exists = AssetDatabase.LoadAssetAtPath<SceneAsset>(TitlePath) != null
                          || AssetDatabase.LoadAssetAtPath<SceneAsset>(MainGamePath) != null;
            if (exists && !EditorUtility.DisplayDialog("シーン再生成",
                    "Title / MainGame シーンを作り直します。シーンへの手動変更は失われます。", "実行", "キャンセル"))
                return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            circle = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

            EnsureFolder(DataDir);
            EnsureFolder(SceneDir);

            var config = LoadOrCreate<GameConfig>($"{DataDir}/GameConfig.asset", _ => { });
            var liquors = new[]
            {
                LoadOrCreate<LiquorData>($"{DataDir}/Liquor_Beer.asset", l =>
                {
                    l.liquorName = "ビール";
                    l.volumeMl = 350f;
                    l.alcoholPercent = 5f;
                    l.color = new Color(0.96f, 0.76f, 0.18f);
                }),
                LoadOrCreate<LiquorData>($"{DataDir}/Liquor_Highball.asset", l =>
                {
                    l.liquorName = "ハイボール";
                    l.volumeMl = 350f;
                    l.alcoholPercent = 7f;
                    l.color = new Color(0.85f, 0.6f, 0.3f);
                }),
                LoadOrCreate<LiquorData>($"{DataDir}/Liquor_Sake.asset", l =>
                {
                    l.liquorName = "日本酒";
                    l.volumeMl = 180f;
                    l.alcoholPercent = 15f;
                    l.color = new Color(0.9f, 0.93f, 0.97f);
                }),
            };
            AssetDatabase.SaveAssets();

            BuildTitleScene();
            BuildMainGameScene(config, liquors);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(TitlePath, true),
                new EditorBuildSettingsScene(MainGamePath, true),
            };
            EditorSceneManager.OpenScene(TitlePath);
            Debug.Log("[DrinkAlcoholic] シーンとデータを生成しました。Title シーンから再生してください。");
        }

        // ================= Title =================

        static void BuildTitleScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(new Color(0.15f, 0.1f, 0.08f));
            CreateEventSystem();
            var canvas = CreateCanvas();

            Stretch(CreateImage("Background", canvas, new Color(0.35f, 0.12f, 0.1f)));
            var title = CreateText("TitleText", canvas, "飲み会\n〜実行委員の務め〜", 110, Color.white);
            Place(title.rectTransform, new Vector2(0.5f, 0.68f), new Vector2(1400, 400));

            var start = CreateButton("StartButton", canvas, "スタート", new Color(0.9f, 0.55f, 0.15f));
            Place(start.transform as RectTransform, new Vector2(0.5f, 0.32f), new Vector2(480, 110));
            var settings = CreateButton("SettingsButton", canvas, "設定", new Color(0.5f, 0.5f, 0.5f));
            Place(settings.transform as RectTransform, new Vector2(0.5f, 0.18f), new Vector2(480, 110));

            // 設定パネル
            var panelRoot = CreateImage("SettingsPanel", canvas, new Color(0f, 0f, 0f, 0.6f));
            Stretch(panelRoot);
            var box = CreateImage("Box", panelRoot.transform, new Color(0.95f, 0.92f, 0.85f));
            Place(box.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(900, 600));

            var header = CreateText("Header", box.transform, "設定", 64, Color.black);
            Place(header.rectTransform, new Vector2(0.5f, 0.85f), new Vector2(600, 100));

            var bgmLabel = CreateText("BgmLabel", box.transform, "BGM", 48, Color.black);
            Place(bgmLabel.rectTransform, new Vector2(0.18f, 0.62f), new Vector2(200, 80));
            var bgmSlider = CreateSlider("BgmSlider", box.transform);
            Place(bgmSlider.transform as RectTransform, new Vector2(0.6f, 0.62f), new Vector2(520, 40));

            var seLabel = CreateText("SeLabel", box.transform, "SE", 48, Color.black);
            Place(seLabel.rectTransform, new Vector2(0.18f, 0.42f), new Vector2(200, 80));
            var seSlider = CreateSlider("SeSlider", box.transform);
            Place(seSlider.transform as RectTransform, new Vector2(0.6f, 0.42f), new Vector2(520, 40));

            var close = CreateButton("CloseButton", box.transform, "閉じる", new Color(0.4f, 0.4f, 0.4f));
            Place(close.transform as RectTransform, new Vector2(0.5f, 0.15f), new Vector2(360, 100));

            var settingsPanel = panelRoot.gameObject.AddComponent<SettingsPanel>();
            Wire(settingsPanel, "bgmSlider", bgmSlider);
            Wire(settingsPanel, "seSlider", seSlider);
            Wire(settingsPanel, "closeButton", close);
            panelRoot.gameObject.SetActive(false);

            var controller = new GameObject("TitleController").AddComponent<TitleController>();
            Wire(controller, "startButton", start);
            Wire(controller, "settingsButton", settings);
            Wire(controller, "settingsPanel", settingsPanel);

            EditorSceneManager.SaveScene(scene, TitlePath);
        }

        // ================= MainGame =================

        static void BuildMainGameScene(GameConfig config, LiquorData[] liquors)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(Color.black);
            CreateEventSystem();
            var canvas = CreateCanvas();

            BuildBanquetHall(canvas);

            // 左：飲まされる人
            var targetRoot = CreatePerson("Target", canvas, new Vector2(-420f, 170f),
                new Color(0.25f, 0.4f, 0.75f), "実行委員");
            var target = targetRoot.gameObject.AddComponent<DrinkerTarget>();

            // ゲージ：相手の横、足元〜身長の80%
            var gaugeRect = CreateRect("AlcoholGauge", targetRoot);
            gaugeRect.anchorMin = new Vector2(1f, 0f);
            gaugeRect.anchorMax = new Vector2(1f, 0.8f);
            gaugeRect.pivot = new Vector2(0f, 0f);
            gaugeRect.anchoredPosition = new Vector2(30f, 0f);
            gaugeRect.sizeDelta = new Vector2(44f, 0f);
            var gaugeBg = gaugeRect.gameObject.AddComponent<Image>();
            gaugeBg.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
            var gaugeFill = CreateImage("Fill", gaugeRect, new Color(0.9f, 0.15f, 0.15f));
            gaugeFill.rectTransform.anchorMin = Vector2.zero;
            gaugeFill.rectTransform.anchorMax = new Vector2(1f, 0f);
            gaugeFill.rectTransform.offsetMin = new Vector2(4f, 4f);
            gaugeFill.rectTransform.offsetMax = new Vector2(-4f, 0f);
            gaugeRect.gameObject.AddComponent<CanvasGroup>();
            var gaugeView = gaugeRect.gameObject.AddComponent<AlcoholGaugeView>();
            Wire(gaugeView, "fill", gaugeFill.rectTransform);

            // 右：プレイヤー
            var playerRoot = CreatePerson("Player", canvas, new Vector2(420f, 170f),
                new Color(0.3f, 0.3f, 0.3f), "あなた");
            var player = playerRoot.gameObject.AddComponent<PlayerController>();

            var bottle = CreateImage("Bottle", playerRoot, new Color(1f, 1f, 1f, 0.35f));
            bottle.rectTransform.anchorMin = bottle.rectTransform.anchorMax = new Vector2(0f, 0.45f);
            bottle.rectTransform.pivot = new Vector2(1f, 0.5f);
            bottle.rectTransform.anchoredPosition = new Vector2(-20f, 0f);
            bottle.rectTransform.sizeDelta = new Vector2(80f, 200f);
            var bottleFill = CreateImage("Liquid", bottle.transform, Color.yellow);
            bottleFill.rectTransform.anchorMin = Vector2.zero;
            bottleFill.rectTransform.anchorMax = Vector2.one;
            bottleFill.rectTransform.offsetMin = new Vector2(6f, 6f);
            bottleFill.rectTransform.offsetMax = new Vector2(-6f, 0f);
            var liquorLabel = CreateText("LiquorLabel", bottle.transform, "", 30, Color.black);
            liquorLabel.rectTransform.anchorMin = liquorLabel.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            liquorLabel.rectTransform.pivot = new Vector2(0.5f, 0f);
            liquorLabel.rectTransform.anchoredPosition = new Vector2(0f, 10f);
            liquorLabel.rectTransform.sizeDelta = new Vector2(320f, 90f);

            var factory = new GameObject("LiquorFactory").AddComponent<LiquorFactory>();
            WireArray(factory, "liquorTable", liquors);
            Wire(player, "factory", factory);
            Wire(player, "bottleImage", bottleFill);
            Wire(player, "bottleFill", bottleFill.rectTransform);
            Wire(player, "liquorLabel", liquorLabel);

            // HUD
            var hudRect = CreateRect("Hud", canvas);
            Stretch(hudRect);
            var hud = hudRect.gameObject.AddComponent<MainGameHud>();

            var scoreText = CreateText("ScoreText", hudRect, "飲ませた本数：0 本", 56, Color.white);
            scoreText.alignment = TextAnchor.UpperLeft;
            scoreText.gameObject.AddComponent<Outline>().effectDistance = new Vector2(3f, -3f);
            scoreText.rectTransform.anchorMin = scoreText.rectTransform.anchorMax = new Vector2(0f, 1f);
            scoreText.rectTransform.pivot = new Vector2(0f, 1f);
            scoreText.rectTransform.anchoredPosition = new Vector2(40f, -30f);
            scoreText.rectTransform.sizeDelta = new Vector2(900f, 80f);

            var startButton = CreateButton("StartButton", hudRect, "飲み会スタート", new Color(0.9f, 0.55f, 0.15f));
            Place(startButton.transform as RectTransform, new Vector2(0.5f, 0.5f), new Vector2(620, 150));

            var pourButton = CreateButton("PourButton", hudRect, "飲ませる（長押し / Space）", new Color(0.8f, 0.2f, 0.2f));
            Place(pourButton.transform as RectTransform, new Vector2(0.5f, 0.09f), new Vector2(760, 130));
            var pour = pourButton.gameObject.AddComponent<PourButton>();

            Wire(hud, "startButton", startButton);
            Wire(hud, "pourButton", pour);
            Wire(hud, "scoreText", scoreText);

            // ゲームオーバー
            var overRoot = CreateImage("GameOverPanel", canvas, new Color(0f, 0f, 0f, 0.8f));
            Stretch(overRoot);
            var message = CreateText("MessageText", overRoot.transform, "文化祭短縮…", 150, new Color(1f, 0.25f, 0.25f));
            Place(message.rectTransform, new Vector2(0.5f, 0.68f), new Vector2(1600, 220));
            var result = CreateText("ResultText", overRoot.transform, "", 56, Color.white);
            Place(result.rectTransform, new Vector2(0.5f, 0.45f), new Vector2(1400, 180));
            var retry = CreateButton("RetryButton", overRoot.transform, "飲みなおす", new Color(0.9f, 0.55f, 0.15f));
            Place(retry.transform as RectTransform, new Vector2(0.38f, 0.2f), new Vector2(480, 120));
            var toTitle = CreateButton("TitleButton", overRoot.transform, "タイトル", new Color(0.5f, 0.5f, 0.5f));
            Place(toTitle.transform as RectTransform, new Vector2(0.62f, 0.2f), new Vector2(480, 120));

            var gameOver = overRoot.gameObject.AddComponent<GameOverPanel>();
            Wire(gameOver, "messageText", message);
            Wire(gameOver, "resultText", result);
            Wire(gameOver, "retryButton", retry);
            Wire(gameOver, "titleButton", toTitle);
            overRoot.gameObject.SetActive(false);

            // 進行
            var controller = new GameObject("MainGameController").AddComponent<MainGameController>();
            Wire(controller, "config", config);
            Wire(controller, "player", player);
            Wire(controller, "target", target);
            Wire(controller, "gaugeView", gaugeView);
            Wire(controller, "hud", hud);
            Wire(controller, "gameOverPanel", gameOver);

            EditorSceneManager.SaveScene(scene, MainGamePath);
        }

        /// <summary>宴会場（畳の大広間）の仮背景。</summary>
        static void BuildBanquetHall(Transform canvas)
        {
            var wall = CreateImage("Wall", canvas, new Color(0.93f, 0.86f, 0.7f));
            wall.rectTransform.anchorMin = new Vector2(0f, 0.45f);
            wall.rectTransform.anchorMax = Vector2.one;
            wall.rectTransform.offsetMin = wall.rectTransform.offsetMax = Vector2.zero;

            // 障子の桟っぽい縦線
            for (int i = 1; i < 8; i++)
            {
                var bar = CreateImage($"Frame{i}", wall.transform, new Color(0.55f, 0.4f, 0.25f));
                bar.rectTransform.anchorMin = new Vector2(i / 8f, 0f);
                bar.rectTransform.anchorMax = new Vector2(i / 8f, 1f);
                bar.rectTransform.sizeDelta = new Vector2(14f, 0f);
            }

            var banner = CreateImage("Banner", wall.transform, Color.white);
            Place(banner.rectTransform, new Vector2(0.5f, 0.72f), new Vector2(1100f, 130f));
            banner.gameObject.AddComponent<Outline>().effectColor = new Color(0.7f, 0.1f, 0.1f);
            var bannerText = CreateText("Text", banner.transform, "祝 文化祭実行委員会 打ち上げ", 64, new Color(0.7f, 0.1f, 0.1f));
            Stretch(bannerText.rectTransform);

            var floor = CreateImage("Tatami", canvas, new Color(0.62f, 0.66f, 0.38f));
            floor.rectTransform.anchorMin = Vector2.zero;
            floor.rectTransform.anchorMax = new Vector2(1f, 0.45f);
            floor.rectTransform.offsetMin = floor.rectTransform.offsetMax = Vector2.zero;

            const int cols = 5, rows = 2;
            for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
            {
                var mat = CreateImage($"Mat{x}_{y}", floor.transform, new Color(0.78f, 0.8f, 0.52f));
                mat.rectTransform.anchorMin = new Vector2((float)x / cols, (float)y / rows);
                mat.rectTransform.anchorMax = new Vector2((float)(x + 1) / cols, (float)(y + 1) / rows);
                mat.rectTransform.offsetMin = new Vector2(5f, 5f);
                mat.rectTransform.offsetMax = new Vector2(-5f, -5f);
            }

            var table = CreateImage("LowTable", canvas, new Color(0.4f, 0.22f, 0.12f));
            table.rectTransform.anchorMin = table.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            table.rectTransform.pivot = new Vector2(0.5f, 0f);
            table.rectTransform.anchoredPosition = new Vector2(0f, 250f);
            table.rectTransform.sizeDelta = new Vector2(1500f, 60f);
        }

        /// <summary>頭（丸）＋胴体の仮キャラ。pivot は足元。</summary>
        static RectTransform CreatePerson(string name, Transform parent, Vector2 footPos, Color color, string label)
        {
            var root = CreateRect(name, parent);
            root.anchorMin = root.anchorMax = new Vector2(0.5f, 0f);
            root.pivot = new Vector2(0.5f, 0f);
            root.anchoredPosition = footPos;
            root.sizeDelta = new Vector2(260f, 560f);

            var body = CreateImage("Body", root, color);
            body.rectTransform.anchorMin = Vector2.zero;
            body.rectTransform.anchorMax = new Vector2(1f, 0.72f);
            body.rectTransform.offsetMin = body.rectTransform.offsetMax = Vector2.zero;

            var head = CreateImage("Head", root, new Color(1f, 0.85f, 0.7f));
            head.sprite = circle;
            head.rectTransform.anchorMin = head.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            head.rectTransform.pivot = new Vector2(0.5f, 1f);
            head.rectTransform.sizeDelta = new Vector2(160f, 160f);

            var text = CreateText("Name", root, label, 40, Color.white);
            text.rectTransform.anchorMin = text.rectTransform.anchorMax = new Vector2(0.5f, 0.35f);
            text.rectTransform.sizeDelta = new Vector2(260f, 60f);
            return root;
        }

        // ================= helpers =================

        static void CreateCamera(Color background)
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = background;
            go.transform.position = new Vector3(0f, 0f, -10f);
        }

        static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem", typeof(EventSystem));
            go.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        static Transform CreateCanvas()
        {
            var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return go.transform;
        }

        static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        static Image CreateImage(string name, Transform parent, Color color)
        {
            var image = CreateRect(name, parent).gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        static Text CreateText(string name, Transform parent, string text, int size, Color color)
        {
            var t = CreateRect(name, parent).gameObject.AddComponent<Text>();
            t.font = font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        static Button CreateButton(string name, Transform parent, string label, Color color)
        {
            var image = CreateImage(name, parent, color);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var text = CreateText("Label", image.transform, label, 52, Color.white);
            Stretch(text.rectTransform);
            return button;
        }

        static Slider CreateSlider(string name, Transform parent)
        {
            var root = CreateRect(name, parent);
            var bg = CreateImage("Background", root, new Color(0.3f, 0.3f, 0.3f));
            bg.rectTransform.anchorMin = new Vector2(0f, 0.3f);
            bg.rectTransform.anchorMax = new Vector2(1f, 0.7f);
            bg.rectTransform.offsetMin = bg.rectTransform.offsetMax = Vector2.zero;

            var fillArea = CreateRect("Fill Area", root);
            fillArea.anchorMin = new Vector2(0f, 0.3f);
            fillArea.anchorMax = new Vector2(1f, 0.7f);
            fillArea.offsetMin = fillArea.offsetMax = Vector2.zero;
            var fill = CreateImage("Fill", fillArea, new Color(0.9f, 0.55f, 0.15f));
            fill.rectTransform.sizeDelta = Vector2.zero;

            var handleArea = CreateRect("Handle Slide Area", root);
            Stretch(handleArea);
            var handle = CreateImage("Handle", handleArea, Color.white);
            handle.sprite = circle;
            handle.rectTransform.anchorMin = Vector2.zero;
            handle.rectTransform.anchorMax = new Vector2(0f, 1f);
            handle.rectTransform.sizeDelta = new Vector2(40f, 0f);

            var slider = root.gameObject.AddComponent<Slider>();
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.8f;
            return slider;
        }

        static void Stretch(Graphic graphic) => Stretch(graphic.rectTransform);

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        /// <summary>親に対する相対位置(0〜1)を中心に、指定サイズで配置する。</summary>
        static void Place(RectTransform rect, Vector2 anchor, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;
        }

        static void Wire(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field) ?? throw new ArgumentException($"{target.GetType().Name}.{field} が見つかりません");
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void WireArray(Object target, string field, Object[] values)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field) ?? throw new ArgumentException($"{target.GetType().Name}.{field} が見つかりません");
            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static T LoadOrCreate<T>(string path, Action<T> init) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            init(asset);
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/');
            AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
        }
    }
}
