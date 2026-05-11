using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Iteration01_Setup : EditorWindow
{
    private static readonly Color ColorBg        = new Color(0.039f, 0.055f, 0.153f, 1f);
    private static readonly Color ColorPrimary   = new Color(0.000f, 0.898f, 1.000f, 1f);
    private static readonly Color ColorAccent    = new Color(1.000f, 0.000f, 0.898f, 1f);
    private static readonly Color ColorSuccess   = new Color(0.000f, 1.000f, 0.533f, 1f);
    private static readonly Color ColorText      = Color.white;
    private static readonly Color ColorDisabled  = new Color(0.227f, 0.263f, 0.408f, 1f);
    private static readonly Color ColorPanel     = new Color(0.078f, 0.102f, 0.231f, 1f);

    private const string ScenesPath = "Assets/SporosGame/Scenes";
    private const string MainMenuScene = "MainMenu";
    private const string LevelSelectScene = "LevelSelect";
    private const string GameScene = "Game";

    [MenuItem("Tools/SporosGame/Iteration 1/Setup All Scenes")]
    public static void SetupAll()
    {
        EnsureScenesFolder();
        SetupMainMenu();
        SetupLevelSelect();
        SetupGame();
        AddScenesToBuild();
        EditorUtility.DisplayDialog("SporosGame", "Iteration 1 setup complete.\nOpen MainMenu scene and press Play.", "OK");
    }

    [MenuItem("Tools/SporosGame/Iteration 1/Setup MainMenu Scene")]
    public static void SetupMainMenu()
    {
        EnsureScenesFolder();
        var scene = CreateOrOpenScene(MainMenuScene);
        ClearScene();

        SetupCamera();
        SetupEventSystem();
        SetupBootstrap();

        var canvas = CreateCanvas("MainMenuCanvas");
        var safeArea = CreateSafeArea(canvas.transform);

        CreateBackgroundDecor(canvas.transform);

        var logo = CreateText(safeArea, "LogoText", "SPOROS", 160, FontStyles.Bold, ColorPrimary);
        var logoRt = logo.rectTransform;
        logoRt.anchorMin = new Vector2(0.5f, 1f);
        logoRt.anchorMax = new Vector2(0.5f, 1f);
        logoRt.pivot = new Vector2(0.5f, 1f);
        logoRt.anchoredPosition = new Vector2(0f, -260f);
        logoRt.sizeDelta = new Vector2(900, 220);
        AddOutline(logo, ColorAccent, 4);

        var subText = CreateText(safeArea, "SubtitleText", "neon puzzle", 50, FontStyles.Italic, new Color(1f,1f,1f,0.6f));
        var subRt = subText.rectTransform;
        subRt.anchorMin = new Vector2(0.5f, 1f);
        subRt.anchorMax = new Vector2(0.5f, 1f);
        subRt.pivot = new Vector2(0.5f, 1f);
        subRt.anchoredPosition = new Vector2(0f, -490f);
        subRt.sizeDelta = new Vector2(600, 70);

        var playBtn = CreateButton(safeArea, "PlayButton", "PLAY", ColorPrimary, ColorBg, 70);
        var playRt = playBtn.GetComponent<RectTransform>();
        playRt.anchorMin = new Vector2(0.5f, 0.5f);
        playRt.anchorMax = new Vector2(0.5f, 0.5f);
        playRt.pivot = new Vector2(0.5f, 0.5f);
        playRt.anchoredPosition = new Vector2(0, -100);
        playRt.sizeDelta = new Vector2(600, 200);

        var settingsBtn = CreateIconButton(safeArea, "SettingsButton", "⚙", ColorPanel, ColorText);
        var setRt = settingsBtn.GetComponent<RectTransform>();
        setRt.anchorMin = new Vector2(1f, 1f);
        setRt.anchorMax = new Vector2(1f, 1f);
        setRt.pivot = new Vector2(1f, 1f);
        setRt.anchoredPosition = new Vector2(-40, -40);
        setRt.sizeDelta = new Vector2(140, 140);

        var shopBtn = CreateIconButton(safeArea, "ShopButton", "★", ColorPanel, ColorAccent);
        var shopRt = shopBtn.GetComponent<RectTransform>();
        shopRt.anchorMin = new Vector2(0f, 1f);
        shopRt.anchorMax = new Vector2(0f, 1f);
        shopRt.pivot = new Vector2(0f, 1f);
        shopRt.anchoredPosition = new Vector2(40, -40);
        shopRt.sizeDelta = new Vector2(140, 140);

        var settingsPopup = CreateGenericPopup(canvas.transform, "SettingsPopup", "SETTINGS", "Settings menu placeholder");
        var shopPopup = CreateGenericPopup(canvas.transform, "ShopPopup", "SHOP", "Shop placeholder\nUnlock 10 extra levels");

        var ctrlGo = new GameObject("MainMenuController");
        ctrlGo.transform.SetParent(canvas.transform, false);
        var ctrl = ctrlGo.AddComponent<MainMenuController>();
        SerializedObject so = new SerializedObject(ctrl);
        so.FindProperty("playButton").objectReferenceValue = playBtn;
        so.FindProperty("settingsButton").objectReferenceValue = settingsBtn;
        so.FindProperty("shopButton").objectReferenceValue = shopBtn;
        so.FindProperty("logoText").objectReferenceValue = logo;
        so.FindProperty("settingsPopup").objectReferenceValue = settingsPopup;
        so.FindProperty("shopPopup").objectReferenceValue = shopPopup;
        so.ApplyModifiedProperties();

        SaveScene(scene, MainMenuScene);
    }

    [MenuItem("Tools/SporosGame/Iteration 1/Setup LevelSelect Scene")]
    public static void SetupLevelSelect()
    {
        EnsureScenesFolder();
        var scene = CreateOrOpenScene(LevelSelectScene);
        ClearScene();

        SetupCamera();
        SetupEventSystem();
        SetupBootstrap();

        var canvas = CreateCanvas("LevelSelectCanvas");
        var safeArea = CreateSafeArea(canvas.transform);

        CreateBackgroundDecor(canvas.transform);

        var title = CreateText(safeArea, "TitleText", "LEVELS", 110, FontStyles.Bold, ColorPrimary);
        var trt = title.rectTransform;
        trt.anchorMin = new Vector2(0.5f, 1f);
        trt.anchorMax = new Vector2(0.5f, 1f);
        trt.pivot = new Vector2(0.5f, 1f);
        trt.anchoredPosition = new Vector2(0, -180);
        trt.sizeDelta = new Vector2(800, 150);
        AddOutline(title, ColorAccent, 3);

        var backBtn = CreateIconButton(safeArea, "BackButton", "‹", ColorPanel, ColorText);
        var brt = backBtn.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0f, 1f);
        brt.anchorMax = new Vector2(0f, 1f);
        brt.pivot = new Vector2(0f, 1f);
        brt.anchoredPosition = new Vector2(40, -40);
        brt.sizeDelta = new Vector2(140, 140);

        var level1Btn = CreateLevelButton(safeArea, "Level1Button", "1");
        var l1rt = level1Btn.GetComponent<RectTransform>();
        l1rt.anchorMin = new Vector2(0.5f, 0.5f);
        l1rt.anchorMax = new Vector2(0.5f, 0.5f);
        l1rt.pivot = new Vector2(0.5f, 0.5f);
        l1rt.anchoredPosition = Vector2.zero;
        l1rt.sizeDelta = new Vector2(280, 280);

        var ctrlGo = new GameObject("LevelSelectController");
        ctrlGo.transform.SetParent(canvas.transform, false);
        var ctrl = ctrlGo.AddComponent<LevelSelectController>();
        SerializedObject so = new SerializedObject(ctrl);
        so.FindProperty("backButton").objectReferenceValue = backBtn;
        so.FindProperty("level1Button").objectReferenceValue = level1Btn;
        so.ApplyModifiedProperties();

        SaveScene(scene, LevelSelectScene);
    }

    [MenuItem("Tools/SporosGame/Iteration 1/Setup Game Scene")]
    public static void SetupGame()
    {
        EnsureScenesFolder();
        var scene = CreateOrOpenScene(GameScene);
        ClearScene();

        SetupCamera();
        SetupEventSystem();
        SetupBootstrap();

        var canvas = CreateCanvas("GameCanvas");
        var safeArea = CreateSafeArea(canvas.transform);

        CreateBackgroundDecor(canvas.transform);

        var backBtn = CreateIconButton(safeArea, "BackButton", "‹", ColorPanel, ColorText);
        var brt = backBtn.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0f, 1f);
        brt.anchorMax = new Vector2(0f, 1f);
        brt.pivot = new Vector2(0f, 1f);
        brt.anchoredPosition = new Vector2(40, -40);
        brt.sizeDelta = new Vector2(140, 140);

        var placeholder = CreateText(safeArea, "PlaceholderText", "GAME SCENE\nIteration 1\n— core systems ready —", 70, FontStyles.Bold, ColorPrimary);
        var pr = placeholder.rectTransform;
        pr.anchorMin = new Vector2(0.5f, 0.5f);
        pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.pivot = new Vector2(0.5f, 0.5f);
        pr.anchoredPosition = Vector2.zero;
        pr.sizeDelta = new Vector2(900, 600);
        placeholder.alignment = TextAlignmentOptions.Center;
        AddOutline(placeholder, ColorAccent, 3);

        var ctrlGo = new GameObject("GameSceneController");
        ctrlGo.transform.SetParent(canvas.transform, false);
        var ctrl = ctrlGo.AddComponent<GameSceneController>();
        SerializedObject so = new SerializedObject(ctrl);
        so.FindProperty("backButton").objectReferenceValue = backBtn;
        so.ApplyModifiedProperties();

        SaveScene(scene, GameScene);
    }

    [MenuItem("Tools/SporosGame/Iteration 1/Add Scenes To Build Settings")]
    public static void AddScenesToBuild()
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        AddSceneIfMissing(scenes, ScenesPath + "/" + MainMenuScene + ".unity");
        AddSceneIfMissing(scenes, ScenesPath + "/" + LevelSelectScene + ".unity");
        AddSceneIfMissing(scenes, ScenesPath + "/" + GameScene + ".unity");
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void AddSceneIfMissing(List<EditorBuildSettingsScene> scenes, string path)
    {
        for (int i = 0; i < scenes.Count; i++)
            if (scenes[i].path == path) return;
        scenes.Add(new EditorBuildSettingsScene(path, true));
    }

    private static void EnsureScenesFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/SporosGame"))
            AssetDatabase.CreateFolder("Assets", "SporosGame");
        if (!AssetDatabase.IsValidFolder(ScenesPath))
            AssetDatabase.CreateFolder("Assets/SporosGame", "Scenes");
    }

    private static Scene CreateOrOpenScene(string name)
    {
        var path = ScenesPath + "/" + name + ".unity";
        if (File.Exists(path))
            return EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        var s = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        return s;
    }

    private static void SaveScene(Scene scene, string name)
    {
        var path = ScenesPath + "/" + name + ".unity";
        EditorSceneManager.SaveScene(scene, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void ClearScene()
    {
        var roots = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var r in roots) Object.DestroyImmediate(r);
    }

    private static void SetupCamera()
    {
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = ColorBg;
        cam.orthographic = true;
        cam.orthographicSize = 10;
        camGo.AddComponent<AudioListener>();
    }

    private static void SetupEventSystem()
    {
        var esGo = new GameObject("EventSystem");
        esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    private static void SetupBootstrap()
    {
        var go = new GameObject("GameBootstrap");
        go.AddComponent<GameBootstrap>();
    }

    private static Canvas CreateCanvas(string name)
    {
        var go = new GameObject(name);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 1f;
        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static RectTransform CreateSafeArea(Transform parent)
    {
        var go = new GameObject("SafeArea");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        go.AddComponent<SafeAreaFitter>();
        return rt;
    }

    private static void CreateBackgroundDecor(Transform parent)
    {
        var bgGo = new GameObject("Background");
        bgGo.transform.SetParent(parent, false);
        var img = bgGo.AddComponent<Image>();
        img.color = ColorBg;
        img.raycastTarget = false;
        var rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        bgGo.transform.SetAsFirstSibling();

        for (int i = 0; i < 4; i++)
        {
            var dot = new GameObject("Glow_" + i);
            dot.transform.SetParent(bgGo.transform, false);
            var dimg = dot.AddComponent<Image>();
            dimg.sprite = CreateCircleSprite();
            dimg.color = i % 2 == 0
                ? new Color(ColorPrimary.r, ColorPrimary.g, ColorPrimary.b, 0.12f)
                : new Color(ColorAccent.r, ColorAccent.g, ColorAccent.b, 0.10f);
            dimg.raycastTarget = false;
            var drt = dimg.rectTransform;
            drt.sizeDelta = new Vector2(700 + i * 80, 700 + i * 80);
            drt.anchorMin = new Vector2(0.5f, 0.5f);
            drt.anchorMax = new Vector2(0.5f, 0.5f);
            drt.pivot = new Vector2(0.5f, 0.5f);
            float x = (i % 2 == 0 ? -1f : 1f) * (200 + i * 50);
            float y = (i < 2 ? 1f : -1f) * (300 + i * 80);
            drt.anchoredPosition = new Vector2(x, y);
        }
    }

    private static TMP_Text CreateText(Transform parent, string name, string text, float size, FontStyles style, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        tmp.enableWordWrapping = true;
        return tmp;
    }

    private static void AddOutline(TMP_Text txt, Color outlineColor, float thickness)
    {
        txt.fontMaterial = new Material(txt.fontMaterial);
        txt.outlineColor = outlineColor;
        txt.outlineWidth = thickness * 0.05f;
    }

    private static Button CreateButton(Transform parent, string name, string label, Color bgColor, Color textColor, float textSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.sprite = CreateRoundedRectSprite();
        img.type = Image.Type.Sliced;
        img.color = bgColor;
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = Color.white;
        cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        cb.disabledColor = new Color(1f, 1f, 1f, 0.4f);
        btn.colors = cb;

        var txtGo = new GameObject("Label");
        txtGo.transform.SetParent(go.transform, false);
        var tmp = txtGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = textSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        var trt = tmp.rectTransform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        go.AddComponent<ButtonAnimator>();
        return btn;
    }

    private static Button CreateIconButton(Transform parent, string name, string icon, Color bgColor, Color iconColor)
    {
        var btn = CreateButton(parent, name, icon, bgColor, iconColor, 80);
        var label = btn.transform.Find("Label").GetComponent<TextMeshProUGUI>();
        label.fontStyle = FontStyles.Bold;
        return btn;
    }

    private static Button CreateLevelButton(Transform parent, string name, string label)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.sprite = CreateHexSprite();
        img.color = ColorPrimary;
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        var inner = new GameObject("Inner");
        inner.transform.SetParent(go.transform, false);
        var iimg = inner.AddComponent<Image>();
        iimg.sprite = CreateHexSprite();
        iimg.color = ColorBg;
        iimg.raycastTarget = false;
        var irt = iimg.rectTransform;
        irt.anchorMin = Vector2.zero;
        irt.anchorMax = Vector2.one;
        irt.offsetMin = new Vector2(14, 14);
        irt.offsetMax = new Vector2(-14, -14);

        var txtGo = new GameObject("Label");
        txtGo.transform.SetParent(go.transform, false);
        var tmp = txtGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 130;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = ColorPrimary;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        var trt = tmp.rectTransform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        go.AddComponent<ButtonAnimator>();
        return btn;
    }

    private static PopupBase CreateGenericPopup(Transform parent, string name, string title, string body)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var cg = go.AddComponent<CanvasGroup>();

        var backdropGo = new GameObject("Backdrop");
        backdropGo.transform.SetParent(go.transform, false);
        var backdrop = backdropGo.AddComponent<Image>();
        backdrop.color = new Color(0f, 0f, 0f, 0f);
        var brt = backdrop.rectTransform;
        brt.anchorMin = Vector2.zero;
        brt.anchorMax = Vector2.one;
        brt.offsetMin = Vector2.zero;
        brt.offsetMax = Vector2.zero;
        var bbtn = backdropGo.AddComponent<Button>();
        bbtn.transition = Selectable.Transition.None;

        var contentGo = new GameObject("Content");
        contentGo.transform.SetParent(go.transform, false);
        var cimg = contentGo.AddComponent<Image>();
        cimg.sprite = CreateRoundedRectSprite();
        cimg.type = Image.Type.Sliced;
        cimg.color = ColorPanel;
        var crt = cimg.rectTransform;
        crt.anchorMin = new Vector2(0.5f, 0.5f);
        crt.anchorMax = new Vector2(0.5f, 0.5f);
        crt.pivot = new Vector2(0.5f, 0.5f);
        crt.sizeDelta = new Vector2(900, 1100);
        crt.anchoredPosition = Vector2.zero;

        var titleText = CreateText(contentGo.transform, "Title", title, 90, FontStyles.Bold, ColorPrimary);
        var trt = titleText.rectTransform;
        trt.anchorMin = new Vector2(0.5f, 1f);
        trt.anchorMax = new Vector2(0.5f, 1f);
        trt.pivot = new Vector2(0.5f, 1f);
        trt.anchoredPosition = new Vector2(0, -80);
        trt.sizeDelta = new Vector2(800, 130);
        AddOutline(titleText, ColorAccent, 3);

        var bodyText = CreateText(contentGo.transform, "Body", body, 50, FontStyles.Normal, ColorText);
        var brt2 = bodyText.rectTransform;
        brt2.anchorMin = new Vector2(0.5f, 0.5f);
        brt2.anchorMax = new Vector2(0.5f, 0.5f);
        brt2.pivot = new Vector2(0.5f, 0.5f);
        brt2.anchoredPosition = Vector2.zero;
        brt2.sizeDelta = new Vector2(800, 400);

        var closeBtn = CreateButton(contentGo.transform, "CloseButton", "CLOSE", ColorAccent, ColorBg, 60);
        var ctrt = closeBtn.GetComponent<RectTransform>();
        ctrt.anchorMin = new Vector2(0.5f, 0f);
        ctrt.anchorMax = new Vector2(0.5f, 0f);
        ctrt.pivot = new Vector2(0.5f, 0f);
        ctrt.anchoredPosition = new Vector2(0, 80);
        ctrt.sizeDelta = new Vector2(500, 160);

        var popup = go.AddComponent<PopupBase>();
        SerializedObject so = new SerializedObject(popup);
        so.FindProperty("content").objectReferenceValue = crt;
        so.FindProperty("backdrop").objectReferenceValue = backdrop;
        so.FindProperty("canvasGroup").objectReferenceValue = cg;
        so.ApplyModifiedProperties();

        closeBtn.onClick.AddListener(popup.Hide);
        bbtn.onClick.AddListener(popup.Hide);

        go.SetActive(false);
        return popup;
    }

    private static Sprite cachedCircle;
    private static Sprite CreateCircleSprite()
    {
        if (cachedCircle != null) return cachedCircle;
        int size = 256;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        var cols = new Color[size * size];
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float r = size / 2f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Vector2.Distance(new Vector2(x, y), center);
            float t = Mathf.Clamp01(1f - d / r);
            cols[y * size + x] = new Color(1f, 1f, 1f, t * t);
        }
        tex.SetPixels(cols);
        tex.Apply();
        cachedCircle = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        return cachedCircle;
    }

    private static Sprite cachedRounded;
    private static Sprite CreateRoundedRectSprite()
    {
        if (cachedRounded != null) return cachedRounded;
        int size = 128;
        int radius = 32;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        var cols = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            bool inside = true;
            int cx = x, cy = y;
            int rcx = -1, rcy = -1;
            if (x < radius && y < radius)              { rcx = radius; rcy = radius; }
            else if (x >= size - radius && y < radius) { rcx = size - radius - 1; rcy = radius; }
            else if (x < radius && y >= size - radius) { rcx = radius; rcy = size - radius - 1; }
            else if (x >= size - radius && y >= size - radius) { rcx = size - radius - 1; rcy = size - radius - 1; }
            if (rcx >= 0)
            {
                float d = Vector2.Distance(new Vector2(cx, cy), new Vector2(rcx, rcy));
                inside = d <= radius;
            }
            cols[y * size + x] = inside ? Color.white : new Color(0, 0, 0, 0);
        }
        tex.SetPixels(cols);
        tex.Apply();
        cachedRounded = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        return cachedRounded;
    }

    private static Sprite cachedHex;
    private static Sprite CreateHexSprite()
    {
        if (cachedHex != null) return cachedHex;
        int size = 256;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        var cols = new Color[size * size];
        Vector2 c = new Vector2(size / 2f, size / 2f);
        float R = size / 2f - 4f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float px = x - c.x;
            float py = y - c.y;
            float qx = Mathf.Abs(px) / R;
            float qy = Mathf.Abs(py) / R;
            float h = Mathf.Max(qx + qy * 0.5f, qy);
            cols[y * size + x] = h <= 0.95f ? Color.white : new Color(0, 0, 0, 0);
        }
        tex.SetPixels(cols);
        tex.Apply();
        cachedHex = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        return cachedHex;
    }
}
