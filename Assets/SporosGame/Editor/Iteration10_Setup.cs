using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class Iteration10_Setup : EditorWindow
{
    private const string PrefabsFolder = "Assets/SporosGame/Prefabs";
    private const string SpritesFolder = "Assets/SporosGame/GeneratedSprites";

    [MenuItem("Tools/SporosGame/Iteration 10/Final Polish (Iteration 10)")]
    public static void Setup()
    {
        var circle = AssetDatabase.LoadAssetAtPath<Sprite>(SpritesFolder + "/circle.png");
        if (circle == null)
        {
            EditorUtility.DisplayDialog("SporosGame", "Sprites missing. Run Iteration 2 first.", "OK");
            return;
        }

        UpdateParticleBurstPrefab(circle);

        EditorUtility.DisplayDialog("SporosGame",
            "Iteration 10 complete.\n\n" +
            "AmbientMusic will start automatically (GameBootstrap spawns it on first scene load).\n\n" +
            "All polish features active:\n" +
            "- Ambient drone music\n" +
            "- Cell idle breathing\n" +
            "- Spore glow rotation\n" +
            "- Micro-shake on undo\n" +
            "- 3-star LevelButton sparkle\n" +
            "- More particles on burst (12)",
            "OK");
    }

    private static void UpdateParticleBurstPrefab(Sprite circle)
    {
        string path = PrefabsFolder + "/ParticleBurst.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) return;

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        var burst = instance.GetComponent<ParticleBurst>();
        if (burst != null) burst.Setup(circle, 12, 1.3f, 0.65f, 0.18f);
        PrefabUtility.SaveAsPrefabAsset(instance, path);
        Object.DestroyImmediate(instance);
    }
}
