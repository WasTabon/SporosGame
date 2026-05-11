using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class Iteration05_Setup : EditorWindow
{
    private static readonly Color ColorBg       = new Color(0.039f, 0.055f, 0.153f, 1f);
    private static readonly Color ColorPrimary  = new Color(0.000f, 0.898f, 1.000f, 1f);
    private static readonly Color ColorAccent   = new Color(1.000f, 0.000f, 0.898f, 1f);
    private static readonly Color ColorText     = Color.white;
    private static readonly Color ColorPanel    = new Color(0.078f, 0.102f, 0.231f, 1f);
    private static readonly Color ColorOutline  = new Color(0.227f, 0.263f, 0.408f, 1f);

    private const string GameFolder = "Assets/SporosGame";
    private const string DataFolder = "Assets/SporosGame/Data";
    private const string ResourcesFolder = "Assets/SporosGame/Resources";
    private const string PrefabsFolder = "Assets/SporosGame/Prefabs";
    private const string SpritesFolder = "Assets/SporosGame/GeneratedSprites";
    private const string LevelSelectScene = "Assets/SporosGame/Scenes/LevelSelect.unity";

    [MenuItem("Tools/SporosGame/Iteration 5/Build LevelDatabase + LevelSelect (Iteration 5)")]
    public static void Setup()
    {
        EnsureFolders();

        var db = BuildLevelDatabase();
        var hex = AssetDatabase.LoadAssetAtPath<Sprite>(SpritesFolder + "/hex.png");
        var rounded = AssetDatabase.LoadAssetAtPath<Sprite>(SpritesFolder + "/rounded.png");
        var circle = AssetDatabase.LoadAssetAtPath<Sprite>(SpritesFolder + "/circle.png");
        if (hex == null || rounded == null || circle == null)
        {
            EditorUtility.DisplayDialog("SporosGame", "Sprites missing — run Iteration 2 first.", "OK");
            return;
        }

        var lockSprite = GetOrCreateLockSprite();

        var btnPrefab = CreateLevelButtonPrefab(hex, lockSprite);
        RebuildLevelSelectScene(rounded, btnPrefab);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("SporosGame", "Iteration 5 complete.\nDatabase: " + db.Count + " levels.", "OK");
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder(DataFolder)) AssetDatabase.CreateFolder(GameFolder, "Data");
        if (!AssetDatabase.IsValidFolder(ResourcesFolder)) AssetDatabase.CreateFolder(GameFolder, "Resources");
    }

    private static LevelDatabase BuildLevelDatabase()
    {
        var dbPath = ResourcesFolder + "/LevelDatabase.asset";
        var db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(dbPath);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<LevelDatabase>();
            AssetDatabase.CreateAsset(db, dbPath);
        }

        var specs = new (int idx, int w, int h, int basicCount, bool extra)[]
        {
            (1, 3,3, 3, false),
            (2, 3,3, 3, false),
            (3, 3,3, 3, false),
            (4, 4,3, 4, false),
            (5, 4,3, 4, false),
            (6, 4,3, 4, false),
            (7, 4,4, 5, false),
            (8, 4,4, 5, false),
            (9, 4,4, 5, false),
            (10, 4,4, 5, false),
            (11, 5,4, 6, false),
            (12, 5,4, 6, false),
            (13, 5,4, 7, false),
            (14, 5,4, 7, false),
            (15, 5,5, 8, false),
            (16, 5,5, 8, false),
            (17, 5,5, 8, false),
            (18, 6,6, 10, false),
            (19, 7,7, 12, false),
            (20, 7,7, 14, false),
        };

        var list = new List<LevelData>();
        for (int i = 0; i < specs.Length; i++)
        {
            var s = specs[i];
            var path = DataFolder + "/Level_" + s.idx.ToString("00") + ".asset";
            var data = AssetDatabase.LoadAssetAtPath<LevelData>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<LevelData>();
                AssetDatabase.CreateAsset(data, path);
            }
            data.levelIndex = s.idx;
            data.width = s.w;
            data.height = s.h;
            data.isExtraPack = s.extra;
            data.rows = new CellTypeRow[s.h];
            for (int y = 0; y < s.h; y++)
            {
                data.rows[y] = new CellTypeRow { cells = new CellType[s.w] };
                for (int x = 0; x < s.w; x++) data.rows[y].cells[x] = CellType.Normal;
            }
            data.spores = new SporeStockEntry[]
            {
                new SporeStockEntry { type = SporeType.Basic, count = s.basicCount }
            };
            EditorUtility.SetDirty(data);
            list.Add(data);
        }

        db.levels = list.ToArray();
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        return db;
    }

    private static Sprite GetOrCreateLockSprite()
    {
        string path = SpritesFolder + "/lock.png";
        if (File.Exists(path)) File.Delete(path);
        if (File.Exists(path + ".meta")) File.Delete(path + ".meta");

        int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var cols = new Color[size * size];
        for (int i = 0; i < cols.Length; i++) cols[i] = new Color(0, 0, 0, 0);

        int bodyTop = (int)(size * 0.62f);
        int bodyBottom = (int)(size * 0.18f);
        int bodyLeft = (int)(size * 0.22f);
        int bodyRight = (int)(size * 0.78f);
        for (int y = bodyBottom; y < bodyTop; y++)
        for (int x = bodyLeft; x < bodyRight; x++)
            cols[y * size + x] = Color.white;

        int shackleCenterY = (int)(size * 0.78f);
        int shackleR = (int)(size * 0.18f);
        int shackleThickness = 8;
        Vector2 c = new Vector2(size / 2f, shackleCenterY);
        for (int y = (int)(size * 0.55f); y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Vector2.Distance(new Vector2(x, y), c);
            if (d <= shackleR && d >= shackleR - shackleThickness && y >= shackleCenterY - 4)
                cols[y * size + x] = Color.white;
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
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static GameObject CreateLevelButtonPrefab(Sprite hex, Sprite lockSprite)
    {
        var root = new GameObject("LevelButton");
        var rt = root.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(220, 220);

        var bgImg = root.AddComponent<Image>();
        bgImg.sprite = hex;
        bgImg.color = ColorPrimary;
        var btn = root.AddComponent<Button>();
        btn.targetGraphic = bgImg;
        root.AddComponent<ButtonAnimator>();

        var innerGo = new GameObject("Inner");
        innerGo.transform.SetParent(root.transform, false);
        var innerRt = innerGo.AddComponent<RectTransform>();
        innerRt.anchorMin = Vector2.zero;
        innerRt.anchorMax = Vector2.one;
        innerRt.offsetMin = new Vector2(14, 14);
        innerRt.offsetMax = new Vector2(-14, -14);
        var innerImg = innerGo.AddComponent<Image>();
        innerImg.sprite = hex;
        innerImg.color = ColorBg;
        innerImg.raycastTarget = false;

        var labelGo = new GameObject("Label");
        labelGo.transform.SetParent(root.transform, false);
        var labelRt = labelGo.AddComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = new Vector2(0, 30);
        labelRt.offsetMax = Vector2.zero;
        var label = labelGo.AddComponent<TextMeshProUGUI>();
        label.text = "1";
        label.fontSize = 90;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = ColorPrimary;
        label.raycastTarget = false;

        var starsGo = new GameObject("Stars");
        starsGo.transform.SetParent(root.transform, false);
        var starsRt = starsGo.AddComponent<RectTransform>();
        starsRt.anchorMin = new Vector2(0.5f, 0f);
        starsRt.anchorMax = new Vector2(0.5f, 0f);
        starsRt.pivot = new Vector2(0.5f, 0f);
        starsRt.anchoredPosition = new Vector2(0, 18);
        starsRt.sizeDelta = new Vector2(140, 40);
        var hg = starsGo.AddComponent<HorizontalLayoutGroup>();
        hg.spacing = 8;
        hg.childAlignment = TextAnchor.MiddleCenter;
        hg.childForceExpandWidth = false;
        hg.childForceExpandHeight = false;

        var stars = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            var sGo = new GameObject("Star_" + i);
            sGo.transform.SetParent(starsGo.transform, false);
            var sRt = sGo.AddComponent<RectTransform>();
            sRt.sizeDelta = new Vector2(34, 34);
            var sImg = sGo.AddComponent<Image>();
            sImg.sprite = hex;
            sImg.color = ColorOutline;
            sImg.raycastTarget = false;
            stars[i] = sImg;
        }

        var lockGo = new GameObject("Lock");
        lockGo.transform.SetParent(root.transform, false);
        var lockRt = lockGo.AddComponent<RectTransform>();
        lockRt.anchorMin = new Vector2(0.5f, 0.5f);
        lockRt.anchorMax = new Vector2(0.5f, 0.5f);
        lockRt.pivot = new Vector2(0.5f, 0.5f);
        lockRt.sizeDelta = new Vector2(80, 80);
        lockRt.anchoredPosition = new Vector2(0, 5);
        var lockImg = lockGo.AddComponent<Image>();
        lockImg.sprite = lockSprite;
        lockImg.color = ColorOutline;
        lockImg.raycastTarget = false;
        lockGo.SetActive(false);

        var extraGo = new GameObject("ExtraBadge");
        extraGo.transform.SetParent(root.transform, false);
        var extraRt = extraGo.AddComponent<RectTransform>();
        extraRt.anchorMin = new Vector2(1f, 1f);
        extraRt.anchorMax = new Vector2(1f, 1f);
        extraRt.pivot = new Vector2(1f, 1f);
        extraRt.sizeDelta = new Vector2(48, 48);
        extraRt.anchoredPosition = new Vector2(-10, -10);
        var extraImg = extraGo.AddComponent<Image>();
        extraImg.sprite = hex;
        extraImg.color = ColorAccent;
        extraImg.raycastTarget = false;
        extraGo.SetActive(false);

        var lb = root.AddComponent<LevelButton>();
        SerializedObject so = new SerializedObject(lb);
        so.FindProperty("button").objectReferenceValue = btn;
        so.FindProperty("bgImage").objectReferenceValue = bgImg;
        so.FindProperty("innerImage").objectReferenceValue = innerImg;
        so.FindProperty("label").objectReferenceValue = label;
        so.FindProperty("lockIcon").objectReferenceValue = lockImg;
        so.FindProperty("extraBadge").objectReferenceValue = extraImg;
        var starsProp = so.FindProperty("stars");
        starsProp.arraySize = 3;
        for (int i = 0; i < 3; i++) starsProp.GetArrayElementAtIndex(i).objectReferenceValue = stars[i];
        so.ApplyModifiedProperties();

        string path = PrefabsFolder + "/LevelButton.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static void RebuildLevelSelectScene(Sprite rounded, GameObject btnPrefab)
    {
        var scene = EditorSceneManager.OpenScene(LevelSelectScene, OpenSceneMode.Single);
        var roots = scene.GetRootGameObjects();
        foreach (var r in roots) Object.DestroyImmediate(r);

        SetupCamera();
        SetupEventSystem();
        SetupBootstrap();

        var canvas = CreateCanvas("LevelSelectCanvas");
        var safeArea = CreateSafeArea(canvas.transform);

        var title = CreateText(safeArea, "TitleText", "LEVELS", 110, FontStyles.Bold, ColorPrimary);
        var trt = title.rectTransform;
        trt.anchorMin = new Vector2(0.5f, 1f);
        trt.anchorMax = new Vector2(0.5f, 1f);
        trt.pivot = new Vector2(0.5f, 1f);
        trt.anchoredPosition = new Vector2(0, -180);
        trt.sizeDelta = new Vector2(800, 150);
        AddOutline(title, ColorAccent, 3);

        var backBtn = CreateIconButton(safeArea, "BackButton", "‹", ColorPanel, ColorText, rounded);
        var brt = backBtn.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0, 1);
        brt.anchorMax = new Vector2(0, 1);
        brt.pivot = new Vector2(0, 1);
        brt.anchoredPosition = new Vector2(40, -40);
        brt.sizeDelta = new Vector2(140, 140);

        var scrollGo = new GameObject("Scroll");
        scrollGo.transform.SetParent(safeArea, false);
        var sRt = scrollGo.AddComponent<RectTransform>();
        sRt.anchorMin = new Vector2(0, 0);
        sRt.anchorMax = new Vector2(1, 1);
        sRt.pivot = new Vector2(0.5f, 0.5f);
        sRt.offsetMin = new Vector2(40, 80);
        sRt.offsetMax = new Vector2(-40, -360);
        var scroll = scrollGo.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Elastic;

        var viewportGo = new GameObject("Viewport");
        viewportGo.transform.SetParent(scrollGo.transform, false);
        var vRt = viewportGo.AddComponent<RectTransform>();
        vRt.anchorMin = Vector2.zero;
        vRt.anchorMax = Vector2.one;
        vRt.offsetMin = Vector2.zero;
        vRt.offsetMax = Vector2.zero;
        var vMask = viewportGo.AddComponent<RectMask2D>();
        var vImg = viewportGo.AddComponent<Image>();
        vImg.color = new Color(0, 0, 0, 0);
        scroll.viewport = vRt;

        var contentGo = new GameObject("Content");
        contentGo.transform.SetParent(viewportGo.transform, false);
        var cRt = contentGo.AddComponent<RectTransform>();
        cRt.anchorMin = new Vector2(0, 1);
        cRt.anchorMax = new Vector2(1, 1);
        cRt.pivot = new Vector2(0.5f, 1f);
        cRt.anchoredPosition = Vector2.zero;
        cRt.sizeDelta = new Vector2(0, 800);

        var grid = contentGo.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(220, 220);
        grid.spacing = new Vector2(30, 30);
        grid.padding = new RectOffset(20, 20, 20, 20);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.childAlignment = TextAnchor.UpperCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;

        var fitter = contentGo.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.content = cRt;

        var ctrlGo = new GameObject("LevelSelectController");
        ctrlGo.transform.SetParent(canvas.transform, false);
        var ctrl = ctrlGo.AddComponent<LevelSelectController>();
        SerializedObject so = new SerializedObject(ctrl);
        so.FindProperty("backButton").objectReferenceValue = backBtn;
        so.FindProperty("levelsContent").objectReferenceValue = cRt;
        so.FindProperty("levelButtonPrefab").objectReferenceValue = btnPrefab;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
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

    private static Button CreateIconButton(Transform parent, string name, string icon, Color bgColor, Color iconColor, Sprite rounded)
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
        tmp.text = icon;
        tmp.fontSize = 80;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = iconColor;
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
