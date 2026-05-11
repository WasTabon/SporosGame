using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class Iteration09_Setup : EditorWindow
{
    private const string SpritesFolder = "Assets/SporosGame/GeneratedSprites";
    private const string GameScene = "Assets/SporosGame/Scenes/Game.unity";

    [MenuItem("Tools/SporosGame/Iteration 9/Tutorial Setup (Iteration 9)")]
    public static void Setup()
    {
        var rounded = AssetDatabase.LoadAssetAtPath<Sprite>(SpritesFolder + "/rounded.png");
        var circle = AssetDatabase.LoadAssetAtPath<Sprite>(SpritesFolder + "/circle.png");
        if (rounded == null || circle == null)
        {
            EditorUtility.DisplayDialog("SporosGame", "Sprites missing. Run Iteration 2 first.", "OK");
            return;
        }

        var handSprite = GetOrCreateHandSprite();

        var scene = EditorSceneManager.OpenScene(GameScene, OpenSceneMode.Single);

        var existing = GameObject.Find("TutorialCanvas");
        if (existing != null) Object.DestroyImmediate(existing);

        var canvasGo = new GameObject("TutorialCanvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 1f;
        canvasGo.AddComponent<GraphicRaycaster>().enabled = false;

        var safeRt = new GameObject("Root").AddComponent<RectTransform>();
        safeRt.transform.SetParent(canvasGo.transform, false);
        safeRt.anchorMin = Vector2.zero;
        safeRt.anchorMax = Vector2.one;
        safeRt.offsetMin = Vector2.zero;
        safeRt.offsetMax = Vector2.zero;

        var overlay = CreateOverlay(safeRt.transform, rounded, circle);
        var pointer = CreatePointer(safeRt.transform, handSprite);

        var tmGo = new GameObject("TutorialManager");
        tmGo.transform.SetParent(canvasGo.transform, false);
        var tm = tmGo.AddComponent<TutorialManager>();
        SerializedObject sto = new SerializedObject(tm);
        sto.FindProperty("overlay").objectReferenceValue = overlay;
        sto.FindProperty("pointer").objectReferenceValue = pointer;
        sto.ApplyModifiedProperties();

        var ctrl = Object.FindObjectOfType<GameController>();
        if (ctrl != null)
        {
            SerializedObject so = new SerializedObject(ctrl);
            so.FindProperty("tutorialManager").objectReferenceValue = tm;
            so.ApplyModifiedProperties();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("SporosGame", "Iteration 9 complete.\nTutorial will trigger on L1-L3 first plays.", "OK");
    }

    private static Sprite GetOrCreateHandSprite()
    {
        string path = SpritesFolder + "/hand_pointer.png";
        if (File.Exists(path)) File.Delete(path);
        if (File.Exists(path + ".meta")) File.Delete(path + ".meta");

        int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var cols = new Color[size * size];
        for (int i = 0; i < cols.Length; i++) cols[i] = new Color(0, 0, 0, 0);

        int palmW = 56;
        int palmH = 56;
        int palmCx = size / 2;
        int palmCy = (int)(size * 0.42f);
        DrawRoundedRect(cols, size, palmCx - palmW/2, palmCy - palmH/2, palmW, palmH, 16, Color.white);

        int fingerW = 18;
        int fingerH = 50;
        int fingerCx = palmCx;
        int fingerBottom = palmCy + palmH/2 - 6;
        int fingerTop = fingerBottom + fingerH;
        DrawRoundedRect(cols, size, fingerCx - fingerW/2, fingerBottom, fingerW, fingerH, 8, Color.white);

        int outlineThickness = 4;
        var outlineCols = new Color[cols.Length];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            if (cols[y * size + x].a > 0) continue;
            bool nearWhite = false;
            for (int dy = -outlineThickness; dy <= outlineThickness && !nearWhite; dy++)
            for (int dx = -outlineThickness; dx <= outlineThickness && !nearWhite; dx++)
            {
                int nx = x + dx, ny = y + dy;
                if (nx < 0 || ny < 0 || nx >= size || ny >= size) continue;
                if (cols[ny * size + nx].a > 0) nearWhite = true;
            }
            if (nearWhite) outlineCols[y * size + x] = new Color(0, 0, 0, 1f);
        }
        for (int i = 0; i < cols.Length; i++)
            if (outlineCols[i].a > 0 && cols[i].a == 0) cols[i] = outlineCols[i];

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

    private static void DrawRoundedRect(Color[] cols, int size, int x0, int y0, int w, int h, int radius, Color c)
    {
        for (int y = y0; y < y0 + h && y < size; y++)
        for (int x = x0; x < x0 + w && x < size; x++)
        {
            if (x < 0 || y < 0) continue;
            int relX = x - x0;
            int relY = y - y0;
            bool inside = true;
            if (relX < radius && relY < radius)
            {
                float d = Vector2.Distance(new Vector2(relX, relY), new Vector2(radius, radius));
                inside = d <= radius;
            }
            else if (relX >= w - radius && relY < radius)
            {
                float d = Vector2.Distance(new Vector2(relX, relY), new Vector2(w - radius - 1, radius));
                inside = d <= radius;
            }
            else if (relX < radius && relY >= h - radius)
            {
                float d = Vector2.Distance(new Vector2(relX, relY), new Vector2(radius, h - radius - 1));
                inside = d <= radius;
            }
            else if (relX >= w - radius && relY >= h - radius)
            {
                float d = Vector2.Distance(new Vector2(relX, relY), new Vector2(w - radius - 1, h - radius - 1));
                inside = d <= radius;
            }
            if (inside) cols[y * size + x] = c;
        }
    }

    private static HighlightOverlay CreateOverlay(Transform parent, Sprite rounded, Sprite circle)
    {
        var go = new GameObject("HighlightOverlay");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var top = CreateMaskRect(go.transform, "MaskTop");
        var bottom = CreateMaskRect(go.transform, "MaskBottom");
        var left = CreateMaskRect(go.transform, "MaskLeft");
        var right = CreateMaskRect(go.transform, "MaskRight");

        var pulseGo = new GameObject("PulseRing");
        pulseGo.transform.SetParent(go.transform, false);
        var pRt = pulseGo.AddComponent<RectTransform>();
        var pImg = pulseGo.AddComponent<Image>();
        pImg.sprite = circle;
        pImg.color = new Color(0f, 0.898f, 1f, 0.8f);
        pImg.raycastTarget = false;
        pImg.type = Image.Type.Simple;

        var overlay = go.AddComponent<HighlightOverlay>();
        SerializedObject so = new SerializedObject(overlay);
        so.FindProperty("overlayRoot").objectReferenceValue = rt;
        so.FindProperty("maskTop").objectReferenceValue = top;
        so.FindProperty("maskBottom").objectReferenceValue = bottom;
        so.FindProperty("maskLeft").objectReferenceValue = left;
        so.FindProperty("maskRight").objectReferenceValue = right;
        so.FindProperty("pulseRing").objectReferenceValue = pImg;
        so.ApplyModifiedProperties();

        go.SetActive(false);
        return overlay;
    }

    private static Image CreateMaskRect(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.7f);
        img.raycastTarget = false;
        return img;
    }

    private static HandPointer CreatePointer(Transform parent, Sprite handSprite)
    {
        var go = new GameObject("HandPointer");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var handGo = new GameObject("Hand");
        handGo.transform.SetParent(go.transform, false);
        var handRt = handGo.AddComponent<RectTransform>();
        handRt.anchorMin = new Vector2(0.5f, 0.5f);
        handRt.anchorMax = new Vector2(0.5f, 0.5f);
        handRt.pivot = new Vector2(0.5f, 0.85f);
        handRt.sizeDelta = new Vector2(110, 110);
        var handImg = handGo.AddComponent<Image>();
        handImg.sprite = handSprite;
        handImg.color = Color.white;
        handImg.raycastTarget = false;

        var pointer = go.AddComponent<HandPointer>();
        SerializedObject so = new SerializedObject(pointer);
        so.FindProperty("handRect").objectReferenceValue = handRt;
        so.FindProperty("handImage").objectReferenceValue = handImg;
        so.ApplyModifiedProperties();

        go.SetActive(false);
        return pointer;
    }
}
