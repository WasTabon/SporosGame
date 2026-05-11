using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_PURCHASING
using UnityEngine.Purchasing;
#endif

public class Iteration08_Setup : EditorWindow
{
    private static readonly Color ColorBg       = new Color(0.039f, 0.055f, 0.153f, 1f);
    private static readonly Color ColorPrimary  = new Color(0.000f, 0.898f, 1.000f, 1f);
    private static readonly Color ColorAccent   = new Color(1.000f, 0.000f, 0.898f, 1f);
    private static readonly Color ColorText     = Color.white;
    private static readonly Color ColorPanel    = new Color(0.078f, 0.102f, 0.231f, 1f);
    private static readonly Color ColorPanel2   = new Color(0.078f, 0.102f, 0.231f, 0.95f);
    private static readonly Color ColorOutline  = new Color(0.227f, 0.263f, 0.408f, 1f);
    private static readonly Color ColorSuccess  = new Color(0f, 1f, 0.533f, 1f);

    private const string SpritesFolder = "Assets/SporosGame/GeneratedSprites";
    private const string DataFolder = "Assets/SporosGame/Data";
    private const string ResourcesFolder = "Assets/SporosGame/Resources";
    private const string MainMenuScene = "Assets/SporosGame/Scenes/MainMenu.unity";

    [MenuItem("Tools/SporosGame/Iteration 8/Settings + Shop + Extra Levels (Iteration 8)")]
    public static void Setup()
    {
        var rounded = AssetDatabase.LoadAssetAtPath<Sprite>(SpritesFolder + "/rounded.png");
        var hex = AssetDatabase.LoadAssetAtPath<Sprite>(SpritesFolder + "/hex.png");
        if (rounded == null || hex == null)
        {
            EditorUtility.DisplayDialog("SporosGame", "Sprites missing. Run Iteration 2 first.", "OK");
            return;
        }

        AddExtraLevels();
        RebuildPopupsInMainMenu(rounded, hex);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("SporosGame",
            "Iteration 8 complete.\n\nNEXT STEPS:\n" +
            "1. Open IAP Catalog (Services → IAP Catalog), add product: com.levelpack.inapp (NonConsumable)\n" +
            "2. On Shop popup → BuyButton (IAPButton component) — confirm Product ID is 'com.levelpack.inapp'\n" +
            "3. Run Tools → SporosGame → Iteration 7 → Auto-Solve and Balance Levels to balance L21-L30",
            "OK");
    }

    private static void AddExtraLevels()
    {
        var dbPath = ResourcesFolder + "/LevelDatabase.asset";
        var db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(dbPath);
        if (db == null) return;

        var specs = BuildExtraSpecs();

        var existingList = new List<LevelData>();
        if (db.levels != null) existingList.AddRange(db.levels);

        for (int i = existingList.Count - 1; i >= 0; i--)
            if (existingList[i] != null && existingList[i].isExtraPack)
                existingList.RemoveAt(i);

        for (int i = 0; i < specs.Length; i++)
        {
            var s = specs[i];
            string path = DataFolder + "/Level_" + s.idx.ToString("00") + ".asset";
            var data = AssetDatabase.LoadAssetAtPath<LevelData>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<LevelData>();
                AssetDatabase.CreateAsset(data, path);
            }
            data.levelIndex = s.idx;
            data.width = s.w;
            data.height = s.h;
            data.isExtraPack = true;
            data.rows = new CellTypeRow[s.h];
            for (int y = 0; y < s.h; y++)
            {
                data.rows[y] = new CellTypeRow { cells = new CellType[s.w] };
                for (int x = 0; x < s.w; x++) data.rows[y].cells[x] = CellType.Normal;
            }
            if (s.blocks != null) foreach (var b in s.blocks) if (InGrid(b, s.w, s.h)) data.rows[b.y].cells[b.x] = CellType.Block;
            if (s.fixedCells != null) foreach (var f in s.fixedCells) if (InGrid(f, s.w, s.h)) data.rows[f.y].cells[f.x] = CellType.Fixed;
            if (s.limited != null) foreach (var l in s.limited) if (InGrid(l, s.w, s.h)) data.rows[l.y].cells[l.x] = CellType.Limited;

            var sporeList = new List<SporeStockEntry>();
            if (s.basicCount > 0) sporeList.Add(new SporeStockEntry { type = SporeType.Basic, count = s.basicCount });
            if (s.diagonalCount > 0) sporeList.Add(new SporeStockEntry { type = SporeType.Diagonal, count = s.diagonalCount });
            data.spores = sporeList.ToArray();

            int cells = s.w * s.h;
            int blockCount = s.blocks != null ? s.blocks.Length : 0;
            int playable = cells - blockCount;
            data.minSporesForThreeStars = Mathf.Max(1, Mathf.CeilToInt((s.basicCount + s.diagonalCount) * 0.7f));
            data.maxSporesForOneStar = s.basicCount + s.diagonalCount;
            data.timeForThreeStars = playable * 3f;
            data.timeForOneStar = playable * 12f;
            data.coinsReward = 20 + cells * 4;

            EditorUtility.SetDirty(data);
            existingList.Add(data);
        }

