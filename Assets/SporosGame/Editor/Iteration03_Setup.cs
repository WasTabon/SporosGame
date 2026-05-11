using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class Iteration03_Setup : EditorWindow
{
    private static readonly Color ColorBg       = new Color(0.039f, 0.055f, 0.153f, 1f);
    private static readonly Color ColorPrimary  = new Color(0.000f, 0.898f, 1.000f, 1f);
    private static readonly Color ColorAccent   = new Color(1.000f, 0.000f, 0.898f, 1f);
    private static readonly Color ColorSuccess  = new Color(0.000f, 1.000f, 0.533f, 1f);
    private static readonly Color ColorText     = Color.white;
    private static readonly Color ColorPanel    = new Color(0.078f, 0.102f, 0.231f, 1f);
    private static readonly Color ColorPanel2   = new Color(0.078f, 0.102f, 0.231f, 0.95f);
    private static readonly Color ColorOutline  = new Color(0.227f, 0.263f, 0.408f, 1f);

    private const string GameScene = "Assets/SporosGame/Scenes/Game.unity";
    private const string SpritesFolder = "Assets/SporosGame/GeneratedSprites";

    [MenuItem("Tools/SporosGame/Iteration 3/Update Game Scene (Iteration 3)")]
    public static void Setup()
    {
        var scene = EditorSceneManager.OpenScene(GameScene, OpenSceneMode.Single);

        var rounded = AssetDatabase.LoadAssetAtPath<Sprite>(SpritesFolder + "/rounded.png");
        var hex = AssetDatabase.LoadAssetAtPath<Sprite>(SpritesFolder + "/hex.png");
        if (rounded == null || hex == null)
        {
            EditorUtility.DisplayDialog("SporosGame", "Run Iteration 2 setup first (need hex/rounded sprites).", "OK");
            return;
        }

        var ctrl = Object.FindObjectOfType<GameController>();
        var hud = Object.FindObjectOfType<HUDController>();
        var canvas = FindUICanvas();
        if (ctrl == null || hud == null || canvas == null)
        {
            EditorUtility.DisplayDialog("SporosGame", "Game scene missing GameController/HUD/Canvas — re-run Iteration 2 setup.", "OK");
            return;
        }

        var safeArea = canvas.transform.Find("SafeArea") as RectTransform;
        if (safeArea == null)
        {
            EditorUtility.DisplayDialog("SporosGame", "SafeArea not found.", "OK");
            return;
        }

        AddHUDButtons(hud, safeArea, rounded);

        var winPopup = CreateWinPopup(canvas.transform, rounded, hex);
        var losePopup = CreateLosePopup(canvas.transform, rounded);
        var pausePopup = CreatePausePopup(canvas.transform, rounded);

        SerializedObject sc = new SerializedObject(ctrl);
        sc.FindProperty("winPopup").objectReferenceValue = winPopup;
        sc.FindProperty("losePopup").objectReferenceValue = losePopup;
        sc.FindProperty("pausePopup").objectReferenceValue = pausePopup;
        sc.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("SporosGame", "Iteration 3 setup complete.\nOpen MainMenu and Play.", "OK");
    }

    private static Canvas FindUICanvas()
    {
        var all = Object.FindObjectsOfType<Canvas>();
        for (int i = 0; i < all.Length; i++)
            if (all[i].name == "GameCanvas") return all[i];
        return null;
    }

    private static void AddHUDButtons(HUDController hud, RectTransform safeArea, Sprite rounded)
    {
        var hudRt = hud.GetComponent<RectTransform>();

        var pauseBtn = FindOrCreateChildButton(hudRt, "PauseButton", "II", rounded, ColorPanel, ColorText, 60);
        var pRt = pauseBtn.GetComponent<RectTransform>();
        pRt.anchorMin = new Vector2(1, 1);
        pRt.anchorMax = new Vector2(1, 1);
        pRt.pivot = new Vector2(1, 1);
        pRt.anchoredPosition = new Vector2(-40, -40);
        pRt.sizeDelta = new Vector2(140, 140);

        var timerRt = (hudRt.Find("TimerText") as RectTransform);
        if (timerRt != null)
        {
            timerRt.anchoredPosition = new Vector2(-200, -85);
        }

        var inventoryPanel = safeArea.Find("InventoryPanel") as RectTransform;
        var actionsParent = safeArea;
        if (inventoryPanel != null)
        {
            var existingActions = safeArea.Find("ActionButtons") as RectTransform;
            RectTransform actionsRt;
            if (existingActions != null) actionsRt = existingActions;
            else
            {
                var aGo = new GameObject("ActionButtons");
                aGo.transform.SetParent(safeArea, false);
                actionsRt = aGo.AddComponent<RectTransform>();
            }
            actionsRt.anchorMin = new Vector2(1, 0);
            actionsRt.anchorMax = new Vector2(1, 0);
            actionsRt.pivot = new Vector2(1, 0);
            actionsRt.anchoredPosition = new Vector2(-30, 340);
            actionsRt.sizeDelta = new Vector2(160, 320);

            var undoBtn = FindOrCreateChildButton(actionsRt, "UndoButton", "↶", rounded, ColorPanel, ColorAccent, 70);
            var urt = undoBtn.GetComponent<RectTransform>();
            urt.anchorMin = new Vector2(0.5f, 1f);
            urt.anchorMax = new Vector2(0.5f, 1f);
            urt.pivot = new Vector2(0.5f, 1f);
            urt.anchoredPosition = new Vector2(0, -10);
            urt.sizeDelta = new Vector2(140, 140);

            var resetBtn = FindOrCreateChildButton(actionsRt, "ResetButton", "↻", rounded, ColorPanel, ColorPrimary, 70);
            var rrt = resetBtn.GetComponent<RectTransform>();
            rrt.anchorMin = new Vector2(0.5f, 1f);
            rrt.anchorMax = new Vector2(0.5f, 1f);
            rrt.pivot = new Vector2(0.5f, 1f);
            rrt.anchoredPosition = new Vector2(0, -170);
            rrt.sizeDelta = new Vector2(140, 140);

            SerializedObject sh = new SerializedObject(hud);
            sh.FindProperty("pauseButton").objectReferenceValue = pauseBtn;
            sh.FindProperty("undoButton").objectReferenceValue = undoBtn;
            sh.FindProperty("resetButton").objectReferenceValue = resetBtn;
            sh.ApplyModifiedProperties();
        }
    }

    private static Button FindOrCreateChildButton(RectTransform parent, string name, string label, Sprite rounded, Color bg, Color fg, float textSize)
    {
        var existing = parent.Find(name);
        if (existing != null) Object.DestroyImmediate(existing.gameObject);
        return CreateButton(parent, name, label, bg, fg, textSize, rounded);
    }

    private static WinPopup CreateWinPopup(Transform parent, Sprite rounded, Sprite hex)
    {
        var existing = parent.Find("WinPopup");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        var go = new GameObject("WinPopup");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var cg = go.AddComponent<CanvasGroup>();

        var (backdrop, content) = AddBackdropAndContent(go.transform, rounded, 900, 1200);

        var title = CreateText(content, "Title", "LEVEL COMPLETE", 80, FontStyles.Bold, ColorSuccess);
        SetAnchored(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -90), new Vector2(820, 130));
        AddOutline(title, ColorPrimary, 3);

        var starsGo = new GameObject("Stars");
        starsGo.transform.SetParent(content, false);
        var starsRt = starsGo.AddComponent<RectTransform>();
        SetAnchored(starsRt, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -280), new Vector2(700, 220));
        var hg = starsGo.AddComponent<HorizontalLayoutGroup>();
        hg.spacing = 30;
        hg.childAlignment = TextAnchor.MiddleCenter;
        hg.childForceExpandWidth = false;
        hg.childForceExpandHeight = false;

        var starImgs = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            var sGo = new GameObject("Star_" + i);
            sGo.transform.SetParent(starsGo.transform, false);
            var img = sGo.AddComponent<Image>();
            img.sprite = hex;
            img.color = ColorPrimary;
            var srt = sGo.GetComponent<RectTransform>();
            srt.sizeDelta = new Vector2(180, 180);
            starImgs[i] = img;
        }

        var nextBtn = CreateButton(content, "NextButton", "NEXT", ColorPrimary, ColorBg, 65, rounded);
        SetAnchored(nextBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 380), new Vector2(620, 180));

        var retryBtn = CreateButton(content, "RetryButton", "RETRY", ColorPanel, ColorAccent, 55, rounded);
        SetAnchored(retryBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-160, 180), new Vector2(290, 160));

        var menuBtn = CreateButton(content, "MenuButton", "MENU", ColorPanel, ColorText, 55, rounded);
        SetAnchored(menuBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(160, 180), new Vector2(290, 160));

        var popup = go.AddComponent<WinPopup>();
        SerializedObject so = new SerializedObject(popup);
        so.FindProperty("content").objectReferenceValue = content;
        so.FindProperty("backdrop").objectReferenceValue = backdrop;
        so.FindProperty("canvasGroup").objectReferenceValue = cg;
        so.FindProperty("nextButton").objectReferenceValue = nextBtn;
        so.FindProperty("retryButton").objectReferenceValue = retryBtn;
        so.FindProperty("menuButton").objectReferenceValue = menuBtn;
        var starsProp = so.FindProperty("starIcons");
        starsProp.arraySize = 3;
        for (int i = 0; i < 3; i++) starsProp.GetArrayElementAtIndex(i).objectReferenceValue = starImgs[i];
        so.ApplyModifiedProperties();

        go.SetActive(false);
        return popup;
    }

    private static LosePopup CreateLosePopup(Transform parent, Sprite rounded)
    {
        var existing = parent.Find("LosePopup");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        var go = new GameObject("LosePopup");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var cg = go.AddComponent<CanvasGroup>();

        var (backdrop, content) = AddBackdropAndContent(go.transform, rounded, 900, 900);

        var title = CreateText(content, "Title", "OUT OF SPORES", 75, FontStyles.Bold, ColorAccent);
        SetAnchored(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -120), new Vector2(820, 130));
        AddOutline(title, ColorPrimary, 3);

        var subTxt = CreateText(content, "Sub", "the field is still not lit", 42, FontStyles.Italic, new Color(1f, 1f, 1f, 0.7f));
        SetAnchored(subTxt.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 60), new Vector2(780, 80));

        var retryBtn = CreateButton(content, "RetryButton", "RETRY", ColorAccent, ColorBg, 65, rounded);
        SetAnchored(retryBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 280), new Vector2(620, 180));

        var menuBtn = CreateButton(content, "MenuButton", "MENU", ColorPanel, ColorText, 55, rounded);
        SetAnchored(menuBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 100), new Vector2(620, 150));

        var popup = go.AddComponent<LosePopup>();
        SerializedObject so = new SerializedObject(popup);
        so.FindProperty("content").objectReferenceValue = content;
        so.FindProperty("backdrop").objectReferenceValue = backdrop;
        so.FindProperty("canvasGroup").objectReferenceValue = cg;
        so.FindProperty("retryButton").objectReferenceValue = retryBtn;
        so.FindProperty("menuButton").objectReferenceValue = menuBtn;
        so.ApplyModifiedProperties();

        go.SetActive(false);
        return popup;
    }

    private static PausePopup CreatePausePopup(Transform parent, Sprite rounded)
    {
        var existing = parent.Find("PausePopup");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        var go = new GameObject("PausePopup");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var cg = go.AddComponent<CanvasGroup>();

        var (backdrop, content) = AddBackdropAndContent(go.transform, rounded, 900, 1050);

        var title = CreateText(content, "Title", "PAUSED", 90, FontStyles.Bold, ColorPrimary);
        SetAnchored(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -100), new Vector2(820, 140));
        AddOutline(title, ColorAccent, 3);

        var resumeBtn = CreateButton(content, "ResumeButton", "RESUME", ColorPrimary, ColorBg, 65, rounded);
        SetAnchored(resumeBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 460), new Vector2(620, 170));

        var restartBtn = CreateButton(content, "RestartButton", "RESTART", ColorPanel, ColorAccent, 55, rounded);
        SetAnchored(restartBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 270), new Vector2(620, 150));

        var menuBtn = CreateButton(content, "MenuButton", "MENU", ColorPanel, ColorText, 55, rounded);
        SetAnchored(menuBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 100), new Vector2(620, 150));

        var popup = go.AddComponent<PausePopup>();
        SerializedObject so = new SerializedObject(popup);
        so.FindProperty("content").objectReferenceValue = content;
        so.FindProperty("backdrop").objectReferenceValue = backdrop;
        so.FindProperty("canvasGroup").objectReferenceValue = cg;
        so.FindProperty("resumeButton").objectReferenceValue = resumeBtn;
        so.FindProperty("restartButton").objectReferenceValue = restartBtn;
        so.FindProperty("menuButton").objectReferenceValue = menuBtn;
        so.ApplyModifiedProperties();

        go.SetActive(false);
        return popup;
    }

    private static (Image, RectTransform) AddBackdropAndContent(Transform parent, Sprite rounded, float w, float h)
    {
        var backdropGo = new GameObject("Backdrop");
        backdropGo.transform.SetParent(parent, false);
        var backdrop = backdropGo.AddComponent<Image>();
        backdrop.color = new Color(0f, 0f, 0f, 0f);
        var brt = backdrop.rectTransform;
        brt.anchorMin = Vector2.zero;
        brt.anchorMax = Vector2.one;
        brt.offsetMin = Vector2.zero;
        brt.offsetMax = Vector2.zero;
        backdropGo.AddComponent<Button>().transition = Selectable.Transition.None;

        var contentGo = new GameObject("Content");
        contentGo.transform.SetParent(parent, false);
        var cimg = contentGo.AddComponent<Image>();
        cimg.sprite = rounded;
        cimg.type = Image.Type.Sliced;
        cimg.color = ColorPanel2;
        var crt = cimg.rectTransform;
        crt.anchorMin = new Vector2(0.5f, 0.5f);
        crt.anchorMax = new Vector2(0.5f, 0.5f);
        crt.pivot = new Vector2(0.5f, 0.5f);
        crt.sizeDelta = new Vector2(w, h);
        crt.anchoredPosition = Vector2.zero;
        return (backdrop, crt);
    }

    private static void SetAnchored(RectTransform rt, Vector2 amin, Vector2 amax, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = amin;
        rt.anchorMax = amax;
        rt.pivot = new Vector2(0.5f, amin.y > 0.5f ? 1f : (amin.y < 0.5f ? 0f : 0.5f));
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
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
        return tmp;
    }

    private static void AddOutline(TMP_Text txt, Color outlineColor, float thickness)
    {
        txt.fontMaterial = new Material(txt.fontMaterial);
        txt.outlineColor = outlineColor;
        txt.outlineWidth = thickness * 0.05f;
    }

    private static Button CreateButton(Transform parent, string name, string label, Color bgColor, Color textColor, float textSize, Sprite rounded)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.sprite = rounded;
        img.type = Image.Type.Sliced;
        img.color = bgColor;
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

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
}
