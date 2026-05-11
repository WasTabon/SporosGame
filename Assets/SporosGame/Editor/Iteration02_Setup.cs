using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Iteration02_Setup : EditorWindow
{
    private static readonly Color ColorBg       = new Color(0.039f, 0.055f, 0.153f, 1f);
    private static readonly Color ColorPrimary  = new Color(0.000f, 0.898f, 1.000f, 1f);
    private static readonly Color ColorAccent   = new Color(1.000f, 0.000f, 0.898f, 1f);
    private static readonly Color ColorText     = Color.white;
    private static readonly Color ColorPanel    = new Color(0.078f, 0.102f, 0.231f, 1f);
    private static readonly Color ColorPanel2   = new Color(0.078f, 0.102f, 0.231f, 0.85f);
    private static readonly Color ColorOutline  = new Color(0.227f, 0.263f, 0.408f, 1f);

    private const string GameScene = "Assets/SporosGame/Scenes/Game.unity";
    private const string PrefabsFolder = "Assets/SporosGame/Prefabs";
    private const string SpritesFolder = "Assets/SporosGame/GeneratedSprites";

    [MenuItem("Tools/SporosGame/Iteration 2/Setup Game Scene (Iteration 2)")]
    public static void Setup()
    {
        EnsureFolders();
        var hexSprite = GetOrCreateHexSprite();
        var circleSprite = GetOrCreateCircleSprite();
        var squareSprite = GetOrCreateSquareSprite();
        var roundedSprite = GetOrCreateRoundedRectSprite();

        var cellPrefab = CreateCellPrefab(hexSprite);
        var raySegPrefab = CreateRaySegmentPrefab(squareSprite);
        var sporePrefab = CreateSporePrefab(circleSprite, raySegPrefab);
        var invItemPrefab = CreateInventoryItemPrefab(circleSprite, roundedSprite);

        var scene = EditorSceneManager.OpenScene(GameScene, OpenSceneMode.Single);
        ClearScene(scene);

        SetupCamera();
        SetupEventSystem();
        SetupBootstrap();

        var sporeParent = new GameObject("SporeRoot");

        var gridGo = new GameObject("Grid");
        var grid = gridGo.AddComponent<GridSystem>();
        SerializedObject sgrid = new SerializedObject(grid);
        sgrid.FindProperty("cellPrefab").objectReferenceValue = cellPrefab;
        sgrid.FindProperty("cellSize").floatValue = 1.5f;
        sgrid.FindProperty("rowOffsetFactor").floatValue = 0.5f;
        sgrid.ApplyModifiedProperties();

        CreateBackgroundCanvas(circleSprite);

        var uiCanvas = CreateUICanvas("GameCanvas");
        var safeArea = CreateSafeArea(uiCanvas.transform);

        var (hud, backBtn, levelTxt, timerTxt) = CreateHUD(safeArea, roundedSprite);
        var inventory = CreateInventoryPanel(safeArea, roundedSprite, invItemPrefab);

        var ctrlGo = new GameObject("GameController");
        var ctrl = ctrlGo.AddComponent<GameController>();
        SerializedObject sc = new SerializedObject(ctrl);
        sc.FindProperty("grid").objectReferenceValue = grid;
        sc.FindProperty("inventory").objectReferenceValue = inventory;
        sc.FindProperty("hud").objectReferenceValue = hud;
        sc.FindProperty("sporePrefab").objectReferenceValue = sporePrefab;
        sc.FindProperty("sporeParent").objectReferenceValue = sporeParent.transform;
        sc.FindProperty("gameCamera").objectReferenceValue = Camera.main;
        sc.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("SporosGame", "Iteration 2 (fixed) setup complete.\nOpen MainMenu and Play.", "OK");
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/SporosGame"))
            AssetDatabase.CreateFolder("Assets", "SporosGame");
        if (!AssetDatabase.IsValidFolder(PrefabsFolder))
            AssetDatabase.CreateFolder("Assets/SporosGame", "Prefabs");
        if (!AssetDatabase.IsValidFolder(SpritesFolder))
            AssetDatabase.CreateFolder("Assets/SporosGame", "GeneratedSprites");
    }

    private static Sprite GetOrCreateHexSprite()
    {
        string path = SpritesFolder + "/hex.png";
        if (File.Exists(path)) File.Delete(path);
        if (File.Exists(path + ".meta")) File.Delete(path + ".meta");

        int width = 256;
        int height = (int)(width / 0.866f);
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        var cols = new Color[width * height];
        float cx = width / 2f;
        float cy = height / 2f;
        float halfW = width / 2f;
        float halfH = height / 2f;
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            float px = (x - cx) / halfW;
            float py = (y - cy) / halfH;
            float ax = Mathf.Abs(px);
            float ay = Mathf.Abs(py);
            float h = Mathf.Max(ay + ax * 0.5f, ax);
            cols[y * width + x] = h <= 1f ? Color.white : new Color(0, 0, 0, 0);
        }
        tex.SetPixels(cols);
        return SaveTextureAsSprite(tex, path, false, 0, width);
    }

    private static Sprite GetOrCreateCircleSprite()
    {
        string path = SpritesFolder + "/circle.png";
        if (File.Exists(path))
        {
            AssetDatabase.ImportAsset(path);
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null) return existing;
        }
        int size = 256;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var cols = new Color[size * size];
        Vector2 c = new Vector2(size / 2f, size / 2f);
        float r = size / 2f - 2f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Vector2.Distance(new Vector2(x, y), c);
            float a = Mathf.Clamp01(1f - (d - r) / 2f);
            if (d > r + 2f) a = 0f;
            if (d <= r) a = 1f;
            cols[y * size + x] = new Color(1f, 1f, 1f, a);
        }
        tex.SetPixels(cols);
        return SaveTextureAsSprite(tex, path, false);
    }

    private static Sprite GetOrCreateSquareSprite()
    {
        string path = SpritesFolder + "/square.png";
        if (File.Exists(path))
        {
            AssetDatabase.ImportAsset(path);
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null) return existing;
        }
        int size = 16;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var cols = new Color[size * size];
        for (int i = 0; i < cols.Length; i++) cols[i] = Color.white;
        tex.SetPixels(cols);
        return SaveTextureAsSprite(tex, path, false);
    }

    private static Sprite GetOrCreateRoundedRectSprite()
    {
        string path = SpritesFolder + "/rounded.png";
        if (File.Exists(path))
        {
            AssetDatabase.ImportAsset(path);
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null) return existing;
        }
        int size = 128;
        int radius = 32;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var cols = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            bool inside = true;
            int rcx = -1, rcy = -1;
            if (x < radius && y < radius) { rcx = radius; rcy = radius; }
            else if (x >= size - radius && y < radius) { rcx = size - radius - 1; rcy = radius; }
            else if (x < radius && y >= size - radius) { rcx = radius; rcy = size - radius - 1; }
            else if (x >= size - radius && y >= size - radius) { rcx = size - radius - 1; rcy = size - radius - 1; }
            if (rcx >= 0)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(rcx, rcy));
                inside = d <= radius;
            }
            cols[y * size + x] = inside ? Color.white : new Color(0, 0, 0, 0);
        }
        tex.SetPixels(cols);
        return SaveTextureAsSprite(tex, path, true, radius);
    }

    private static Sprite SaveTextureAsSprite(Texture2D tex, string path, bool sliced, int border = 0, int ppu = 100)
    {
        tex.Apply();
        var png = tex.EncodeToPNG();
        File.WriteAllBytes(path, png);
        AssetDatabase.ImportAsset(path);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = ppu;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;
        if (sliced)
        {
            var ss = new SpriteMetaData
            {
                name = Path.GetFileNameWithoutExtension(path),
                rect = new Rect(0, 0, tex.width, tex.height),
                alignment = (int)SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f),
                border = new Vector4(border, border, border, border)
            };
            importer.spritesheet = new[] { ss };
        }
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static GameObject CreateCellPrefab(Sprite hex)
    {
        var root = new GameObject("Cell");
        root.transform.localScale = Vector3.one;
        var cell = root.AddComponent<Cell>();

        var outlineGo = new GameObject("Outline");
        outlineGo.transform.SetParent(root.transform, false);
        outlineGo.transform.localScale = Vector3.one;
        var outlineSr = outlineGo.AddComponent<SpriteRenderer>();
        outlineSr.sprite = hex;
        outlineSr.color = ColorOutline;
        outlineSr.sortingOrder = 0;

        var fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(root.transform, false);
        fillGo.transform.localScale = Vector3.one * 0.88f;
        var fillSr = fillGo.AddComponent<SpriteRenderer>();
        fillSr.sprite = hex;
        fillSr.color = ColorPanel;
        fillSr.sortingOrder = 1;

        var glowGo = new GameObject("Glow");
        glowGo.transform.SetParent(root.transform, false);
        glowGo.transform.localScale = Vector3.one;
        var glowSr = glowGo.AddComponent<SpriteRenderer>();
        glowSr.sprite = hex;
        glowSr.color = new Color(0f, 0.898f, 1f, 0.45f);
        glowSr.sortingOrder = 2;
        glowGo.SetActive(false);

        SerializedObject so = new SerializedObject(cell);
        so.FindProperty("fillRenderer").objectReferenceValue = fillSr;
        so.FindProperty("outlineRenderer").objectReferenceValue = outlineSr;
        so.FindProperty("glowRenderer").objectReferenceValue = glowSr;
        so.ApplyModifiedProperties();

        string path = PrefabsFolder + "/Cell.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreateRaySegmentPrefab(Sprite square)
    {
        var root = new GameObject("RaySegment");
        root.transform.localScale = Vector3.one;
        var seg = root.AddComponent<RaySegment>();

        var glowGo = new GameObject("Glow");
        glowGo.transform.SetParent(root.transform, false);
        var glowSr = glowGo.AddComponent<SpriteRenderer>();
        glowSr.sprite = square;
        glowSr.color = new Color(0f, 0.898f, 1f, 0.5f);
        glowSr.sortingOrder = 5;
        glowGo.transform.localScale = new Vector3(1f, 0.4f, 1f);

        var lineGo = new GameObject("Line");
        lineGo.transform.SetParent(root.transform, false);
        var lineSr = lineGo.AddComponent<SpriteRenderer>();
        lineSr.sprite = square;
        lineSr.color = new Color(0f, 0.898f, 1f, 1f);
        lineSr.sortingOrder = 6;
        lineGo.transform.localScale = new Vector3(1f, 0.18f, 1f);

        SerializedObject so = new SerializedObject(seg);
        so.FindProperty("lineRenderer").objectReferenceValue = lineSr;
        so.FindProperty("glowRenderer").objectReferenceValue = glowSr;
        so.ApplyModifiedProperties();

        string path = PrefabsFolder + "/RaySegment.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreateSporePrefab(Sprite circle, GameObject raySegPrefab)
    {
        var root = new GameObject("Spore");
        root.transform.localScale = Vector3.one;
        var spore = root.AddComponent<Spore>();

        var glowGo = new GameObject("Glow");
        glowGo.transform.SetParent(root.transform, false);
        var glowSr = glowGo.AddComponent<SpriteRenderer>();
        glowSr.sprite = circle;
        glowSr.color = new Color(1f, 0f, 0.898f, 0.55f);
        glowSr.sortingOrder = 8;
        glowGo.transform.localScale = Vector3.one * 1.3f;

        var coreGo = new GameObject("Core");
        coreGo.transform.SetParent(root.transform, false);
        var coreSr = coreGo.AddComponent<SpriteRenderer>();
        coreSr.sprite = circle;
        coreSr.color = new Color(1f, 0f, 0.898f, 1f);
        coreSr.sortingOrder = 9;
        coreGo.transform.localScale = Vector3.one * 0.75f;

        SerializedObject so = new SerializedObject(spore);
        so.FindProperty("coreRenderer").objectReferenceValue = coreSr;
        so.FindProperty("glowRenderer").objectReferenceValue = glowSr;
        so.FindProperty("rayPrefab").objectReferenceValue = raySegPrefab;
        so.ApplyModifiedProperties();

        string path = PrefabsFolder + "/Spore.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreateInventoryItemPrefab(Sprite circle, Sprite rounded)
    {
        var root = new GameObject("SporeInventoryItem");
        var rt = root.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(180, 180);
        var bg = root.AddComponent<Image>();
        bg.sprite = rounded;
        bg.type = Image.Type.Sliced;
        bg.color = ColorPanel2;
        bg.raycastTarget = true;

        var item = root.AddComponent<SporeInventoryItem>();

        var glowGo = new GameObject("Glow");
        glowGo.transform.SetParent(root.transform, false);
        var glowRt = glowGo.AddComponent<RectTransform>();
        glowRt.anchorMin = new Vector2(0.5f, 0.5f);
        glowRt.anchorMax = new Vector2(0.5f, 0.5f);
        glowRt.sizeDelta = new Vector2(140, 140);
        glowRt.anchoredPosition = new Vector2(0, 10);
        var glowImg = glowGo.AddComponent<Image>();
        glowImg.sprite = circle;
        glowImg.color = new Color(1f, 0f, 0.898f, 0.45f);
        glowImg.raycastTarget = false;

        var iconGo = new GameObject("Icon");
        iconGo.transform.SetParent(root.transform, false);
        var iconRt = iconGo.AddComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.5f, 0.5f);
        iconRt.anchorMax = new Vector2(0.5f, 0.5f);
        iconRt.sizeDelta = new Vector2(100, 100);
        iconRt.anchoredPosition = new Vector2(0, 10);
        var iconImg = iconGo.AddComponent<Image>();
        iconImg.sprite = circle;
        iconImg.color = new Color(1f, 0f, 0.898f, 1f);
        iconImg.raycastTarget = false;

        var countGo = new GameObject("Count");
        countGo.transform.SetParent(root.transform, false);
        var countRt = countGo.AddComponent<RectTransform>();
        countRt.anchorMin = new Vector2(0.5f, 0f);
        countRt.anchorMax = new Vector2(0.5f, 0f);
        countRt.sizeDelta = new Vector2(160, 50);
        countRt.anchoredPosition = new Vector2(0, 18);
        var countTxt = countGo.AddComponent<TextMeshProUGUI>();
        countTxt.text = "x0";
        countTxt.fontSize = 38;
        countTxt.fontStyle = FontStyles.Bold;
        countTxt.color = Color.white;
        countTxt.alignment = TextAlignmentOptions.Center;
        countTxt.raycastTarget = false;

        var disabledGo = new GameObject("Disabled");
        disabledGo.transform.SetParent(root.transform, false);
        var disRt = disabledGo.AddComponent<RectTransform>();
        disRt.anchorMin = Vector2.zero;
        disRt.anchorMax = Vector2.one;
        disRt.offsetMin = Vector2.zero;
        disRt.offsetMax = Vector2.zero;
        var disImg = disabledGo.AddComponent<Image>();
        disImg.sprite = rounded;
        disImg.type = Image.Type.Sliced;
        disImg.color = new Color(0f, 0f, 0f, 0.55f);
        disImg.raycastTarget = false;
        disabledGo.SetActive(false);

        SerializedObject so = new SerializedObject(item);
        so.FindProperty("icon").objectReferenceValue = iconImg;
        so.FindProperty("glow").objectReferenceValue = glowImg;
        so.FindProperty("countText").objectReferenceValue = countTxt;
        so.FindProperty("disabledOverlay").objectReferenceValue = disImg;
        so.ApplyModifiedProperties();

        string path = PrefabsFolder + "/SporeInventoryItem.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static void ClearScene(Scene scene)
    {
        var roots = scene.GetRootGameObjects();
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
        cam.orthographicSize = 6;
        cam.transform.position = new Vector3(0, 0, -10);
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

    private static void CreateBackgroundCanvas(Sprite circle)
    {
        var canvasGo = new GameObject("BackgroundCanvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = Camera.main;
        canvas.planeDistance = 80f;
        canvas.sortingOrder = -100;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 1f;
        canvasGo.AddComponent<GraphicRaycaster>().enabled = false;

        var bgGo = new GameObject("BgFill");
        bgGo.transform.SetParent(canvasGo.transform, false);
        var img = bgGo.AddComponent<Image>();
        img.color = ColorBg;
        img.raycastTarget = false;
        var rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        for (int i = 0; i < 3; i++)
        {
            var dot = new GameObject("Glow_" + i);
            dot.transform.SetParent(canvasGo.transform, false);
            var dimg = dot.AddComponent<Image>();
            dimg.sprite = circle;
            dimg.color = i % 2 == 0
                ? new Color(ColorPrimary.r, ColorPrimary.g, ColorPrimary.b, 0.10f)
                : new Color(ColorAccent.r, ColorAccent.g, ColorAccent.b, 0.08f);
            dimg.raycastTarget = false;
            var drt = dimg.rectTransform;
            drt.sizeDelta = new Vector2(800 + i * 80, 800 + i * 80);
            drt.anchorMin = new Vector2(0.5f, 0.5f);
            drt.anchorMax = new Vector2(0.5f, 0.5f);
            float x = (i % 2 == 0 ? -1f : 1f) * (200 + i * 50);
            float y = (i < 2 ? 1f : -1f) * (400 + i * 80);
            drt.anchoredPosition = new Vector2(x, y);
        }
    }

    private static Canvas CreateUICanvas(string name)
    {
        var go = new GameObject(name);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
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

    private static (HUDController, Button, TMP_Text, TMP_Text) CreateHUD(Transform parent, Sprite rounded)
    {
        var hudGo = new GameObject("HUD");
        hudGo.transform.SetParent(parent, false);
        var hudRt = hudGo.AddComponent<RectTransform>();
        hudRt.anchorMin = new Vector2(0, 1);
        hudRt.anchorMax = new Vector2(1, 1);
        hudRt.pivot = new Vector2(0.5f, 1);
        hudRt.anchoredPosition = Vector2.zero;
        hudRt.sizeDelta = new Vector2(0, 200);

        var backBtn = CreateButton(hudGo.transform, "BackButton", "‹", ColorPanel, ColorText, 80, rounded);
        var brt = backBtn.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0, 1);
        brt.anchorMax = new Vector2(0, 1);
        brt.pivot = new Vector2(0, 1);
        brt.anchoredPosition = new Vector2(40, -40);
        brt.sizeDelta = new Vector2(140, 140);

        var levelTxt = CreateText(hudGo.transform, "LevelText", "LEVEL 1", 70, FontStyles.Bold, ColorPrimary);
        var lrt = levelTxt.rectTransform;
        lrt.anchorMin = new Vector2(0.5f, 1f);
        lrt.anchorMax = new Vector2(0.5f, 1f);
        lrt.pivot = new Vector2(0.5f, 1f);
        lrt.anchoredPosition = new Vector2(0, -65);
        lrt.sizeDelta = new Vector2(500, 100);
        AddOutline(levelTxt, ColorAccent, 3);

        var timerTxt = CreateText(hudGo.transform, "TimerText", "00:00", 50, FontStyles.Bold, ColorText);
        var trt = timerTxt.rectTransform;
        trt.anchorMin = new Vector2(1, 1);
        trt.anchorMax = new Vector2(1, 1);
        trt.pivot = new Vector2(1, 1);
        trt.anchoredPosition = new Vector2(-50, -85);
        trt.sizeDelta = new Vector2(280, 80);
        timerTxt.alignment = TextAlignmentOptions.Right;

        var hud = hudGo.AddComponent<HUDController>();
        SerializedObject so = new SerializedObject(hud);
        so.FindProperty("backButton").objectReferenceValue = backBtn;
        so.FindProperty("levelText").objectReferenceValue = levelTxt;
        so.FindProperty("timerText").objectReferenceValue = timerTxt;
        so.ApplyModifiedProperties();

        return (hud, backBtn, levelTxt, timerTxt);
    }

    private static SporeInventory CreateInventoryPanel(Transform parent, Sprite rounded, GameObject itemPrefab)
    {
        var panelGo = new GameObject("InventoryPanel");
        panelGo.transform.SetParent(parent, false);
        var prt = panelGo.AddComponent<RectTransform>();
        prt.anchorMin = new Vector2(0, 0);
        prt.anchorMax = new Vector2(1, 0);
        prt.pivot = new Vector2(0.5f, 0);
        prt.anchoredPosition = new Vector2(0, 40);
        prt.sizeDelta = new Vector2(-80, 280);

        var bgImg = panelGo.AddComponent<Image>();
        bgImg.sprite = rounded;
        bgImg.type = Image.Type.Sliced;
        bgImg.color = ColorPanel2;
        bgImg.raycastTarget = true;

        var itemsGo = new GameObject("Items");
        itemsGo.transform.SetParent(panelGo.transform, false);
        var iRt = itemsGo.AddComponent<RectTransform>();
        iRt.anchorMin = Vector2.zero;
        iRt.anchorMax = Vector2.one;
        iRt.offsetMin = new Vector2(30, 30);
        iRt.offsetMax = new Vector2(-30, -30);
        var hg = itemsGo.AddComponent<HorizontalLayoutGroup>();
        hg.spacing = 30;
        hg.childAlignment = TextAnchor.MiddleCenter;
        hg.childForceExpandHeight = false;
        hg.childForceExpandWidth = false;

        var inv = panelGo.AddComponent<SporeInventory>();
        SerializedObject so = new SerializedObject(inv);
        so.FindProperty("itemsParent").objectReferenceValue = iRt;
        so.FindProperty("itemPrefab").objectReferenceValue = itemPrefab;
        so.ApplyModifiedProperties();
        return inv;
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
