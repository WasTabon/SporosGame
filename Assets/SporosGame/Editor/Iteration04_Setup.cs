using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class Iteration04_Setup : EditorWindow
{
    private const string PrefabsFolder = "Assets/SporosGame/Prefabs";
    private const string SpritesFolder = "Assets/SporosGame/GeneratedSprites";
    private const string GameScene = "Assets/SporosGame/Scenes/Game.unity";

    [MenuItem("Tools/SporosGame/Iteration 4/Update Effects + Polish (Iteration 4)")]
    public static void Setup()
    {
        var circle = AssetDatabase.LoadAssetAtPath<Sprite>(SpritesFolder + "/circle.png");
        var square = AssetDatabase.LoadAssetAtPath<Sprite>(SpritesFolder + "/square.png");
        if (circle == null || square == null)
        {
            EditorUtility.DisplayDialog("SporosGame", "Sprites not found — run Iteration 2 setup first.", "OK");
            return;
        }

        var ringSprite = GetOrCreateRingSprite();

        var burstPrefab = CreateParticleBurstPrefab(circle);
        var ringPrefab = CreateRingExpandPrefab(ringSprite);

        UpdateRaySegmentPrefab(square, circle);

        var scene = EditorSceneManager.OpenScene(GameScene, OpenSceneMode.Single);

        var existing = Object.FindObjectOfType<EffectsManager>();
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        var fxGo = new GameObject("EffectsManager");
        var fx = fxGo.AddComponent<EffectsManager>();
        SerializedObject so = new SerializedObject(fx);
        so.FindProperty("particleBurstPrefab").objectReferenceValue = burstPrefab;
        so.FindProperty("ringExpandPrefab").objectReferenceValue = ringPrefab;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("SporosGame", "Iteration 4 setup complete.\nOpen MainMenu and Play.", "OK");
    }

    private static Sprite GetOrCreateRingSprite()
    {
        string path = SpritesFolder + "/ring.png";
        if (File.Exists(path)) File.Delete(path);
        if (File.Exists(path + ".meta")) File.Delete(path + ".meta");

        int size = 256;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var cols = new Color[size * size];
        Vector2 c = new Vector2(size / 2f, size / 2f);
        float outerR = size / 2f - 2f;
        float innerR = outerR - 18f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Vector2.Distance(new Vector2(x, y), c);
            float a;
            if (d > outerR) a = 0f;
            else if (d < innerR) a = 0f;
            else
            {
                float t = (d - innerR) / (outerR - innerR);
                a = Mathf.Sin(t * Mathf.PI);
            }
            cols[y * size + x] = new Color(1f, 1f, 1f, a);
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

    private static GameObject CreateParticleBurstPrefab(Sprite circle)
    {
        var root = new GameObject("ParticleBurst");
        var burst = root.AddComponent<ParticleBurst>();
        burst.Setup(circle, 10, 1.2f, 0.55f, 0.18f);

        string path = PrefabsFolder + "/ParticleBurst.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreateRingExpandPrefab(Sprite ring)
    {
        var root = new GameObject("RingExpand");
        var sr = root.AddComponent<SpriteRenderer>();
        sr.sprite = ring;
        sr.color = Color.white;
        sr.sortingOrder = 7;

        var re = root.AddComponent<RingExpand>();
        re.Setup(sr, 0.45f, 0.2f, 2.2f);

        string path = PrefabsFolder + "/RingExpand.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static void UpdateRaySegmentPrefab(Sprite square, Sprite circle)
    {
        string path = PrefabsFolder + "/RaySegment.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing == null) return;

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(existing);

        var head = instance.transform.Find("Head");
        if (head == null)
        {
            var headGo = new GameObject("Head");
            headGo.transform.SetParent(instance.transform, false);
            var headSr = headGo.AddComponent<SpriteRenderer>();
            headSr.sprite = circle;
            headSr.color = Color.white;
            headSr.sortingOrder = 7;
            headGo.transform.localScale = Vector3.one * 0.32f;

            var seg = instance.GetComponent<RaySegment>();
            SerializedObject so = new SerializedObject(seg);
            so.FindProperty("headRenderer").objectReferenceValue = headSr;
            so.ApplyModifiedProperties();
        }

        PrefabUtility.SaveAsPrefabAsset(instance, path);
        Object.DestroyImmediate(instance);
    }
}