        existingList.Sort((a, b) => a.levelIndex.CompareTo(b.levelIndex));
        db.levels = existingList.ToArray();
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
    }

    private static bool InGrid(Vector2Int p, int w, int h) { return p.x >= 0 && p.x < w && p.y >= 0 && p.y < h; }

    private struct ExtraSpec { public int idx, w, h, basicCount, diagonalCount; public Vector2Int[] blocks, fixedCells, limited; }

    private static ExtraSpec[] BuildExtraSpecs()
    {
        return new ExtraSpec[]
        {
            new ExtraSpec { idx=21, w=6, h=6, basicCount=5, diagonalCount=2,
                blocks = new[]{ new Vector2Int(2,2), new Vector2Int(3,3), new Vector2Int(2,3), new Vector2Int(3,2) },
                fixedCells = new[]{ new Vector2Int(0,0), new Vector2Int(5,5) } },
            new ExtraSpec { idx=22, w=6, h=6, basicCount=4, diagonalCount=4,
                blocks = new[]{ new Vector2Int(1,1), new Vector2Int(4,4) },
                limited = new[]{ new Vector2Int(2,3), new Vector2Int(3,2) },
                fixedCells = new[]{ new Vector2Int(0,5), new Vector2Int(5,0) } },
            new ExtraSpec { idx=23, w=7, h=6, basicCount=6, diagonalCount=2,
                blocks = new[]{ new Vector2Int(3,1), new Vector2Int(3,4) },
                limited = new[]{ new Vector2Int(1,2), new Vector2Int(5,3) },
                fixedCells = new[]{ new Vector2Int(3,3), new Vector2Int(0,0), new Vector2Int(6,5) } },
            new ExtraSpec { idx=24, w=7, h=7, basicCount=5, diagonalCount=4,
                blocks = new[]{ new Vector2Int(2,2), new Vector2Int(4,4), new Vector2Int(2,4), new Vector2Int(4,2) },
                limited = new[]{ new Vector2Int(3,3) },
                fixedCells = new[]{ new Vector2Int(0,3), new Vector2Int(6,3), new Vector2Int(3,0), new Vector2Int(3,6) } },
            new ExtraSpec { idx=25, w=7, h=7, basicCount=4, diagonalCount=5,
                blocks = new[]{ new Vector2Int(1,3), new Vector2Int(5,3), new Vector2Int(3,1), new Vector2Int(3,5) },
                limited = new[]{ new Vector2Int(2,2), new Vector2Int(4,4), new Vector2Int(2,4), new Vector2Int(4,2) },
                fixedCells = new[]{ new Vector2Int(3,3) } },
            new ExtraSpec { idx=26, w=7, h=7, basicCount=6, diagonalCount=4,
                blocks = new[]{ new Vector2Int(3,0), new Vector2Int(3,6), new Vector2Int(0,3), new Vector2Int(6,3) },
                limited = new[]{ new Vector2Int(1,1), new Vector2Int(5,5), new Vector2Int(1,5), new Vector2Int(5,1) },
                fixedCells = new[]{ new Vector2Int(3,3), new Vector2Int(2,2), new Vector2Int(4,4) } },
            new ExtraSpec { idx=27, w=7, h=7, basicCount=5, diagonalCount=5,
                blocks = new[]{ new Vector2Int(2,3), new Vector2Int(4,3), new Vector2Int(3,2), new Vector2Int(3,4), new Vector2Int(3,3) },
                limited = new[]{ new Vector2Int(1,3), new Vector2Int(5,3), new Vector2Int(3,1), new Vector2Int(3,5) },
                fixedCells = new[]{ new Vector2Int(0,0), new Vector2Int(6,6), new Vector2Int(0,6), new Vector2Int(6,0) } },
            new ExtraSpec { idx=28, w=7, h=7, basicCount=4, diagonalCount=6,
                blocks = new[]{ new Vector2Int(1,1), new Vector2Int(5,1), new Vector2Int(1,5), new Vector2Int(5,5) },
                limited = new[]{ new Vector2Int(3,1), new Vector2Int(3,5), new Vector2Int(1,3), new Vector2Int(5,3), new Vector2Int(3,3) },
                fixedCells = new[]{ new Vector2Int(2,2), new Vector2Int(4,4), new Vector2Int(2,4), new Vector2Int(4,2) } },
            new ExtraSpec { idx=29, w=7, h=7, basicCount=6, diagonalCount=5,
                blocks = new[]{ new Vector2Int(0,3), new Vector2Int(6,3), new Vector2Int(3,0), new Vector2Int(3,6), new Vector2Int(2,2), new Vector2Int(4,4) },
                limited = new[]{ new Vector2Int(3,3), new Vector2Int(2,4), new Vector2Int(4,2) },
                fixedCells = new[]{ new Vector2Int(1,1), new Vector2Int(5,5), new Vector2Int(1,5), new Vector2Int(5,1) } },
            new ExtraSpec { idx=30, w=7, h=7, basicCount=7, diagonalCount=6,
                blocks = new[]{ new Vector2Int(2,2), new Vector2Int(2,4), new Vector2Int(4,2), new Vector2Int(4,4), new Vector2Int(3,1), new Vector2Int(3,5) },
                limited = new[]{ new Vector2Int(1,3), new Vector2Int(5,3), new Vector2Int(2,3), new Vector2Int(4,3) },
                fixedCells = new[]{ new Vector2Int(3,3), new Vector2Int(0,0), new Vector2Int(6,6), new Vector2Int(0,6), new Vector2Int(6,0) } },
        };
    }

    private static void RebuildPopupsInMainMenu(Sprite rounded, Sprite hex)
    {
        var scene = EditorSceneManager.OpenScene(MainMenuScene, OpenSceneMode.Single);

        Canvas canvas = null;
        var allCanvases = Object.FindObjectsOfType<Canvas>();
        for (int i = 0; i < allCanvases.Length; i++)
            if (allCanvases[i].name == "MainMenuCanvas") { canvas = allCanvases[i]; break; }
        if (canvas == null) return;

        var oldSettings = canvas.transform.Find("SettingsPopup");
        if (oldSettings != null) Object.DestroyImmediate(oldSettings.gameObject);
        var oldShop = canvas.transform.Find("ShopPopup");
        if (oldShop != null) Object.DestroyImmediate(oldShop.gameObject);

        var settingsPopup = CreateSettingsPopup(canvas.transform, rounded);
        var shopPopup = CreateShopPopup(canvas.transform, rounded, hex);

        var mm = Object.FindObjectOfType<MainMenuController>();
        if (mm != null)
        {
            SerializedObject so = new SerializedObject(mm);
            so.FindProperty("settingsPopup").objectReferenceValue = settingsPopup;
            so.FindProperty("shopPopup").objectReferenceValue = shopPopup;
            so.ApplyModifiedProperties();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static SettingsPopup CreateSettingsPopup(Transform parent, Sprite rounded)
    {
        var go = new GameObject("SettingsPopup");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var cg = go.AddComponent<CanvasGroup>();

        var (backdrop, content) = AddBackdropAndContent(go.transform, rounded, 900, 1300);

        var title = CreateText(content, "Title", "SETTINGS", 90, FontStyles.Bold, ColorPrimary);
        SetAnchored(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -90), new Vector2(820, 130));
        AddOutline(title, ColorAccent, 3);

        var sfxRow = CreateSliderRow(content, "SfxRow", "SFX", rounded);
        SetAnchored(sfxRow.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -300), new Vector2(800, 120));

        var musicRow = CreateSliderRow(content, "MusicRow", "MUSIC", rounded);
        SetAnchored(musicRow.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -440), new Vector2(800, 120));

        var (hapticsToggleGo, hapticsToggle) = CreateToggleRow(content, "HapticsRow", "HAPTICS", rounded);
        SetAnchored(hapticsToggleGo.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -580), new Vector2(800, 120));

        var restoreBtn = CreateButton(content, "RestoreButton", "RESTORE PURCHASES", ColorPanel, ColorText, 40, rounded);
        SetAnchored(restoreBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 280), new Vector2(620, 130));

        var closeBtn = CreateButton(content, "CloseButton", "CLOSE", ColorAccent, ColorBg, 55, rounded);
        SetAnchored(closeBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 100), new Vector2(500, 150));

        var popup = go.AddComponent<SettingsPopup>();
        SerializedObject so = new SerializedObject(popup);
        so.FindProperty("content").objectReferenceValue = content;
        so.FindProperty("backdrop").objectReferenceValue = backdrop;
        so.FindProperty("canvasGroup").objectReferenceValue = cg;
        so.FindProperty("sfxRow").objectReferenceValue = sfxRow;
        so.FindProperty("musicRow").objectReferenceValue = musicRow;
        so.FindProperty("hapticsToggle").objectReferenceValue = hapticsToggle;
        so.FindProperty("restoreButton").objectReferenceValue = restoreBtn;
        so.FindProperty("closeButton").objectReferenceValue = closeBtn;
        so.ApplyModifiedProperties();

        go.SetActive(false);
        return popup;
    }

    private static ShopPopup CreateShopPopup(Transform parent, Sprite rounded, Sprite hex)
    {
        var go = new GameObject("ShopPopup");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var cg = go.AddComponent<CanvasGroup>();

        var (backdrop, content) = AddBackdropAndContent(go.transform, rounded, 900, 1400);

        var title = CreateText(content, "Title", "EXTRA LEVELS", 80, FontStyles.Bold, ColorAccent);
        SetAnchored(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -90), new Vector2(820, 130));
        AddOutline(title, ColorPrimary, 3);

        var desc = CreateText(content, "Description", "10 unique levels\nwith advanced mechanics", 42, FontStyles.Italic, new Color(1f, 1f, 1f, 0.8f));
        SetAnchored(desc.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -250), new Vector2(800, 140));

        var previewsGo = new GameObject("Previews");
        previewsGo.transform.SetParent(content, false);
        var pRt = previewsGo.AddComponent<RectTransform>();
        SetAnchored(pRt, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -430), new Vector2(760, 280));
        var grid = previewsGo.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(130, 130);
        grid.spacing = new Vector2(15, 15);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;
        grid.childAlignment = TextAnchor.UpperCenter;

        for (int i = 21; i <= 30; i++)
        {
            var iconGo = new GameObject("Lv" + i);
            iconGo.transform.SetParent(previewsGo.transform, false);
            var iconRt = iconGo.AddComponent<RectTransform>();
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.sprite = hex;
            iconImg.color = ColorAccent;
            iconImg.raycastTarget = false;

            var inner = new GameObject("Inner");
            inner.transform.SetParent(iconGo.transform, false);
            var iiRt = inner.AddComponent<RectTransform>();
            iiRt.anchorMin = Vector2.zero; iiRt.anchorMax = Vector2.one;
            iiRt.offsetMin = new Vector2(8, 8); iiRt.offsetMax = new Vector2(-8, -8);
            var iiImg = inner.AddComponent<Image>();
            iiImg.sprite = hex;
            iiImg.color = ColorBg;
            iiImg.raycastTarget = false;

            var numGo = new GameObject("Num");
            numGo.transform.SetParent(iconGo.transform, false);
            var nRt = numGo.AddComponent<RectTransform>();
            nRt.anchorMin = Vector2.zero; nRt.anchorMax = Vector2.one;
            nRt.offsetMin = Vector2.zero; nRt.offsetMax = Vector2.zero;
            var nTmp = numGo.AddComponent<TextMeshProUGUI>();
            nTmp.text = i.ToString();
            nTmp.fontSize = 45;
            nTmp.fontStyle = FontStyles.Bold;
            nTmp.color = ColorAccent;
            nTmp.alignment = TextAlignmentOptions.Center;
            nTmp.raycastTarget = false;
        }

        var buyGo = new GameObject("BuyButton");
        buyGo.transform.SetParent(content, false);
        var buyImg = buyGo.AddComponent<Image>();
        buyImg.sprite = rounded;
        buyImg.type = Image.Type.Sliced;
        buyImg.color = ColorSuccess;
        var buyRt = buyGo.GetComponent<RectTransform>();
        SetAnchored(buyRt, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 320), new Vector2(620, 170));

        var buyBtn = buyGo.AddComponent<Button>();
        buyBtn.targetGraphic = buyImg;
        buyGo.AddComponent<ButtonAnimator>();

        var buyLabelGo = new GameObject("Label");
        buyLabelGo.transform.SetParent(buyGo.transform, false);
        var buyLabelRt = buyLabelGo.AddComponent<RectTransform>();
        buyLabelRt.anchorMin = Vector2.zero; buyLabelRt.anchorMax = Vector2.one;
        buyLabelRt.offsetMin = Vector2.zero; buyLabelRt.offsetMax = Vector2.zero;
        var buyLabel = buyLabelGo.AddComponent<TextMeshProUGUI>();
        buyLabel.text = "UNLOCK";
        buyLabel.fontSize = 60;
        buyLabel.fontStyle = FontStyles.Bold;
        buyLabel.color = ColorBg;
        buyLabel.alignment = TextAlignmentOptions.Center;
        buyLabel.raycastTarget = false;

        var priceGo = new GameObject("Price");
        priceGo.transform.SetParent(content, false);
        var priceTmp = priceGo.AddComponent<TextMeshProUGUI>();
        priceTmp.text = "$0.99";
        priceTmp.fontSize = 45;
        priceTmp.fontStyle = FontStyles.Bold;
        priceTmp.color = ColorSuccess;
        priceTmp.alignment = TextAlignmentOptions.Center;
        priceTmp.raycastTarget = false;
        SetAnchored(priceGo.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 510), new Vector2(400, 70));

