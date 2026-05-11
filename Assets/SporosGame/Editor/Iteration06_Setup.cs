using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class Iteration06_Setup : EditorWindow
{
    private static readonly Color ColorBg       = new Color(0.039f, 0.055f, 0.153f, 1f);
    private static readonly Color ColorPrimary  = new Color(0.000f, 0.898f, 1.000f, 1f);
    private static readonly Color ColorAccent   = new Color(1.000f, 0.000f, 0.898f, 1f);
    private static readonly Color ColorCoin     = new Color(1.000f, 0.823f, 0.220f, 1f);
    private static readonly Color ColorCoinDark = new Color(0.800f, 0.580f, 0.090f, 1f);
    private static readonly Color ColorText     = Color.white;
    private static readonly Color ColorPanel    = new Color(0.078f, 0.102f, 0.231f, 1f);
    private static readonly Color ColorPanel2   = new Color(0.078f, 0.102f, 0.231f, 0.85f);

    private const string SpritesFolder = "Assets/SporosGame/GeneratedSprites";
    private const string PrefabsFolder = "Assets/SporosGame/Prefabs";
    private const string GameScene = "Assets/SporosGame/Scenes/Game.unity";
    private const string MainMenuScene = "Assets/SporosGame/Scenes/MainMenu.unity";
    private const string LevelSelectScene = "Assets/SporosGame/Scenes/LevelSelect.unity";

    [MenuItem("Tools/SporosGame/Iteration 6/Star Rating + Currency (Iteration 6)")]
    public static void Setup()
    {
        var rounded = AssetDatabase.LoadAssetAtPath<Sprite>(SpritesFolder + "/rounded.png");
        var circle = AssetDatabase.LoadAssetAtPath<Sprite>(SpritesFolder + "/circle.png");
        if (rounded == null || circle == null)
        {
            EditorUtility.DisplayDialog("SporosGame", "Sprites missing. Run Iteration 2 first.", "OK");
            return;
        }

        var coinSprite = GetOrCreateCoinSprite();
        var coinPrefab = CreateCoinIconPrefab(coinSprite);

        UpdateLevelDataThresholds();

        AddCoinCounterToGame(rounded, coinSprite, coinPrefab);
        AddCoinCounterToMainMenu(rounded, coinSprite);
        AddCoinCounterToLevelSelect(rounded, coinSprite);
        RebuildWinPopup(rounded, coinSprite, coinPrefab);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("SporosGame", "Iteration 6 complete.", "OK");
    }

    private static Sprite GetOrCreateCoinSprite()
    {
        string path = SpritesFolder + "/coin.png";
        if (File.Exists(path)) File.Delete(path);
        if (File.Exists(path + ".meta")) File.Delete(path + ".meta");

        int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var cols = new Color[size * size];
        Vector2 c = new Vector2(size / 2f, size / 2f);
        float outerR = size / 2f - 2f;
        float innerR = outerR - 10f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Vector2.Distance(new Vector2(x, y), c);
            if (d > outerR) { cols[y * size + x] = new Color(0, 0, 0, 0); continue; }
            if (d > innerR) cols[y * size + x] = ColorCoinDark;
            else cols[y * size + x] = ColorCoin;
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

    private static GameObject CreateCoinIconPrefab(Sprite coinSprite)
    {
        var root = new GameObject("CoinIcon");
        var rt = root.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(50, 50);
        var img = root.AddComponent<Image>();
        img.sprite = coinSprite;
        img.color = Color.white;
        img.raycastTarget = false;

        string path = PrefabsFolder + "/CoinIcon.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static void UpdateLevelDataThresholds()
    {
        var guids = AssetDatabase.FindAssets("t:LevelData", new[] { "Assets/SporosGame/Data" });
        for (int i = 0; i < guids.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[i]);
            var data = AssetDatabase.LoadAssetAtPath<LevelData>(path);
            if (data == null) continue;

            int cells = data.width * data.height;
            int given = data.TotalSporesGiven();
            if (given <= 0) given = 3;

            data.minSporesForThreeStars = Mathf.Max(1, Mathf.CeilToInt(given * 0.7f));
            data.maxSporesForOneStar = given;
            data.timeForThreeStars = cells * 3f;
            data.timeForOneStar = cells * 12f;
            data.coinsReward = 10 + cells * 3;

            EditorUtility.SetDirty(data);
        }
        AssetDatabase.SaveAssets();
    }

    private static void AddCoinCounterToGame(Sprite rounded, Sprite coinSprite, GameObject coinPrefab)
    {
        var scene = EditorSceneManager.OpenScene(GameScene, OpenSceneMode.Single);

        var hud = Object.FindObjectOfType<HUDController>();
        if (hud == null) return;

        var safeArea = hud.transform.parent as RectTransform;
        if (safeArea == null) return;

        var existing = safeArea.Find("CoinCounterHUD");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        var counter = CreateCoinCounterPanel(safeArea, "CoinCounterHUD", rounded, coinSprite);
        var counterRt = counter.GetComponent<RectTransform>();
        counterRt.anchorMin = new Vector2(0.5f, 1f);
        counterRt.anchorMax = new Vector2(0.5f, 1f);
        counterRt.pivot = new Vector2(0.5f, 1f);
        counterRt.anchoredPosition = new Vector2(0, -210);
        counterRt.sizeDelta = new Vector2(280, 90);

        var ctrl = Object.FindObjectOfType<GameController>();
        if (ctrl != null)
        {
            SerializedObject so = new SerializedObject(ctrl);
            so.FindProperty("hudCoinCounter").objectReferenceValue = counter;
            so.ApplyModifiedProperties();
        }

        var flyContainer = safeArea.Find("CoinFlyContainer");
        if (flyContainer == null)
        {
            var fcGo = new GameObject("CoinFlyContainer");
            fcGo.transform.SetParent(safeArea, false);
            var frt = fcGo.AddComponent<RectTransform>();
            frt.anchorMin = Vector2.zero;
            frt.anchorMax = Vector2.one;
            frt.offsetMin = Vector2.zero;
            frt.offsetMax = Vector2.zero;
            fcGo.AddComponent<CanvasGroup>();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void AddCoinCounterToMainMenu(Sprite rounded, Sprite coinSprite)
    {
        var scene = EditorSceneManager.OpenScene(MainMenuScene, OpenSceneMode.Single);

        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null) return;
        var safeArea = canvas.transform.Find("SafeArea") as RectTransform;
        if (safeArea == null) return;

        var existing = safeArea.Find("CoinCounterMM");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        var counter = CreateCoinCounterPanel(safeArea, "CoinCounterMM", rounded, coinSprite);
        var counterRt = counter.GetComponent<RectTransform>();
        counterRt.anchorMin = new Vector2(0.5f, 1f);
        counterRt.anchorMax = new Vector2(0.5f, 1f);
        counterRt.pivot = new Vector2(0.5f, 1f);
        counterRt.anchoredPosition = new Vector2(0, -210);
        counterRt.sizeDelta = new Vector2(320, 100);

        var mm = Object.FindObjectOfType<MainMenuController>();
        if (mm != null)
        {
            SerializedObject so = new SerializedObject(mm);
            var prop = so.FindProperty("coinCounter");
            if (prop != null) prop.objectReferenceValue = counter;
            so.ApplyModifiedProperties();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void AddCoinCounterToLevelSelect(Sprite rounded, Sprite coinSprite)
    {
        var scene = EditorSceneManager.OpenScene(LevelSelectScene, OpenSceneMode.Single);

        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null) return;
        var safeArea = canvas.transform.Find("SafeArea") as RectTransform;
        if (safeArea == null) return;

        var existing = safeArea.Find("CoinCounterLS");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        var counter = CreateCoinCounterPanel(safeArea, "CoinCounterLS", rounded, coinSprite);
        var counterRt = counter.GetComponent<RectTransform>();
        counterRt.anchorMin = new Vector2(1f, 1f);
        counterRt.anchorMax = new Vector2(1f, 1f);
        counterRt.pivot = new Vector2(1f, 1f);
        counterRt.anchoredPosition = new Vector2(-40, -40);
        counterRt.sizeDelta = new Vector2(260, 90);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static CoinCounter CreateCoinCounterPanel(Transform parent, string name, Sprite rounded, Sprite coinSprite)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        var bg = go.AddComponent<Image>();
        bg.sprite = rounded;
        bg.type = Image.Type.Sliced;
        bg.color = ColorPanel2;
        bg.raycastTarget = false;

        var iconGo = new GameObject("Icon");
        iconGo.transform.SetParent(go.transform, false);
        var iconRt = iconGo.AddComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0f, 0.5f);
        iconRt.anchorMax = new Vector2(0f, 0.5f);
        iconRt.pivot = new Vector2(0f, 0.5f);
        iconRt.sizeDelta = new Vector2(64, 64);
        iconRt.anchoredPosition = new Vector2(15, 0);
        var iconImg = iconGo.AddComponent<Image>();
        iconImg.sprite = coinSprite;
        iconImg.color = Color.white;
        iconImg.raycastTarget = false;

        var txtGo = new GameObject("Value");
        txtGo.transform.SetParent(go.transform, false);
        var txtRt = txtGo.AddComponent<RectTransform>();
        txtRt.anchorMin = new Vector2(0f, 0f);
        txtRt.anchorMax = new Vector2(1f, 1f);
        txtRt.offsetMin = new Vector2(85, 0);
        txtRt.offsetMax = new Vector2(-15, 0);
        var tmp = txtGo.AddComponent<TextMeshProUGUI>();
        tmp.text = "0";
        tmp.fontSize = 50;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = ColorCoin;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.raycastTarget = false;

        var cc = go.AddComponent<CoinCounter>();
        SerializedObject so = new SerializedObject(cc);
        so.FindProperty("valueText").objectReferenceValue = tmp;
        so.FindProperty("coinIcon").objectReferenceValue = iconImg;
        so.ApplyModifiedProperties();

        return cc;
    }

    private static void RebuildWinPopup(Sprite rounded, Sprite coinSprite, GameObject coinPrefab)
    {
        var scene = EditorSceneManager.OpenScene(GameScene, OpenSceneMode.Single);

        WinPopup winPopup = null;
        var allWinPopups = Resources.FindObjectsOfTypeAll<WinPopup>();
        for (int i = 0; i < allWinPopups.Length; i++)
        {
            if (allWinPopups[i].gameObject.scene == scene)
            {
                winPopup = allWinPopups[i];
                break;
            }
        }
        if (winPopup == null)
        {
            Debug.LogWarning("[Iteration06] WinPopup not found in Game scene — Iteration 3 setup might be needed first.");
            return;
        }

        var winPopupGo = winPopup.gameObject;
        var content = winPopupGo.transform.Find("Content") as RectTransform;
        if (content == null)
        {
            Debug.LogWarning("[Iteration06] WinPopup has no 'Content' child.");
            return;
        }

        var existingReward = content.Find("CoinReward");
        if (existingReward != null) Object.DestroyImmediate(existingReward.gameObject);

        var rewardGo = new GameObject("CoinReward");
        rewardGo.transform.SetParent(content, false);
        var rewardRt = rewardGo.AddComponent<RectTransform>();
        rewardRt.anchorMin = new Vector2(0.5f, 0f);
        rewardRt.anchorMax = new Vector2(0.5f, 0f);
        rewardRt.pivot = new Vector2(0.5f, 0f);
        rewardRt.anchoredPosition = new Vector2(0, 580);
        rewardRt.sizeDelta = new Vector2(420, 120);

        var rewardIconGo = new GameObject("Icon");
        rewardIconGo.transform.SetParent(rewardGo.transform, false);
        var ricRt = rewardIconGo.AddComponent<RectTransform>();
        ricRt.anchorMin = new Vector2(0f, 0.5f);
        ricRt.anchorMax = new Vector2(0f, 0.5f);
        ricRt.pivot = new Vector2(0f, 0.5f);
        ricRt.sizeDelta = new Vector2(90, 90);
        ricRt.anchoredPosition = new Vector2(20, 0);
        var ricImg = rewardIconGo.AddComponent<Image>();
        ricImg.sprite = coinSprite;
        ricImg.color = Color.white;
        ricImg.raycastTarget = false;

        var rewardTextGo = new GameObject("Value");
        rewardTextGo.transform.SetParent(rewardGo.transform, false);
        var rtxtRt = rewardTextGo.AddComponent<RectTransform>();
        rtxtRt.anchorMin = new Vector2(0f, 0f);
        rtxtRt.anchorMax = new Vector2(1f, 1f);
        rtxtRt.offsetMin = new Vector2(125, 0);
        rtxtRt.offsetMax = new Vector2(-20, 0);
        var rtxt = rewardTextGo.AddComponent<TextMeshProUGUI>();
        rtxt.text = "+0";
        rtxt.fontSize = 70;
        rtxt.fontStyle = FontStyles.Bold;
        rtxt.color = ColorCoin;
        rtxt.alignment = TextAlignmentOptions.Left;
        rtxt.raycastTarget = false;

        var flySourceGo = new GameObject("FlySource");
        flySourceGo.transform.SetParent(rewardGo.transform, false);
        var flyRt = flySourceGo.AddComponent<RectTransform>();
        flyRt.anchorMin = new Vector2(0f, 0.5f);
        flyRt.anchorMax = new Vector2(0f, 0.5f);
        flyRt.pivot = new Vector2(0.5f, 0.5f);
        flyRt.sizeDelta = new Vector2(10, 10);
        flyRt.anchoredPosition = new Vector2(65, 0);

        var flyFxGo = new GameObject("CoinFlyFx");
        flyFxGo.transform.SetParent(winPopupGo.transform, false);
        var flyFxRt = flyFxGo.AddComponent<RectTransform>();
        flyFxRt.anchorMin = Vector2.zero;
        flyFxRt.anchorMax = Vector2.one;
        flyFxRt.offsetMin = Vector2.zero;
        flyFxRt.offsetMax = Vector2.zero;
        var flyFx = flyFxGo.AddComponent<CoinFlyEffect>();
        flyFx.Setup(coinPrefab);

        SerializedObject so = new SerializedObject(winPopup);
        so.FindProperty("coinRewardText").objectReferenceValue = rtxt;
        so.FindProperty("coinRewardIcon").objectReferenceValue = ricImg;
        so.FindProperty("coinFlySource").objectReferenceValue = flyRt;
        so.FindProperty("coinFlyEffect").objectReferenceValue = flyFx;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}
