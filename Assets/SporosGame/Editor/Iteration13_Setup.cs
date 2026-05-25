using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Iteration13_Setup : EditorWindow
{
    private const string SpritesFolder = "Assets/SporosGame/GeneratedSprites";
    private const string PrefabsFolder = "Assets/SporosGame/Prefabs";
    private const string DataFolder = "Assets/SporosGame/Data";
    private const string ResourcesFolder = "Assets/SporosGame/Resources";

    [MenuItem("Tools/SporosGame/Iteration 13/Bigger Levels + Arrow Indicators (Iteration 13)")]
    public static void Setup()
    {
        var triangle = GetOrCreateTriangleSprite();

        UpdateSporePrefab(triangle);
        UpdateInventoryItemPrefab(triangle);
        RebuildLevels();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("SporosGame",
            "Iteration 13 complete.\n\nNEXT STEP:\n" +
            "Tools -> SporosGame -> Iteration 7 -> Auto-Solve and Balance Levels\n" +
            "(чтобы пересчитать spore counts для новых layouts)",
            "OK");
    }

    private static Sprite GetOrCreateTriangleSprite()
    {
        string path = SpritesFolder + "/triangle.png";
        if (File.Exists(path)) File.Delete(path);
        if (File.Exists(path + ".meta")) File.Delete(path + ".meta");

        int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var cols = new Color[size * size];
        for (int i = 0; i < cols.Length; i++) cols[i] = new Color(0, 0, 0, 0);

        for (int y = 0; y < size; y++)
        {
            float t = (float)y / (size - 1);
            float halfW = (1f - t) * (size / 2f - 2);
            int cx = size / 2;
            int x0 = cx - Mathf.RoundToInt(halfW);
            int x1 = cx + Mathf.RoundToInt(halfW);
            for (int x = x0; x <= x1; x++)
            {
                if (x < 0 || x >= size) continue;
                cols[y * size + x] = Color.white;
            }
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

    private static void UpdateSporePrefab(Sprite triangle)
    {
        string path = PrefabsFolder + "/Spore.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) return;

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

        for (int i = instance.transform.childCount - 1; i >= 0; i--)
        {
            var c = instance.transform.GetChild(i);
            if (c.name.StartsWith("Arrow_")) Object.DestroyImmediate(c.gameObject);
        }

        float distance = 0.42f;
        var basicDirs = new[]
        {
            new Vector2(0, 1), new Vector2(0, -1),
            new Vector2(1, 0), new Vector2(-1, 0)
        };
        var diagDirs = new[]
        {
            new Vector2(0.707f, 0.707f), new Vector2(-0.707f, -0.707f),
            new Vector2(0.707f, -0.707f), new Vector2(-0.707f, 0.707f)
        };

        var arrowBasic = new SpriteRenderer[basicDirs.Length];
        for (int i = 0; i < basicDirs.Length; i++)
            arrowBasic[i] = CreateArrowChild(instance.transform, "Arrow_B_" + i, triangle, basicDirs[i], distance);

        var arrowDiag = new SpriteRenderer[diagDirs.Length];
        for (int i = 0; i < diagDirs.Length; i++)
            arrowDiag[i] = CreateArrowChild(instance.transform, "Arrow_D_" + i, triangle, diagDirs[i], distance);

        var spore = instance.GetComponent<Spore>();
        SerializedObject so = new SerializedObject(spore);
        var pBasic = so.FindProperty("arrowBasic");
        pBasic.arraySize = arrowBasic.Length;
        for (int i = 0; i < arrowBasic.Length; i++)
            pBasic.GetArrayElementAtIndex(i).objectReferenceValue = arrowBasic[i];
        var pDiag = so.FindProperty("arrowDiagonal");
        pDiag.arraySize = arrowDiag.Length;
        for (int i = 0; i < arrowDiag.Length; i++)
            pDiag.GetArrayElementAtIndex(i).objectReferenceValue = arrowDiag[i];
        so.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAsset(instance, path);
        Object.DestroyImmediate(instance);
    }

    private static SpriteRenderer CreateArrowChild(Transform parent, string name, Sprite triangle, Vector2 dir, float distance)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(dir.x * distance, dir.y * distance, 0);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        go.transform.localEulerAngles = new Vector3(0, 0, angle);
        go.transform.localScale = Vector3.one * 0.22f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = triangle;
        sr.color = Color.white;
        sr.sortingOrder = 3;
        return sr;
    }

    private static void UpdateInventoryItemPrefab(Sprite triangle)
    {
        string path = PrefabsFolder + "/SporeInventoryItem.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) return;

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

        for (int i = instance.transform.childCount - 1; i >= 0; i--)
        {
            var c = instance.transform.GetChild(i);
            if (c.name.StartsWith("Arrow_")) Object.DestroyImmediate(c.gameObject);
        }

        float distance = 55f;
        var basicDirs = new[]
        {
            new Vector2(0, 1), new Vector2(0, -1),
            new Vector2(1, 0), new Vector2(-1, 0)
        };
        var diagDirs = new[]
        {
            new Vector2(0.707f, 0.707f), new Vector2(-0.707f, -0.707f),
            new Vector2(0.707f, -0.707f), new Vector2(-0.707f, 0.707f)
        };

        var arrowBasic = new Image[basicDirs.Length];
        for (int i = 0; i < basicDirs.Length; i++)
            arrowBasic[i] = CreateArrowUI(instance.transform, "Arrow_B_" + i, triangle, basicDirs[i], distance);

        var arrowDiag = new Image[diagDirs.Length];
        for (int i = 0; i < diagDirs.Length; i++)
            arrowDiag[i] = CreateArrowUI(instance.transform, "Arrow_D_" + i, triangle, diagDirs[i], distance);

        var inv = instance.GetComponent<SporeInventoryItem>();
        SerializedObject so = new SerializedObject(inv);
        var pBasic = so.FindProperty("arrowBasic");
        pBasic.arraySize = arrowBasic.Length;
        for (int i = 0; i < arrowBasic.Length; i++)
            pBasic.GetArrayElementAtIndex(i).objectReferenceValue = arrowBasic[i];
        var pDiag = so.FindProperty("arrowDiagonal");
        pDiag.arraySize = arrowDiag.Length;
        for (int i = 0; i < arrowDiag.Length; i++)
            pDiag.GetArrayElementAtIndex(i).objectReferenceValue = arrowDiag[i];
        so.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAsset(instance, path);
        Object.DestroyImmediate(instance);
    }

    private static Image CreateArrowUI(Transform parent, string name, Sprite triangle, Vector2 dir, float distance)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(dir.x * distance, dir.y * distance);
        rt.sizeDelta = new Vector2(28, 28);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        rt.localEulerAngles = new Vector3(0, 0, angle);

        var img = go.AddComponent<Image>();
        img.sprite = triangle;
        img.color = Color.white;
        img.raycastTarget = false;
        return img;
    }

    private static void RebuildLevels()
    {
        var dbPath = ResourcesFolder + "/LevelDatabase.asset";
        var db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(dbPath);
        if (db == null) return;

        var existingList = new List<LevelData>();
        if (db.levels != null) existingList.AddRange(db.levels);

        var specs = BuildLevelSpecs();
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
            data.isExtraPack = false;

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
            int total = s.basicCount + s.diagonalCount;
            data.minSporesForThreeStars = Mathf.Max(1, Mathf.CeilToInt(total * 0.7f));
            data.maxSporesForOneStar = total;
            data.timeForThreeStars = playable * 3f;
            data.timeForOneStar = playable * 12f;
            data.coinsReward = 10 + cells * 3;

            EditorUtility.SetDirty(data);

            bool found = false;
            for (int j = 0; j < existingList.Count; j++)
                if (existingList[j] != null && existingList[j].levelIndex == data.levelIndex)
                {
                    existingList[j] = data;
                    found = true;
                    break;
                }
            if (!found) existingList.Add(data);
        }

        existingList.Sort((a, b) => a.levelIndex.CompareTo(b.levelIndex));
        db.levels = existingList.ToArray();
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
    }

    private static bool InGrid(Vector2Int p, int w, int h) { return p.x >= 0 && p.x < w && p.y >= 0 && p.y < h; }

    private struct LevelSpec
    {
        public int idx, w, h, basicCount, diagonalCount;
        public Vector2Int[] blocks, fixedCells, limited;
    }

    private static LevelSpec[] BuildLevelSpecs()
    {
        return new LevelSpec[]
        {
            new LevelSpec { idx=1, w=4, h=4, basicCount=3 },
            new LevelSpec { idx=2, w=4, h=4, basicCount=3 },
            new LevelSpec { idx=3, w=5, h=4, basicCount=3,
                blocks = new[]{ new Vector2Int(2,1) } },
            new LevelSpec { idx=4, w=5, h=4, basicCount=4,
                blocks = new[]{ new Vector2Int(1,1), new Vector2Int(3,2) } },
            new LevelSpec { idx=5, w=5, h=5, basicCount=4,
                blocks = new[]{ new Vector2Int(2,2) } },
            new LevelSpec { idx=6, w=5, h=5, basicCount=4,
                blocks = new[]{ new Vector2Int(1,1), new Vector2Int(3,3) } },
            new LevelSpec { idx=7, w=6, h=5, basicCount=5,
                blocks = new[]{ new Vector2Int(2,2), new Vector2Int(3,2) } },
            new LevelSpec { idx=8, w=6, h=5, basicCount=5,
                blocks = new[]{ new Vector2Int(1,2), new Vector2Int(4,2) },
                fixedCells = new[]{ new Vector2Int(0,0), new Vector2Int(5,4) } },
            new LevelSpec { idx=9, w=6, h=6, basicCount=5,
                blocks = new[]{ new Vector2Int(2,1), new Vector2Int(3,4) },
                fixedCells = new[]{ new Vector2Int(0,4), new Vector2Int(5,1) } },
            new LevelSpec { idx=10, w=6, h=6, basicCount=6,
                blocks = new[]{ new Vector2Int(2,2), new Vector2Int(3,3) },
                fixedCells = new[]{ new Vector2Int(0,0), new Vector2Int(5,5), new Vector2Int(0,5) } },
            new LevelSpec { idx=11, w=6, h=6, basicCount=5,
                limited = new[]{ new Vector2Int(2,2), new Vector2Int(3,3) },
                blocks = new[]{ new Vector2Int(1,4) } },
            new LevelSpec { idx=12, w=6, h=6, basicCount=6,
                blocks = new[]{ new Vector2Int(1,1), new Vector2Int(4,4) },
                limited = new[]{ new Vector2Int(2,3), new Vector2Int(3,2) },
                fixedCells = new[]{ new Vector2Int(0,5) } },
            new LevelSpec { idx=13, w=7, h=6, basicCount=6,
                blocks = new[]{ new Vector2Int(3,2), new Vector2Int(3,3) },
                limited = new[]{ new Vector2Int(1,3), new Vector2Int(5,2) },
                fixedCells = new[]{ new Vector2Int(0,0), new Vector2Int(6,5) } },
            new LevelSpec { idx=14, w=7, h=6, basicCount=6,
                blocks = new[]{ new Vector2Int(2,2), new Vector2Int(4,3) },
                limited = new[]{ new Vector2Int(3,1), new Vector2Int(3,4) },
                fixedCells = new[]{ new Vector2Int(0,3), new Vector2Int(6,2) } },
            new LevelSpec { idx=15, w=7, h=7, basicCount=7,
                blocks = new[]{ new Vector2Int(2,2), new Vector2Int(4,4) },
                limited = new[]{ new Vector2Int(3,3) },
                fixedCells = new[]{ new Vector2Int(0,0), new Vector2Int(6,6), new Vector2Int(3,0) } },
            new LevelSpec { idx=16, w=7, h=7, basicCount=7,
                blocks = new[]{ new Vector2Int(1,3), new Vector2Int(5,3), new Vector2Int(3,1), new Vector2Int(3,5) },
                limited = new[]{ new Vector2Int(2,2), new Vector2Int(4,4) },
                fixedCells = new[]{ new Vector2Int(3,3) } },
            new LevelSpec { idx=17, w=7, h=7, basicCount=5, diagonalCount=3,
                blocks = new[]{ new Vector2Int(2,3), new Vector2Int(4,3) },
                fixedCells = new[]{ new Vector2Int(3,0), new Vector2Int(3,6) } },
            new LevelSpec { idx=18, w=8, h=7, basicCount=5, diagonalCount=4,
                blocks = new[]{ new Vector2Int(2,2), new Vector2Int(5,4), new Vector2Int(3,3) },
                limited = new[]{ new Vector2Int(4,2), new Vector2Int(3,4) },
                fixedCells = new[]{ new Vector2Int(0,0), new Vector2Int(7,6) } },
            new LevelSpec { idx=19, w=8, h=8, basicCount=6, diagonalCount=4,
                blocks = new[]{ new Vector2Int(3,3), new Vector2Int(4,4), new Vector2Int(3,4), new Vector2Int(4,3) },
                limited = new[]{ new Vector2Int(2,3), new Vector2Int(5,4) },
                fixedCells = new[]{ new Vector2Int(0,7), new Vector2Int(7,0), new Vector2Int(3,7) } },
            new LevelSpec { idx=20, w=8, h=8, basicCount=7, diagonalCount=5,
                blocks = new[]{ new Vector2Int(2,2), new Vector2Int(5,2), new Vector2Int(2,5), new Vector2Int(5,5), new Vector2Int(4,4) },
                limited = new[]{ new Vector2Int(3,4), new Vector2Int(4,3) },
                fixedCells = new[]{ new Vector2Int(3,3), new Vector2Int(0,0), new Vector2Int(7,7) } },
        };
    }
}