#if UNITY_PURCHASING
        var iapBtn = buyGo.AddComponent<IAPButton>();
        iapBtn.productId = IAPManager.ExtraPackProductId;
        iapBtn.buttonType = IAPButton.ButtonType.Purchase;
#endif

        var ownedLabelGo = new GameObject("OwnedLabel");
        ownedLabelGo.transform.SetParent(content, false);
        var ownedRt = ownedLabelGo.AddComponent<RectTransform>();
        SetAnchored(ownedRt, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 320), new Vector2(620, 170));
        var ownedImg = ownedLabelGo.AddComponent<Image>();
        ownedImg.sprite = rounded;
        ownedImg.type = Image.Type.Sliced;
        ownedImg.color = ColorPanel;
        ownedImg.raycastTarget = false;
        var ownedTxtGo = new GameObject("Label");
        ownedTxtGo.transform.SetParent(ownedLabelGo.transform, false);
        var ownedTxtRt = ownedTxtGo.AddComponent<RectTransform>();
        ownedTxtRt.anchorMin = Vector2.zero; ownedTxtRt.anchorMax = Vector2.one;
        ownedTxtRt.offsetMin = Vector2.zero; ownedTxtRt.offsetMax = Vector2.zero;
        var ownedTxt = ownedTxtGo.AddComponent<TextMeshProUGUI>();
        ownedTxt.text = "OWNED";
        ownedTxt.fontSize = 60;
        ownedTxt.fontStyle = FontStyles.Bold;
        ownedTxt.color = ColorSuccess;
        ownedTxt.alignment = TextAlignmentOptions.Center;
        ownedTxt.raycastTarget = false;
        ownedLabelGo.SetActive(false);

        var restoreBtn = CreateButton(content, "RestoreButton", "RESTORE", ColorPanel, ColorText, 40, rounded);
        SetAnchored(restoreBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 170), new Vector2(420, 130));

        var closeBtn = CreateButton(content, "CloseButton", "CLOSE", ColorAccent, ColorBg, 50, rounded);
        SetAnchored(closeBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 30), new Vector2(420, 130));

        var popup = go.AddComponent<ShopPopup>();
        SerializedObject so = new SerializedObject(popup);
        so.FindProperty("content").objectReferenceValue = content;
        so.FindProperty("backdrop").objectReferenceValue = backdrop;
        so.FindProperty("canvasGroup").objectReferenceValue = cg;
        so.FindProperty("closeButton").objectReferenceValue = closeBtn;
        so.FindProperty("restoreButton").objectReferenceValue = restoreBtn;
        so.FindProperty("buyButtonGo").objectReferenceValue = buyGo;
        so.FindProperty("ownedLabelGo").objectReferenceValue = ownedLabelGo;
        so.FindProperty("priceText").objectReferenceValue = priceTmp;
        so.FindProperty("titleText").objectReferenceValue = title;
        so.FindProperty("descriptionText").objectReferenceValue = desc;
        so.ApplyModifiedProperties();

