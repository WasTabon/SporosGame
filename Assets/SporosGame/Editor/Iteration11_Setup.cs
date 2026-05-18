using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class Iteration11_Setup : EditorWindow
{
    private static readonly Color ColorBg       = new Color(0.039f, 0.055f, 0.153f, 1f);
    private static readonly Color ColorPrimary  = new Color(0.000f, 0.898f, 1.000f, 1f);
    private static readonly Color ColorAccent   = new Color(1.000f, 0.000f, 0.898f, 1f);
    private static readonly Color ColorSuccess  = new Color(0.000f, 1.000f, 0.533f, 1f);
    private static readonly Color ColorCoin     = new Color(1.000f, 0.823f, 0.220f, 1f);
    private static readonly Color ColorText     = Color.white;
    private static readonly Color ColorPanel    = new Color(0.078f, 0.102f, 0.231f, 1f);
    private static readonly Color ColorPanel2   = new Color(0.078f, 0.102f, 0.231f, 0.95f);

    private const string SpritesFolder = "Assets/SporosGame/GeneratedSprites";
    private const string PrefabsFolder = "Assets/SporosGame/Prefabs";
    private const string MainMenuScene = "Assets/SporosGame/Scenes/MainMenu.unity";

    [MenuItem("Tools/SporosGame/Iteration 11/Daily Reward Setup (Iteration 11)")]
    public static void Setup()
    {
        var rounded = AssetDatabase.LoadAssetAtPath<Sprite>(SpritesFolder + "/rounded.png");
        var circle = AssetDatabase.LoadAssetAtPath<Sprite>(SpritesFolder + "/circle.png");
        var coinSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritesFolder + "/coin.png");
        if (rounded == null || circle == null || coinSprite == null)
        {
            EditorUtility.DisplayDialog("SporosGame", "Sprites missing. Run earlier iterations first.", "OK");
            return;
        }

        var checkSprite = GetOrCreateCheckSprite();
        var coinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabsFolder + "/CoinIcon.prefab");

        var dayBoxPrefab = CreateDayBoxPrefab(rounded, coinSprite, checkSprite, circle);
        RebuildDailyRewardPopupInMainMenu(rounded, circle, coinSprite, dayBoxPrefab, coinPrefab);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("SporosGame", "Iteration 11 complete.\nDaily Reward popup will auto-show on MainMenu load.", "OK");
    }

    [MenuItem("Tools/SporosGame/Iteration 11/Reset Daily Reward (for testing)")]
    public static void ResetForTesting()
    {
        DailyRewardManager.ResetForTesting();
        EditorUtility.DisplayDialog("SporosGame", "Daily reward state reset.\nNext MainMenu load will show Day 1 popup.", "OK");
    }

    private static Sprite GetOrCreateCheckSprite()
    {
        string path = SpritesFolder + "/check.png";
        if (File.Exists(path)) File.Delete(path);
        if (File.Exists(path + ".meta")) File.Delete(path + ".meta");

        int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var cols = new Color[size * size];
        for (int i = 0; i < cols.Length; i++) cols[i] = new Color(0, 0, 0, 0);

        int thickness = 10;
        for (int t = 0; t < 50; t++)
        {
            int x = 30 + t;
            int y = 60 - t;
            DrawDot(cols, size, x, y, thickness);
        }
        for (int t = 0; t < 70; t++)
        {
            int x = 50 + t;
            int y = 10 + t;
            DrawDot(cols, size, x, y, thickness);
        }

        tex.SetPixels(cols);
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void DrawDot(Color[] cols, int size, int cx, int cy, int radius)
    {
        for (int dy = -radius; dy <= radius; dy++)
        for (int dx = -radius; dx <= radius; dx++)
        {
            int x = cx + dx, y = cy + dy;
            if (x < 0 || y < 0 || x >= size || y >= size) continue;
            if (dx * dx + dy * dy <= radius * radius)
                cols[y * size + x] = Color.white;
        }
    }

    private static GameObject CreateDayBoxPrefab(Sprite rounded, Sprite coinSprite, Sprite checkSprite, Sprite circle)
    {
        var root = new GameObject("DayBox");
        var rt = root.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(110, 150);

        var pulseGo = new GameObject("PulseRing");
        pulseGo.transform.SetParent(root.transform, false);
        var pRt = pulseGo.AddComponent<RectTransform>();
        pRt.anchorMin = Vector2.zero;
        pRt.anchorMax = Vector2.one;
        pRt.offsetMin = new Vector2(-12, -12);
        pRt.offsetMax = new Vector2(12, 12);
        var pImg = pulseGo.AddComponent<Image>();
        pImg.sprite = rounded;
        pImg.type = Image.Type.Sliced;
        pImg.color = new Color(0f, 0.898f, 1f, 0.5f);
        pImg.raycastTarget = false;
        pulseGo.SetActive(false);

        var bgImg = root.AddComponent<Image>();
        bgImg.sprite = rounded;
        bgImg.type = Image.Type.Sliced;
        bgImg.color = ColorPanel;
        bgImg.raycastTarget = false;

        var innerGo = new GameObject("Inner");
        innerGo.transform.SetParent(root.transform, false);
        var iRt = innerGo.AddComponent<RectTransform>();
        iRt.anchorMin = Vector2.zero;
        iRt.anchorMax = Vector2.one;
        iRt.offsetMin = new Vector2(6, 6);
        iRt.offsetMax = new Vector2(-6, -6);
        var innerImg = innerGo.AddComponent<Image>();
        innerImg.sprite = rounded;
        innerImg.type = Image.Type.Sliced;
        innerImg.color = ColorBg;
        innerImg.raycastTarget = false;

        var dayGo = new GameObject("Day");
        dayGo.transform.SetParent(root.transform, false);
        var dRt = dayGo.AddComponent<RectTransform>();
        dRt.anchorMin = new Vector2(0f, 1f);
        dRt.anchorMax = new Vector2(1f, 1f);
        dRt.pivot = new Vector2(0.5f, 1f);
        dRt.anchoredPosition = new Vector2(0, -10);
        dRt.sizeDelta = new Vector2(0, 30);
        var dTmp = dayGo.AddComponent<TextMeshProUGUI>();
        dTmp.text = "Day 1";
        dTmp.fontSize = 24;
        dTmp.fontStyle = FontStyles.Bold;
        dTmp.color = Color.white;
        dTmp.alignment = TextAlignmentOptions.Center;
        dTmp.raycastTarget = false;

        var coinGo = new GameObject("CoinIcon");
        coinGo.transform.SetParent(root.transform, false);
        var cRt = coinGo.AddComponent<RectTransform>();
        cRt.anchorMin = new Vector2(0.5f, 0.5f);
        cRt.anchorMax = new Vector2(0.5f, 0.5f);
        cRt.pivot = new Vector2(0.5f, 0.5f);
        cRt.sizeDelta = new Vector2(42, 42);
        cRt.anchoredPosition = new Vector2(0, 5);
        var cImg = coinGo.AddComponent<Image>();
        cImg.sprite = coinSprite;
        cImg.color = ColorCoin;
        cImg.raycastTarget = false;

        var rewardGo = new GameObject("Reward");
        rewardGo.transform.SetParent(root.transform, false);
        var rRt = rewardGo.AddComponent<RectTransform>();
        rRt.anchorMin = new Vector2(0f, 0f);
        rRt.anchorMax = new Vector2(1f, 0f);
        rRt.pivot = new Vector2(0.5f, 0f);
        rRt.anchoredPosition = new Vector2(0, 10);
        rRt.sizeDelta = new Vector2(0, 32);
        var rTmp = rewardGo.AddComponent<TextMeshProUGUI>();
        rTmp.text = "10";
        rTmp.fontSize = 26;
        rTmp.fontStyle = FontStyles.Bold;
        rTmp.color = ColorCoin;
        rTmp.alignment = TextAlignmentOptions.Center;
        rTmp.raycastTarget = false;

        var checkGo = new GameObject("Check");
        checkGo.transform.SetParent(root.transform, false);
        var chRt = checkGo.AddComponent<RectTransform>();
        chRt.anchorMin = new Vector2(0.5f, 0.5f);
        chRt.anchorMax = new Vector2(0.5f, 0.5f);
        chRt.pivot = new Vector2(0.5f, 0.5f);
        chRt.sizeDelta = new Vector2(70, 70);
        chRt.anchoredPosition = Vector2.zero;
        var chImg = checkGo.AddComponent<Image>();
        chImg.sprite = checkSprite;
        chImg.color = Color.white;
        chImg.raycastTarget = false;
        checkGo.SetActive(false);

        var box = root.AddComponent<DayBoxView>();
        SerializedObject so = new SerializedObject(box);
        so.FindProperty("bgImage").objectReferenceValue = bgImg;
        so.FindProperty("innerImage").objectReferenceValue = innerImg;
        so.FindProperty("dayLabel").objectReferenceValue = dTmp;
        so.FindProperty("rewardLabel").objectReferenceValue = rTmp;
        so.FindProperty("coinIcon").objectReferenceValue = cImg;
        so.FindProperty("checkMark").objectReferenceValue = chImg;
        so.FindProperty("pulseRing").objectReferenceValue = pImg;
        so.ApplyModifiedProperties();

        string path = PrefabsFolder + "/DayBox.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static void RebuildDailyRewardPopupInMainMenu(Sprite rounded, Sprite circle, Sprite coinSprite, GameObject dayBoxPrefab, GameObject coinPrefab)
    {
        var scene = EditorSceneManager.OpenScene(MainMenuScene, OpenSceneMode.Single);

        Canvas canvas = null;
        var allCanvases = Object.FindObjectsOfType<Canvas>();
        for (int i = 0; i < allCanvases.Length; i++)
            if (allCanvases[i].name == "MainMenuCanvas") { canvas = allCanvases[i]; break; }
        if (canvas == null) return;

        var existing = canvas.transform.Find("DailyRewardPopup");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        var popupGo = new GameObject("DailyRewardPopup");
        popupGo.transform.SetParent(canvas.transform, false);
        var prt = popupGo.AddComponent<RectTransform>();
        prt.anchorMin = Vector2.zero;
        prt.anchorMax = Vector2.one;
        prt.offsetMin = Vector2.zero;
        prt.offsetMax = Vector2.zero;
        var cg = popupGo.AddComponent<CanvasGroup>();

        var (backdrop, content) = AddBackdropAndContent(popupGo.transform, rounded, 980, 1100);

        var title = CreateText(content, "Title", "DAILY REWARD", 75, FontStyles.Bold, ColorPrimary);
        SetAnchored(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -80), new Vector2(900, 110));
        AddOutline(title, ColorAccent, 3);

        var subTitle = CreateText(content, "SubTitle", "come back every day", 36, FontStyles.Italic, new Color(1f, 1f, 1f, 0.6f));
        SetAnchored(subTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -200), new Vector2(800, 60));

        var boxesGo = new GameObject("DayBoxes");
        boxesGo.transform.SetParent(content, false);
        var bRt = boxesGo.AddComponent<RectTransform>();
        SetAnchored(bRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 80), new Vector2(880, 200));
        var hg = boxesGo.AddComponent<HorizontalLayoutGroup>();
        hg.spacing = 8;
        hg.childAlignment = TextAnchor.MiddleCenter;
        hg.childForceExpandWidth = false;
        hg.childForceExpandHeight = false;

        var claimBtnGo = new GameObject("ClaimButton");
        claimBtnGo.transform.SetParent(content, false);
        var btnImg = claimBtnGo.AddComponent<Image>();
        btnImg.sprite = rounded;
        btnImg.type = Image.Type.Sliced;
        btnImg.color = ColorSuccess;
        var claimRt = claimBtnGo.GetComponent<RectTransform>();
        SetAnchored(claimRt, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 110), new Vector2(620, 170));
        var claimBtn = claimBtnGo.AddComponent<Button>();
        claimBtn.targetGraphic = btnImg;
        claimBtnGo.AddComponent<ButtonAnimator>();

        var claimLabelGo = new GameObject("Label");
        claimLabelGo.transform.SetParent(claimBtnGo.transform, false);
        var clRt = claimLabelGo.AddComponent<RectTransform>();
        clRt.anchorMin = Vector2.zero;
        clRt.anchorMax = Vector2.one;
        clRt.offsetMin = Vector2.zero;
        clRt.offsetMax = Vector2.zero;
        var clTmp = claimLabelGo.AddComponent<TextMeshProUGUI>();
        clTmp.text = "CLAIM";
        clTmp.fontSize = 70;
        clTmp.fontStyle = FontStyles.Bold;
        clTmp.color = ColorBg;
        clTmp.alignment = TextAlignmentOptions.Center;
        clTmp.raycastTarget = false;

        var flySrcGo = new GameObject("FlySource");
        flySrcGo.transform.SetParent(content, false);
        var fsRt = flySrcGo.AddComponent<RectTransform>();
        fsRt.anchorMin = new Vector2(0.5f, 0.5f);
        fsRt.anchorMax = new Vector2(0.5f, 0.5f);
        fsRt.pivot = new Vector2(0.5f, 0.5f);
        fsRt.sizeDelta = new Vector2(10, 10);
        fsRt.anchoredPosition = new Vector2(0, 80);

        var flyFxGo = new GameObject("CoinFlyFx");
        flyFxGo.transform.SetParent(popupGo.transform, false);
        var flyFxRt = flyFxGo.AddComponent<RectTransform>();
        flyFxRt.anchorMin = Vector2.zero;
        flyFxRt.anchorMax = Vector2.one;
        flyFxRt.offsetMin = Vector2.zero;
        flyFxRt.offsetMax = Vector2.zero;
        var flyFx = flyFxGo.AddComponent<CoinFlyEffect>();
        if (coinPrefab != null) flyFx.Setup(coinPrefab);

        var popup = popupGo.AddComponent<DailyRewardPopup>();
        SerializedObject so = new SerializedObject(popup);
        so.FindProperty("content").objectReferenceValue = content;
        so.FindProperty("backdrop").objectReferenceValue = backdrop;
        so.FindProperty("canvasGroup").objectReferenceValue = cg;
        so.FindProperty("claimButton").objectReferenceValue = claimBtn;
        so.FindProperty("claimButtonLabel").objectReferenceValue = clTmp;
        so.FindProperty("dayBoxesContainer").objectReferenceValue = bRt;
        so.FindProperty("dayBoxPrefab").objectReferenceValue = dayBoxPrefab;
        so.FindProperty("coinFlySource").objectReferenceValue = fsRt;
        so.FindProperty("coinFlyEffect").objectReferenceValue = flyFx;
        so.FindProperty("titleText").objectReferenceValue = title;
        so.ApplyModifiedProperties();

        popupGo.SetActive(false);

        var mm = Object.FindObjectOfType<MainMenuController>();
        if (mm != null)
        {
            SerializedObject ms = new SerializedObject(mm);
            var prop = ms.FindProperty("dailyRewardPopup");
            if (prop != null) prop.objectReferenceValue = popup;
            ms.ApplyModifiedProperties();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
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
}
