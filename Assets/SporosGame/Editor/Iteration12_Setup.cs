using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class Iteration12_Setup : EditorWindow
{
    private static readonly Color ColorBg       = new Color(0.039f, 0.055f, 0.153f, 1f);
    private static readonly Color ColorPrimary  = new Color(0.000f, 0.898f, 1.000f, 1f);
    private static readonly Color ColorAccent   = new Color(1.000f, 0.000f, 0.898f, 1f);
    private static readonly Color ColorGold     = new Color(1.000f, 0.700f, 0.150f, 1f);
    private static readonly Color ColorText     = Color.white;
    private static readonly Color ColorPanel    = new Color(0.078f, 0.102f, 0.231f, 1f);
    private static readonly Color ColorPanel2   = new Color(0.078f, 0.102f, 0.231f, 0.97f);
    private static readonly Color ColorOutline  = new Color(0.227f, 0.263f, 0.408f, 1f);
    private static readonly Color ColorCoin     = new Color(1.000f, 0.823f, 0.220f, 1f);
    private static readonly Color ColorSuccess  = new Color(0f, 1f, 0.533f, 1f);

    private const string SpritesFolder = "Assets/SporosGame/GeneratedSprites";
    private const string PrefabsFolder = "Assets/SporosGame/Prefabs";
    private const string MainMenuScene = "Assets/SporosGame/Scenes/MainMenu.unity";
    private const string GameScene = "Assets/SporosGame/Scenes/Game.unity";

    [MenuItem("Tools/SporosGame/Iteration 12/Achievements Setup (Iteration 12)")]
    public static void Setup()
    {
        var rounded = AssetDatabase.LoadAssetAtPath<Sprite>(SpritesFolder + "/rounded.png");
        var circle = AssetDatabase.LoadAssetAtPath<Sprite>(SpritesFolder + "/circle.png");
        var coinSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritesFolder + "/coin.png");
        var checkSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritesFolder + "/check.png");
        if (rounded == null || circle == null || coinSprite == null)
        {
            EditorUtility.DisplayDialog("SporosGame", "Sprites missing. Run earlier iterations first.", "OK");
            return;
        }

        var trophySprite = GetOrCreateTrophySprite();
        var rowPrefab = CreateAchievementRowPrefab(rounded, trophySprite, coinSprite, checkSprite);
        var unlockedPopupMainMenu = RebuildAchievementsInMainMenu(rounded, trophySprite, coinSprite, rowPrefab);
        var unlockedPopupGame = AddUnlockedPopupToGameScene(rounded, trophySprite, coinSprite);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("SporosGame",
            "Iteration 12 complete.\n\n12 achievements active.\nButton added to MainMenu.\nUnlock popup will slide in from bottom when triggered.",
            "OK");
    }

    [MenuItem("Tools/SporosGame/Iteration 12/Reset All Achievements")]
    public static void ResetAch()
    {
        AchievementsManager.ResetForTesting();
        EditorUtility.DisplayDialog("SporosGame", "All achievements reset.", "OK");
    }

    private static Sprite GetOrCreateTrophySprite()
    {
        string path = SpritesFolder + "/trophy.png";
        if (File.Exists(path)) File.Delete(path);
        if (File.Exists(path + ".meta")) File.Delete(path + ".meta");

        int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var cols = new Color[size * size];
        for (int i = 0; i < cols.Length; i++) cols[i] = new Color(0, 0, 0, 0);

        int cupTop = 28;
        int cupBottom = 75;
        int cupLeft = 38;
        int cupRight = 90;

        for (int y = cupTop; y <= cupBottom; y++)
        for (int x = cupLeft; x <= cupRight; x++)
        {
            float tx = (x - (cupLeft + cupRight) * 0.5f) / ((cupRight - cupLeft) * 0.5f);
            float ty = (y - cupTop) / (float)(cupBottom - cupTop);
            float widthAtY = Mathf.Lerp(1f, 0.7f, ty);
            if (Mathf.Abs(tx) <= widthAtY) cols[y * size + x] = Color.white;
        }

        for (int y = 76; y <= 88; y++)
        for (int x = 56; x <= 72; x++) cols[y * size + x] = Color.white;

        for (int y = 88; y <= 100; y++)
        for (int x = 44; x <= 84; x++) cols[y * size + x] = Color.white;

        int handleY1 = 35, handleY2 = 60;
        for (int y = handleY1; y <= handleY2; y++)
        {
            int x1 = 32, x2 = 96;
            for (int t = -3; t <= 3; t++) { if (x1 + t >= 0) cols[y * size + (x1 + t)] = Color.white; if (x2 + t < size) cols[y * size + (x2 + t)] = Color.white; }
        }
        for (int x = 28; x <= 38; x++) { cols[handleY1 * size + x] = Color.white; cols[(handleY1+1) * size + x] = Color.white; }
        for (int x = 90; x <= 100; x++) { cols[handleY1 * size + x] = Color.white; cols[(handleY1+1) * size + x] = Color.white; }
        for (int x = 28; x <= 38; x++) { cols[handleY2 * size + x] = Color.white; cols[(handleY2-1) * size + x] = Color.white; }
        for (int x = 90; x <= 100; x++) { cols[handleY2 * size + x] = Color.white; cols[(handleY2-1) * size + x] = Color.white; }

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

    private static GameObject CreateAchievementRowPrefab(Sprite rounded, Sprite trophy, Sprite coinSprite, Sprite checkSprite)
    {
        var root = new GameObject("AchievementRow");
        var rt = root.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(900, 160);

        var bgImg = root.AddComponent<Image>();
        bgImg.sprite = rounded;
        bgImg.type = Image.Type.Sliced;
        bgImg.color = ColorPanel;
        bgImg.raycastTarget = false;

        var le = root.AddComponent<LayoutElement>();
        le.minHeight = 160; le.preferredHeight = 160;

        var iconGo = new GameObject("Icon");
        iconGo.transform.SetParent(root.transform, false);
        var iRt = iconGo.AddComponent<RectTransform>();
        iRt.anchorMin = new Vector2(0f, 0.5f);
        iRt.anchorMax = new Vector2(0f, 0.5f);
        iRt.pivot = new Vector2(0f, 0.5f);
        iRt.anchoredPosition = new Vector2(20, 0);
        iRt.sizeDelta = new Vector2(110, 110);
        var iImg = iconGo.AddComponent<Image>();
        iImg.sprite = trophy;
        iImg.color = ColorPrimary;
        iImg.raycastTarget = false;

        var titleGo = new GameObject("Title");
        titleGo.transform.SetParent(root.transform, false);
        var tRt = titleGo.AddComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0f, 1f);
        tRt.anchorMax = new Vector2(1f, 1f);
        tRt.pivot = new Vector2(0f, 1f);
        tRt.offsetMin = new Vector2(150, -55);
        tRt.offsetMax = new Vector2(-200, -10);
        var tTmp = titleGo.AddComponent<TextMeshProUGUI>();
        tTmp.text = "Title";
        tTmp.fontSize = 38;
        tTmp.fontStyle = FontStyles.Bold;
        tTmp.color = ColorPrimary;
        tTmp.alignment = TextAlignmentOptions.MidlineLeft;
        tTmp.raycastTarget = false;

        var descGo = new GameObject("Description");
        descGo.transform.SetParent(root.transform, false);
        var dRt = descGo.AddComponent<RectTransform>();
        dRt.anchorMin = new Vector2(0f, 0f);
        dRt.anchorMax = new Vector2(1f, 1f);
        dRt.offsetMin = new Vector2(150, 50);
        dRt.offsetMax = new Vector2(-200, -55);
        var dTmp = descGo.AddComponent<TextMeshProUGUI>();
        dTmp.text = "Description";
        dTmp.fontSize = 26;
        dTmp.color = Color.white;
        dTmp.alignment = TextAlignmentOptions.TopLeft;
        dTmp.raycastTarget = false;

        var progressGo = new GameObject("ProgressBar");
        progressGo.transform.SetParent(root.transform, false);
        var prRt = progressGo.AddComponent<RectTransform>();
        prRt.anchorMin = new Vector2(0f, 0f);
        prRt.anchorMax = new Vector2(1f, 0f);
        prRt.pivot = new Vector2(0f, 0f);
        prRt.offsetMin = new Vector2(150, 10);
        prRt.offsetMax = new Vector2(-280, 30);
        var slider = progressGo.AddComponent<Slider>();
        slider.minValue = 0; slider.maxValue = 1; slider.value = 0;

        var sliderBg = new GameObject("BG");
        sliderBg.transform.SetParent(progressGo.transform, false);
        var sbRt = sliderBg.AddComponent<RectTransform>();
        sbRt.anchorMin = Vector2.zero; sbRt.anchorMax = Vector2.one;
        sbRt.offsetMin = Vector2.zero; sbRt.offsetMax = Vector2.zero;
        var sbImg = sliderBg.AddComponent<Image>();
        sbImg.sprite = rounded;
        sbImg.type = Image.Type.Sliced;
        sbImg.color = ColorOutline;
        sbImg.raycastTarget = false;

        var fillArea = new GameObject("FillArea");
        fillArea.transform.SetParent(progressGo.transform, false);
        var faRt = fillArea.AddComponent<RectTransform>();
        faRt.anchorMin = Vector2.zero; faRt.anchorMax = Vector2.one;
        faRt.offsetMin = Vector2.zero; faRt.offsetMax = Vector2.zero;
        var fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(fillArea.transform, false);
        var fRt = fillGo.AddComponent<RectTransform>();
        fRt.anchorMin = Vector2.zero; fRt.anchorMax = Vector2.one;
        fRt.offsetMin = Vector2.zero; fRt.offsetMax = Vector2.zero;
        var fImg = fillGo.AddComponent<Image>();
        fImg.sprite = rounded;
        fImg.type = Image.Type.Sliced;
        fImg.color = ColorPrimary;
        fImg.raycastTarget = false;
        slider.fillRect = fRt;
        slider.targetGraphic = fImg;
        slider.handleRect = null;

        var progLabelGo = new GameObject("ProgressLabel");
        progLabelGo.transform.SetParent(root.transform, false);
        var plRt = progLabelGo.AddComponent<RectTransform>();
        plRt.anchorMin = new Vector2(0f, 0f);
        plRt.anchorMax = new Vector2(1f, 0f);
        plRt.offsetMin = new Vector2(150, 35);
        plRt.offsetMax = new Vector2(-280, 60);
        var plTmp = progLabelGo.AddComponent<TextMeshProUGUI>();
        plTmp.text = "0 / 10";
        plTmp.fontSize = 22;
        plTmp.color = ColorPrimary;
        plTmp.alignment = TextAlignmentOptions.MidlineLeft;
        plTmp.raycastTarget = false;

        var rewardGo = new GameObject("Reward");
        rewardGo.transform.SetParent(root.transform, false);
        var reRt = rewardGo.AddComponent<RectTransform>();
        reRt.anchorMin = new Vector2(1f, 0.5f);
        reRt.anchorMax = new Vector2(1f, 0.5f);
        reRt.pivot = new Vector2(1f, 0.5f);
        reRt.anchoredPosition = new Vector2(-25, 0);
        reRt.sizeDelta = new Vector2(230, 110);
        var reBgImg = rewardGo.AddComponent<Image>();
        reBgImg.sprite = rounded;
        reBgImg.type = Image.Type.Sliced;
        reBgImg.color = new Color(0.039f, 0.055f, 0.153f, 1f);
        reBgImg.raycastTarget = false;

        var reCoinGo = new GameObject("Coin");
        reCoinGo.transform.SetParent(rewardGo.transform, false);
        var rcRt = reCoinGo.AddComponent<RectTransform>();
        rcRt.anchorMin = new Vector2(0f, 0.5f);
        rcRt.anchorMax = new Vector2(0f, 0.5f);
        rcRt.pivot = new Vector2(0f, 0.5f);
        rcRt.anchoredPosition = new Vector2(15, 0);
        rcRt.sizeDelta = new Vector2(60, 60);
        var rcImg = reCoinGo.AddComponent<Image>();
        rcImg.sprite = coinSprite;
        rcImg.color = ColorCoin;
        rcImg.raycastTarget = false;

        var reLabelGo = new GameObject("Label");
        reLabelGo.transform.SetParent(rewardGo.transform, false);
        var rlRt = reLabelGo.AddComponent<RectTransform>();
        rlRt.anchorMin = new Vector2(0f, 0f);
        rlRt.anchorMax = new Vector2(1f, 1f);
        rlRt.offsetMin = new Vector2(80, 0);
        rlRt.offsetMax = new Vector2(-15, 0);
        var rlTmp = reLabelGo.AddComponent<TextMeshProUGUI>();
        rlTmp.text = "+50";
        rlTmp.fontSize = 42;
        rlTmp.fontStyle = FontStyles.Bold;
        rlTmp.color = ColorCoin;
        rlTmp.alignment = TextAlignmentOptions.MidlineLeft;
        rlTmp.raycastTarget = false;

        var unlockedBadgeGo = new GameObject("UnlockedBadge");
        unlockedBadgeGo.transform.SetParent(root.transform, false);
        var ubRt = unlockedBadgeGo.AddComponent<RectTransform>();
        ubRt.anchorMin = new Vector2(0f, 0.5f);
        ubRt.anchorMax = new Vector2(0f, 0.5f);
        ubRt.pivot = new Vector2(0.5f, 0.5f);
        ubRt.anchoredPosition = new Vector2(110, -35);
        ubRt.sizeDelta = new Vector2(50, 50);
        var ubBg = unlockedBadgeGo.AddComponent<Image>();
        ubBg.sprite = rounded;
        ubBg.type = Image.Type.Sliced;
        ubBg.color = ColorSuccess;
        ubBg.raycastTarget = false;
        if (checkSprite != null)
        {
            var ckGo = new GameObject("Check");
            ckGo.transform.SetParent(unlockedBadgeGo.transform, false);
            var ckRt = ckGo.AddComponent<RectTransform>();
            ckRt.anchorMin = Vector2.zero; ckRt.anchorMax = Vector2.one;
            ckRt.offsetMin = new Vector2(8, 8); ckRt.offsetMax = new Vector2(-8, -8);
            var ckImg = ckGo.AddComponent<Image>();
            ckImg.sprite = checkSprite;
            ckImg.color = ColorBg;
            ckImg.raycastTarget = false;
        }
        unlockedBadgeGo.SetActive(false);

        var row = root.AddComponent<AchievementRow>();
        SerializedObject so = new SerializedObject(row);
        so.FindProperty("bgImage").objectReferenceValue = bgImg;
        so.FindProperty("iconImage").objectReferenceValue = iImg;
        so.FindProperty("titleLabel").objectReferenceValue = tTmp;
        so.FindProperty("descLabel").objectReferenceValue = dTmp;
        so.FindProperty("progressBar").objectReferenceValue = slider;
        so.FindProperty("progressLabel").objectReferenceValue = plTmp;
        so.FindProperty("unlockedBadge").objectReferenceValue = unlockedBadgeGo;
        so.FindProperty("rewardLabel").objectReferenceValue = rlTmp;
        so.ApplyModifiedProperties();

        string path = PrefabsFolder + "/AchievementRow.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static AchievementUnlockedPopup RebuildAchievementsInMainMenu(Sprite rounded, Sprite trophy, Sprite coinSprite, GameObject rowPrefab)
    {
        var scene = EditorSceneManager.OpenScene(MainMenuScene, OpenSceneMode.Single);

        Canvas canvas = null;
        var allCanvases = Object.FindObjectsOfType<Canvas>();
        for (int i = 0; i < allCanvases.Length; i++)
            if (allCanvases[i].name == "MainMenuCanvas") { canvas = allCanvases[i]; break; }
        if (canvas == null) return null;

        var oldUnlocked = canvas.transform.Find("AchievementUnlockedPopup");
        if (oldUnlocked != null) Object.DestroyImmediate(oldUnlocked.gameObject);
        var oldList = canvas.transform.Find("AchievementsPopup");
        if (oldList != null) Object.DestroyImmediate(oldList.gameObject);

        var unlockedPopup = CreateUnlockedPopup(canvas.transform, rounded, trophy, coinSprite);
        var achPopup = CreateAchievementsPopup(canvas.transform, rounded, rowPrefab);

        Transform safeArea = canvas.transform.Find("SafeArea");
        if (safeArea == null) safeArea = canvas.transform;

        Button achBtn = AddAchievementsButtonToMainMenu(safeArea, rounded, trophy);

        var mm = Object.FindObjectOfType<MainMenuController>();
        if (mm != null)
        {
            SerializedObject so = new SerializedObject(mm);
            var p1 = so.FindProperty("achievementsButton"); if (p1 != null) p1.objectReferenceValue = achBtn;
            var p2 = so.FindProperty("achievementsPopup"); if (p2 != null) p2.objectReferenceValue = achPopup;
            var p3 = so.FindProperty("achievementUnlockedPopup"); if (p3 != null) p3.objectReferenceValue = unlockedPopup;
            so.ApplyModifiedProperties();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return unlockedPopup;
    }

    private static AchievementUnlockedPopup AddUnlockedPopupToGameScene(Sprite rounded, Sprite trophy, Sprite coinSprite)
    {
        var scene = EditorSceneManager.OpenScene(GameScene, OpenSceneMode.Single);

        Canvas canvas = null;
        var allCanvases = Object.FindObjectsOfType<Canvas>();
        for (int i = 0; i < allCanvases.Length; i++)
        {
            if (allCanvases[i].renderMode == RenderMode.ScreenSpaceOverlay
                && allCanvases[i].name != "TutorialCanvas"
                && allCanvases[i].name != "BackgroundCanvas")
            {
                canvas = allCanvases[i];
                break;
            }
        }
        if (canvas == null) return null;

        var existing = canvas.transform.Find("AchievementUnlockedPopup");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        var unlockedPopup = CreateUnlockedPopup(canvas.transform, rounded, trophy, coinSprite);

        var ctrl = Object.FindObjectOfType<GameController>();
        if (ctrl != null)
        {
            SerializedObject so = new SerializedObject(ctrl);
            var p = so.FindProperty("achievementUnlockedPopup");
            if (p != null) p.objectReferenceValue = unlockedPopup;
            so.ApplyModifiedProperties();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return unlockedPopup;
    }

    private static AchievementUnlockedPopup CreateUnlockedPopup(Transform parent, Sprite rounded, Sprite trophy, Sprite coinSprite)
    {
        var go = new GameObject("AchievementUnlockedPopup");
        go.transform.SetParent(parent, false);
        var grt = go.AddComponent<RectTransform>();
        grt.anchorMin = Vector2.zero;
        grt.anchorMax = Vector2.one;
        grt.offsetMin = Vector2.zero;
        grt.offsetMax = Vector2.zero;

        var rootGo = new GameObject("Root");
        rootGo.transform.SetParent(go.transform, false);
        var rt = rootGo.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(880, 200);
        rt.anchoredPosition = new Vector2(0, -300);
        var bgImg = rootGo.AddComponent<Image>();
        bgImg.sprite = rounded;
        bgImg.type = Image.Type.Sliced;
        bgImg.color = new Color(0.039f, 0.055f, 0.153f, 0.97f);
        bgImg.raycastTarget = false;

        var iconGo = new GameObject("TrophyIcon");
        iconGo.transform.SetParent(rootGo.transform, false);
        var iRt = iconGo.AddComponent<RectTransform>();
        iRt.anchorMin = new Vector2(0f, 0.5f);
        iRt.anchorMax = new Vector2(0f, 0.5f);
        iRt.pivot = new Vector2(0f, 0.5f);
        iRt.anchoredPosition = new Vector2(25, 0);
        iRt.sizeDelta = new Vector2(140, 140);
        var iImg = iconGo.AddComponent<Image>();
        iImg.sprite = trophy;
        iImg.color = ColorPrimary;
        iImg.raycastTarget = false;

        var titleGo = new GameObject("Title");
        titleGo.transform.SetParent(rootGo.transform, false);
        var tRt = titleGo.AddComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0f, 1f);
        tRt.anchorMax = new Vector2(1f, 1f);
        tRt.offsetMin = new Vector2(180, -90);
        tRt.offsetMax = new Vector2(-200, -10);
        var tTmp = titleGo.AddComponent<TextMeshProUGUI>();
        tTmp.text = "Achievement";
        tTmp.fontSize = 46;
        tTmp.fontStyle = FontStyles.Bold;
        tTmp.color = ColorPrimary;
        tTmp.alignment = TextAlignmentOptions.MidlineLeft;
        tTmp.raycastTarget = false;

        var descGo = new GameObject("Description");
        descGo.transform.SetParent(rootGo.transform, false);
        var dRt = descGo.AddComponent<RectTransform>();
        dRt.anchorMin = new Vector2(0f, 0f);
        dRt.anchorMax = new Vector2(1f, 0f);
        dRt.offsetMin = new Vector2(180, 20);
        dRt.offsetMax = new Vector2(-200, 90);
        var dTmp = descGo.AddComponent<TextMeshProUGUI>();
        dTmp.text = "Description";
        dTmp.fontSize = 28;
        dTmp.color = new Color(1f, 1f, 1f, 0.85f);
        dTmp.alignment = TextAlignmentOptions.MidlineLeft;
        dTmp.raycastTarget = false;

        var rewardBadgeGo = new GameObject("RewardBadge");
        rewardBadgeGo.transform.SetParent(rootGo.transform, false);
        var rbRt = rewardBadgeGo.AddComponent<RectTransform>();
        rbRt.anchorMin = new Vector2(1f, 0.5f);
        rbRt.anchorMax = new Vector2(1f, 0.5f);
        rbRt.pivot = new Vector2(1f, 0.5f);
        rbRt.anchoredPosition = new Vector2(-25, 0);
        rbRt.sizeDelta = new Vector2(180, 100);
        var rbBg = rewardBadgeGo.AddComponent<Image>();
        rbBg.sprite = rounded;
        rbBg.type = Image.Type.Sliced;
        rbBg.color = ColorSuccess;
        rbBg.raycastTarget = false;

        var rbCoinGo = new GameObject("Coin");
        rbCoinGo.transform.SetParent(rewardBadgeGo.transform, false);
        var rcRt = rbCoinGo.AddComponent<RectTransform>();
        rcRt.anchorMin = new Vector2(0f, 0.5f);
        rcRt.anchorMax = new Vector2(0f, 0.5f);
        rcRt.pivot = new Vector2(0f, 0.5f);
        rcRt.anchoredPosition = new Vector2(15, 0);
        rcRt.sizeDelta = new Vector2(55, 55);
        var rcImg = rbCoinGo.AddComponent<Image>();
        rcImg.sprite = coinSprite;
        rcImg.color = ColorBg;
        rcImg.raycastTarget = false;

        var rewardLabelGo = new GameObject("Label");
        rewardLabelGo.transform.SetParent(rewardBadgeGo.transform, false);
        var rlRt = rewardLabelGo.AddComponent<RectTransform>();
        rlRt.anchorMin = new Vector2(0f, 0f);
        rlRt.anchorMax = new Vector2(1f, 1f);
        rlRt.offsetMin = new Vector2(70, 0);
        rlRt.offsetMax = new Vector2(-15, 0);
        var rlTmp = rewardLabelGo.AddComponent<TextMeshProUGUI>();
        rlTmp.text = "+50";
        rlTmp.fontSize = 50;
        rlTmp.fontStyle = FontStyles.Bold;
        rlTmp.color = ColorBg;
        rlTmp.alignment = TextAlignmentOptions.Center;
        rlTmp.raycastTarget = false;

        var popup = go.AddComponent<AchievementUnlockedPopup>();
        SerializedObject so = new SerializedObject(popup);
        so.FindProperty("root").objectReferenceValue = rt;
        so.FindProperty("bgImage").objectReferenceValue = bgImg;
        so.FindProperty("trophyIcon").objectReferenceValue = iImg;
        so.FindProperty("titleLabel").objectReferenceValue = tTmp;
        so.FindProperty("descriptionLabel").objectReferenceValue = dTmp;
        so.FindProperty("rewardLabel").objectReferenceValue = rlTmp;
        so.FindProperty("rewardBadge").objectReferenceValue = rewardBadgeGo;
        so.ApplyModifiedProperties();

        go.SetActive(false);
        return popup;
    }

    private static AchievementsPopup CreateAchievementsPopup(Transform parent, Sprite rounded, GameObject rowPrefab)
    {
        var go = new GameObject("AchievementsPopup");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var cg = go.AddComponent<CanvasGroup>();

        var (backdrop, content) = AddBackdropAndContent(go.transform, rounded, 1020, 1700);

        var title = CreateText(content, "Title", "ACHIEVEMENTS", 75, FontStyles.Bold, ColorPrimary);
        SetAnchored(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -80), new Vector2(900, 110));
        AddOutline(title, ColorAccent, 3);

        var scrollGo = new GameObject("ScrollView");
        scrollGo.transform.SetParent(content, false);
        var srt = scrollGo.AddComponent<RectTransform>();
        SetAnchored(srt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 30), new Vector2(960, 1280));
        var scrollImg = scrollGo.AddComponent<Image>();
        scrollImg.color = new Color(0, 0, 0, 0.2f);
        scrollImg.sprite = rounded;
        scrollImg.type = Image.Type.Sliced;
        var sr = scrollGo.AddComponent<ScrollRect>();
        sr.horizontal = false;
        sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Elastic;
        scrollGo.AddComponent<RectMask2D>();

        var viewportGo = new GameObject("Viewport");
        viewportGo.transform.SetParent(scrollGo.transform, false);
        var vRt = viewportGo.AddComponent<RectTransform>();
        vRt.anchorMin = Vector2.zero; vRt.anchorMax = Vector2.one;
        vRt.offsetMin = new Vector2(10, 10); vRt.offsetMax = new Vector2(-10, -10);
        viewportGo.AddComponent<Image>().color = new Color(0, 0, 0, 0);
        viewportGo.AddComponent<RectMask2D>();
        sr.viewport = vRt;

        var contentRtGo = new GameObject("Content");
        contentRtGo.transform.SetParent(viewportGo.transform, false);
        var crt = contentRtGo.AddComponent<RectTransform>();
        crt.anchorMin = new Vector2(0f, 1f);
        crt.anchorMax = new Vector2(1f, 1f);
        crt.pivot = new Vector2(0.5f, 1f);
        crt.anchoredPosition = Vector2.zero;
        crt.sizeDelta = new Vector2(0, 0);
        var vlg = contentRtGo.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 10;
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        var csf = contentRtGo.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sr.content = crt;

        var closeBtn = CreateButton(content, "CloseButton", "CLOSE", ColorAccent, ColorBg, 55, rounded);
        SetAnchored(closeBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 80), new Vector2(500, 150));

        var popup = go.AddComponent<AchievementsPopup>();
        SerializedObject so = new SerializedObject(popup);
        so.FindProperty("content").objectReferenceValue = content;
        so.FindProperty("backdrop").objectReferenceValue = backdrop;
        so.FindProperty("canvasGroup").objectReferenceValue = cg;
        so.FindProperty("closeButton").objectReferenceValue = closeBtn;
        so.FindProperty("listContent").objectReferenceValue = crt;
        so.FindProperty("rowPrefab").objectReferenceValue = rowPrefab;
        so.ApplyModifiedProperties();

        go.SetActive(false);
        return popup;
    }

    private static Button AddAchievementsButtonToMainMenu(Transform safeArea, Sprite rounded, Sprite trophy)
    {
        var existing = safeArea.Find("AchievementsButton");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        var go = new GameObject("AchievementsButton");
        go.transform.SetParent(safeArea, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(40, -40);
        rt.sizeDelta = new Vector2(140, 140);
        var img = go.AddComponent<Image>();
        img.sprite = rounded;
        img.type = Image.Type.Sliced;
        img.color = ColorPanel;
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        go.AddComponent<ButtonAnimator>();

        var iconGo = new GameObject("Icon");
        iconGo.transform.SetParent(go.transform, false);
        var iRt = iconGo.AddComponent<RectTransform>();
        iRt.anchorMin = Vector2.zero; iRt.anchorMax = Vector2.one;
        iRt.offsetMin = new Vector2(25, 25); iRt.offsetMax = new Vector2(-25, -25);
        var iImg = iconGo.AddComponent<Image>();
        iImg.sprite = trophy;
        iImg.color = ColorPrimary;
        iImg.raycastTarget = false;

        return btn;
    }

    private static (Image, RectTransform) AddBackdropAndContent(Transform parent, Sprite rounded, float w, float h)
    {
        var backdropGo = new GameObject("Backdrop");
        backdropGo.transform.SetParent(parent, false);
        var backdrop = backdropGo.AddComponent<Image>();
        backdrop.color = new Color(0f, 0f, 0f, 0f);
        var brt = backdrop.rectTransform;
        brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
        brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero;
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
        rt.anchorMin = amin; rt.anchorMax = amax;
        rt.pivot = new Vector2(0.5f, amin.y > 0.5f ? 1f : (amin.y < 0.5f ? 0f : 0.5f));
        rt.anchoredPosition = pos; rt.sizeDelta = size;
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
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;

        go.AddComponent<ButtonAnimator>();
        return btn;
    }
}