#if UNITY_PURCHASING
        var iapBtnComp = buyGo.GetComponent<IAPButton>();
        if (iapBtnComp != null)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(iapBtnComp.onPurchaseComplete, popup.OnPurchaseCompleted);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(iapBtnComp.onPurchaseFailed, (UnityEngine.Events.UnityAction<Product, PurchaseFailureReason>)popup.OnPurchaseFailedEvent);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(iapBtnComp.onProductFetched, popup.OnProductFetched);
        }
#endif

        go.SetActive(false);
        return popup;
    }

    private static SliderRow CreateSliderRow(Transform parent, string name, string label, Sprite rounded)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        var bg = go.AddComponent<Image>();
        bg.sprite = rounded;
        bg.type = Image.Type.Sliced;
        bg.color = ColorPanel;
        bg.raycastTarget = false;

        var labelGo = new GameObject("Label");
        labelGo.transform.SetParent(go.transform, false);
        var lRt = labelGo.AddComponent<RectTransform>();
        lRt.anchorMin = new Vector2(0f, 0f); lRt.anchorMax = new Vector2(0.3f, 1f);
        lRt.offsetMin = new Vector2(30, 0); lRt.offsetMax = Vector2.zero;
        var lTmp = labelGo.AddComponent<TextMeshProUGUI>();
        lTmp.text = label;
        lTmp.fontSize = 42;
        lTmp.fontStyle = FontStyles.Bold;
        lTmp.color = Color.white;
        lTmp.alignment = TextAlignmentOptions.MidlineLeft;
        lTmp.raycastTarget = false;

        var sliderGo = new GameObject("Slider");
        sliderGo.transform.SetParent(go.transform, false);
        var sRt = sliderGo.AddComponent<RectTransform>();
        sRt.anchorMin = new Vector2(0.3f, 0.3f); sRt.anchorMax = new Vector2(0.85f, 0.7f);
        sRt.offsetMin = Vector2.zero; sRt.offsetMax = Vector2.zero;
        var slider = sliderGo.AddComponent<Slider>();
        slider.minValue = 0f; slider.maxValue = 1f; slider.value = 1f;

        var bgImgGo = new GameObject("Background");
        bgImgGo.transform.SetParent(sliderGo.transform, false);
        var bgImgRt = bgImgGo.AddComponent<RectTransform>();
        bgImgRt.anchorMin = new Vector2(0f, 0.4f); bgImgRt.anchorMax = new Vector2(1f, 0.6f);
        bgImgRt.offsetMin = Vector2.zero; bgImgRt.offsetMax = Vector2.zero;
        var bgImg = bgImgGo.AddComponent<Image>();
        bgImg.sprite = rounded;
        bgImg.type = Image.Type.Sliced;
        bgImg.color = ColorOutline;

        var fillAreaGo = new GameObject("FillArea");
        fillAreaGo.transform.SetParent(sliderGo.transform, false);
        var faRt = fillAreaGo.AddComponent<RectTransform>();
        faRt.anchorMin = new Vector2(0f, 0.4f); faRt.anchorMax = new Vector2(1f, 0.6f);
        faRt.offsetMin = Vector2.zero; faRt.offsetMax = Vector2.zero;
        var fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(fillAreaGo.transform, false);
        var fRt = fillGo.AddComponent<RectTransform>();
        fRt.anchorMin = Vector2.zero; fRt.anchorMax = Vector2.one;
        fRt.offsetMin = Vector2.zero; fRt.offsetMax = Vector2.zero;
        var fillImg = fillGo.AddComponent<Image>();
        fillImg.sprite = rounded;
        fillImg.type = Image.Type.Sliced;
        fillImg.color = ColorPrimary;

        var handleAreaGo = new GameObject("HandleSlideArea");
        handleAreaGo.transform.SetParent(sliderGo.transform, false);
        var haRt = handleAreaGo.AddComponent<RectTransform>();
        haRt.anchorMin = Vector2.zero; haRt.anchorMax = Vector2.one;
        haRt.offsetMin = new Vector2(20, 0); haRt.offsetMax = new Vector2(-20, 0);
        var handleGo = new GameObject("Handle");
        handleGo.transform.SetParent(handleAreaGo.transform, false);
        var hRt = handleGo.AddComponent<RectTransform>();
        hRt.sizeDelta = new Vector2(40, 60);
        var handleImg = handleGo.AddComponent<Image>();
        handleImg.sprite = rounded;
        handleImg.type = Image.Type.Sliced;
        handleImg.color = ColorPrimary;

        slider.fillRect = fRt;
        slider.handleRect = hRt;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;

        var valGo = new GameObject("Value");
        valGo.transform.SetParent(go.transform, false);
        var vRt = valGo.AddComponent<RectTransform>();
        vRt.anchorMin = new Vector2(0.85f, 0f); vRt.anchorMax = new Vector2(1f, 1f);
        vRt.offsetMin = Vector2.zero; vRt.offsetMax = new Vector2(-20, 0);
        var vTmp = valGo.AddComponent<TextMeshProUGUI>();
        vTmp.text = "100";
        vTmp.fontSize = 40;
        vTmp.fontStyle = FontStyles.Bold;
        vTmp.color = ColorPrimary;
        vTmp.alignment = TextAlignmentOptions.MidlineRight;
        vTmp.raycastTarget = false;

        var row = go.AddComponent<SliderRow>();
        SerializedObject so = new SerializedObject(row);
        so.FindProperty("labelText").objectReferenceValue = lTmp;
        so.FindProperty("slider").objectReferenceValue = slider;
        so.FindProperty("valueText").objectReferenceValue = vTmp;
        so.ApplyModifiedProperties();

        return row;
    }

    private static (GameObject, Toggle) CreateToggleRow(Transform parent, string name, string label, Sprite rounded)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        var bg = go.AddComponent<Image>();
        bg.sprite = rounded;
        bg.type = Image.Type.Sliced;
        bg.color = ColorPanel;
        bg.raycastTarget = false;

        var labelGo = new GameObject("Label");
        labelGo.transform.SetParent(go.transform, false);
        var lRt = labelGo.AddComponent<RectTransform>();
        lRt.anchorMin = new Vector2(0f, 0f); lRt.anchorMax = new Vector2(0.6f, 1f);
        lRt.offsetMin = new Vector2(30, 0); lRt.offsetMax = Vector2.zero;
        var lTmp = labelGo.AddComponent<TextMeshProUGUI>();
        lTmp.text = label;
        lTmp.fontSize = 42;
        lTmp.fontStyle = FontStyles.Bold;
        lTmp.color = Color.white;
        lTmp.alignment = TextAlignmentOptions.MidlineLeft;
        lTmp.raycastTarget = false;

        var tGo = new GameObject("Toggle");
        tGo.transform.SetParent(go.transform, false);
        var tRt = tGo.AddComponent<RectTransform>();
        tRt.anchorMin = new Vector2(1f, 0.5f); tRt.anchorMax = new Vector2(1f, 0.5f);
        tRt.pivot = new Vector2(1f, 0.5f);
        tRt.anchoredPosition = new Vector2(-30, 0);
        tRt.sizeDelta = new Vector2(110, 70);
        var tBg = tGo.AddComponent<Image>();
        tBg.sprite = rounded;
        tBg.type = Image.Type.Sliced;
        tBg.color = ColorOutline;

        var checkGo = new GameObject("Check");
        checkGo.transform.SetParent(tGo.transform, false);
        var cRt = checkGo.AddComponent<RectTransform>();
        cRt.anchorMin = Vector2.zero; cRt.anchorMax = Vector2.one;
        cRt.offsetMin = new Vector2(8, 8); cRt.offsetMax = new Vector2(-8, -8);
        var checkImg = checkGo.AddComponent<Image>();
        checkImg.sprite = rounded;
        checkImg.type = Image.Type.Sliced;
        checkImg.color = ColorPrimary;

        var toggle = tGo.AddComponent<Toggle>();
        toggle.isOn = true;
        toggle.targetGraphic = tBg;
        toggle.graphic = checkImg;
        return (go, toggle);
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
        tmp.enableWordWrapping = true;
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
