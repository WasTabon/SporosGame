using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class Iteration07_Setup : EditorWindow
{
    private const string SpritesFolder = "Assets/SporosGame/GeneratedSprites";
    private const string PrefabsFolder = "Assets/SporosGame/Prefabs";
    private const string DataFolder = "Assets/SporosGame/Data";
    private const string ResourcesFolder = "Assets/SporosGame/Resources";
    private const string GameScene = "Assets/SporosGame/Scenes/Game.unity";

    [MenuItem("Tools/SporosGame/Iteration 7/Special Cells + Levels Redesign (Iteration 7)")]
    public static void Setup()
    {
        var hex = AssetDatabase.LoadAssetAtPath<Sprite>(SpritesFolder + "/hex.png");
        if (hex == null)
        {
            EditorUtility.DisplayDialog("SporosGame", "hex sprite missing. Run Iteration 2 first.", "OK");
            return;
        }

        var blockMark = GetOrCreateBlockMarkSprite();
        var limitedOverlay = GetOrCreateLimitedOverlaySprite();

        UpdateCellPrefab(hex, blockMark, limitedOverlay);
        UpdateGameSceneCellPrefab();

        RebuildLevelDatabase();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("SporosGame", "Iteration 7 complete.\n20 levels redesigned with special cells.", "OK");
    }

    private static Sprite GetOrCreateBlockMarkSprite()
    {
        string path = SpritesFolder + "/block_x.png";
        if (File.Exists(path)) File.Delete(path);
        if (File.Exists(path + ".meta")) File.Delete(path + ".meta");

        int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var cols = new Color[size * size];
        for (int i = 0; i < cols.Length; i++) cols[i] = new Color(0, 0, 0, 0);

        int thickness = 8;
        for (int i = 0; i < size; i++)
        {
            for (int t = -thickness; t <= thickness; t++)
            {
                int x1 = i;
                int y1 = i + t;
                if (y1 >= 0 && y1 < size) cols[y1 * size + x1] = Color.white;

                int x2 = i;
                int y2 = (size - 1 - i) + t;
                if (y2 >= 0 && y2 < size) cols[y2 * size + x2] = Color.white;
            }
        }

        Vector2 c = new Vector2(size / 2f, size / 2f);
        float circR = size / 2f - 4f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Vector2.Distance(new Vector2(x, y), c);
            if (d > circR) cols[y * size + x] = new Color(0, 0, 0, 0);
        }

        tex.SetPixels(cols);
        return SaveSprite(tex, path);
    }

    private static Sprite GetOrCreateLimitedOverlaySprite()
    {
        string path = SpritesFolder + "/limited_dashes.png";
        if (File.Exists(path)) File.Delete(path);
        if (File.Exists(path + ".meta")) File.Delete(path + ".meta");

        int width = 256;
        int height = (int)(width / 0.866f);
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        var cols = new Color[width * height];
        for (int i = 0; i < cols.Length; i++) cols[i] = new Color(0, 0, 0, 0);

        float cx = width / 2f;
        float cy = height / 2f;
        float halfW = width / 2f;
        float halfH = height / 2f;
        float innerScale = 0.85f;

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            float px = (x - cx) / halfW;
            float py = (y - cy) / halfH;
            float ax = Mathf.Abs(px);
            float ay = Mathf.Abs(py);
            float h = Mathf.Max(ay + ax * 0.5f, ax);
            if (h > 1f) continue;

            float angle = Mathf.Atan2(py, px);
            float deg = angle * Mathf.Rad2Deg;
            if (deg < 0) deg += 360;

            float dashCycle = (deg % 30f) / 30f;
            if (h > innerScale && dashCycle < 0.6f)
            {
                cols[y * width + x] = new Color(1f, 1f, 1f, 1f);
            }
        }

        tex.SetPixels(cols);
        return SaveSprite(tex, path);
    }

    private static Sprite SaveSprite(Texture2D tex, string path)
    {
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void UpdateCellPrefab(Sprite hex, Sprite blockMark, Sprite limitedOverlay)
    {
        string path = PrefabsFolder + "/Cell.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) return;

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

        EnsureChild(instance, "BlockMark", out var blockGo, () =>
        {
            var go = new GameObject("BlockMark");
            go.transform.SetParent(instance.transform, false);
            go.transform.localScale = Vector3.one * 0.55f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 3;
            return go;
        });
        var blockSr = blockGo.GetComponent<SpriteRenderer>();
        blockSr.sprite = blockMark;
        blockSr.color = new Color(0.35f, 0.4f, 0.52f, 1f);
        blockGo.SetActive(false);

        EnsureChild(instance, "FixedInner", out var fixedGo, () =>
        {
            var go = new GameObject("FixedInner");
            go.transform.SetParent(instance.transform, false);
            go.transform.localScale = Vector3.one * 0.45f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 3;
            return go;
        });
        var fixedSr = fixedGo.GetComponent<SpriteRenderer>();
        fixedSr.sprite = hex;
        fixedSr.color = new Color(1f, 0.7f, 0.15f, 0.85f);
        fixedGo.SetActive(false);

        EnsureChild(instance, "LimitedOverlay", out var limitedGo, () =>
        {
            var go = new GameObject("LimitedOverlay");
            go.transform.SetParent(instance.transform, false);
            go.transform.localScale = Vector3.one;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 4;
            return go;
        });
        var limitedSr = limitedGo.GetComponent<SpriteRenderer>();
        limitedSr.sprite = limitedOverlay;
        limitedSr.color = new Color(1f, 0.53f, 0f, 0.55f);
        limitedGo.SetActive(false);

        var cell = instance.GetComponent<Cell>();
        SerializedObject so = new SerializedObject(cell);
        so.FindProperty("blockMarkRenderer").objectReferenceValue = blockSr;
        so.FindProperty("fixedInnerRenderer").objectReferenceValue = fixedSr;
        so.FindProperty("limitedOverlayRenderer").objectReferenceValue = limitedSr;
        so.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAsset(instance, path);
        Object.DestroyImmediate(instance);
    }

    private static void EnsureChild(GameObject parent, string name, out GameObject child, System.Func<GameObject> create)
    {
        var existing = parent.transform.Find(name);
        if (existing != null) { child = existing.gameObject; return; }
        child = create();
    }

    private static void UpdateGameSceneCellPrefab()
    {
        var scene = EditorSceneManager.OpenScene(GameScene, OpenSceneMode.Single);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void RebuildLevelDatabase()
    {
        var dbPath = ResourcesFolder + "/LevelDatabase.asset";
        var db = AssetDatabase.LoadAssetAtPath<LevelDatabase>(dbPath);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<LevelDatabase>();
            AssetDatabase.CreateAsset(db, dbPath);
        }

        var specs = BuildLevelSpecs();

        var list = new List<LevelData>();
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
            if (s.blocks != null)
                foreach (var b in s.blocks)
                    if (InGrid(b, s.w, s.h)) data.rows[b.y].cells[b.x] = CellType.Block;
            if (s.fixedCells != null)
                foreach (var f in s.fixedCells)
                    if (InGrid(f, s.w, s.h)) data.rows[f.y].cells[f.x] = CellType.Fixed;
            if (s.limited != null)
                foreach (var l in s.limited)
                    if (InGrid(l, s.w, s.h)) data.rows[l.y].cells[l.x] = CellType.Limited;

            var spores = new List<SporeStockEntry>();
            if (s.basicCount > 0) spores.Add(new SporeStockEntry { type = SporeType.Basic, count = s.basicCount });
            if (s.diagonalCount > 0) spores.Add(new SporeStockEntry { type = SporeType.Diagonal, count = s.diagonalCount });
            data.spores = spores.ToArray();

            int totalGiven = s.basicCount + s.diagonalCount;
            int cells = s.w * s.h;
            int playableCells = cells - (s.blocks != null ? s.blocks.Length : 0);
            data.minSporesForThreeStars = Mathf.Max(1, Mathf.CeilToInt(totalGiven * 0.7f));
            data.maxSporesForOneStar = totalGiven;
            data.timeForThreeStars = playableCells * 3f;
            data.timeForOneStar = playableCells * 12f;
            data.coinsReward = 10 + cells * 3;

            EditorUtility.SetDirty(data);
            list.Add(data);
        }

        db.levels = list.ToArray();
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
    }

    private static bool InGrid(Vector2Int p, int w, int h)
    {
        return p.x >= 0 && p.x < w && p.y >= 0 && p.y < h;
    }

    private struct LevelSpec
    {
        public int idx;
        public int w, h;
        public int basicCount, diagonalCount;
        public Vector2Int[] blocks;
        public Vector2Int[] fixedCells;
        public Vector2Int[] limited;
    }

    private static LevelSpec[] BuildLevelSpecs()
    {
        return new LevelSpec[]
        {
            new LevelSpec { idx=1, w=3, h=3, basicCount=2 },
            new LevelSpec { idx=2, w=3, h=3, basicCount=2 },
            new LevelSpec { idx=3, w=4, h=3, basicCount=2 },
            new LevelSpec { idx=4, w=4, h=3, basicCount=2,
                blocks = new[]{ new Vector2Int(1,1) } },
            new LevelSpec { idx=5, w=4, h=4, basicCount=3,
                blocks = new[]{ new Vector2Int(1,1), new Vector2Int(2,2) } },
            new LevelSpec { idx=6, w=4, h=4, basicCount=3,
                blocks = new[]{ new Vector2Int(0,1), new Vector2Int(3,2) } },
            new LevelSpec { idx=7, w=4, h=4, basicCount=3,
                blocks = new[]{ new Vector2Int(1,2), new Vector2Int(2,1) } },
            new LevelSpec { idx=8, w=4, h=4, basicCount=3,
                fixedCells = new[]{ new Vector2Int(0,3), new Vector2Int(3,0) } },
            new LevelSpec { idx=9, w=5, h=4, basicCount=4,
                blocks = new[]{ new Vector2Int(2,1) },
                fixedCells = new[]{ new Vector2Int(0,0), new Vector2Int(4,3) } },
            new LevelSpec { idx=10, w=5, h=4, basicCount=4,
                blocks = new[]{ new Vector2Int(2,1), new Vector2Int(2,2) },
                fixedCells = new[]{ new Vector2Int(0,2), new Vector2Int(4,1) } },
            new LevelSpec { idx=11, w=5, h=5, basicCount=4,
                limited = new[]{ new Vector2Int(2,2) } },
            new LevelSpec { idx=12, w=5, h=5, basicCount=5,
                blocks = new[]{ new Vector2Int(1,1), new Vector2Int(3,3) },
                limited = new[]{ new Vector2Int(2,2) } },
            new LevelSpec { idx=13, w=5, h=5, basicCount=5,
                limited = new[]{ new Vector2Int(1,2), new Vector2Int(3,2) },
                fixedCells = new[]{ new Vector2Int(2,4) } },
            new LevelSpec { idx=14, w=5, h=5, basicCount=5,
                blocks = new[]{ new Vector2Int(0,2), new Vector2Int(4,2) },
                limited = new[]{ new Vector2Int(2,1), new Vector2Int(2,3) },
                fixedCells = new[]{ new Vector2Int(2,2) } },
            new LevelSpec { idx=15, w=5, h=5, basicCount=6,
                blocks = new[]{ new Vector2Int(1,2), new Vector2Int(3,2) },
                fixedCells = new[]{ new Vector2Int(0,4), new Vector2Int(4,0), new Vector2Int(2,2) } },
            new LevelSpec { idx=16, w=6, h=5, basicCount=5,
                blocks = new[]{ new Vector2Int(2,2), new Vector2Int(3,2) },
                limited = new[]{ new Vector2Int(1,2), new Vector2Int(4,2) } },
            new LevelSpec { idx=17, w=5, h=5, basicCount=3, diagonalCount=2,
                blocks = new[]{ new Vector2Int(2,2) } },
            new LevelSpec { idx=18, w=6, h=6, basicCount=4, diagonalCount=3,
                blocks = new[]{ new Vector2Int(2,2), new Vector2Int(3,3) },
                fixedCells = new[]{ new Vector2Int(0,0), new Vector2Int(5,5) } },
            new LevelSpec { idx=19, w=7, h=6, basicCount=5, diagonalCount=3,
                blocks = new[]{ new Vector2Int(3,2), new Vector2Int(3,3) },
                limited = new[]{ new Vector2Int(1,2), new Vector2Int(5,3) },
                fixedCells = new[]{ new Vector2Int(0,5), new Vector2Int(6,0), new Vector2Int(3,5) } },
            new LevelSpec { idx=20, w=7, h=7, basicCount=6, diagonalCount=4,
                blocks = new[]{ new Vector2Int(2,2), new Vector2Int(4,2), new Vector2Int(2,4), new Vector2Int(4,4) },
                limited = new[]{ new Vector2Int(3,1), new Vector2Int(3,5), new Vector2Int(1,3), new Vector2Int(5,3) },
                fixedCells = new[]{ new Vector2Int(3,3), new Vector2Int(0,0), new Vector2Int(6,6) } },
        };
    }
}
