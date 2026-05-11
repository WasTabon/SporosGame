using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Iteration07_LevelSolver : EditorWindow
{
    private const string DataFolder = "Assets/SporosGame/Data";
    private const int MaxSporeCount = 30;

    private static readonly Vector2Int[] DirsBasic = new[]
    {
        new Vector2Int(1, 0), new Vector2Int(-1, 0),
        new Vector2Int(0, 1), new Vector2Int(0, -1)
    };

    private static readonly Vector2Int[] DirsDiagonal = new[]
    {
        new Vector2Int(1, 1), new Vector2Int(1, -1),
        new Vector2Int(-1, 1), new Vector2Int(-1, -1)
    };

    private class Placement
    {
        public int x, y;
        public SporeType type;
    }

    [MenuItem("Tools/SporosGame/Iteration 7/Auto-Solve and Balance Levels")]
    public static void RunSolver()
    {
        var guids = AssetDatabase.FindAssets("t:LevelData", new[] { DataFolder });
        var dataList = new List<LevelData>();
        for (int i = 0; i < guids.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[i]);
            var d = AssetDatabase.LoadAssetAtPath<LevelData>(path);
            if (d != null) dataList.Add(d);
        }
        dataList.Sort((a, b) => a.levelIndex.CompareTo(b.levelIndex));

        var report = new System.Text.StringBuilder();
        report.AppendLine("=== Auto-balance report ===");

        for (int i = 0; i < dataList.Count; i++)
        {
            var data = dataList[i];
            float progress = (float)i / dataList.Count;
            if (EditorUtility.DisplayCancelableProgressBar("Solving levels", "Level " + data.levelIndex, progress))
            {
                EditorUtility.ClearProgressBar();
                return;
            }

            int basicGiven = 0, diagGiven = 0;
            if (data.spores != null)
            {
                for (int s = 0; s < data.spores.Length; s++)
                {
                    if (data.spores[s].type == SporeType.Basic) basicGiven = data.spores[s].count;
                    if (data.spores[s].type == SporeType.Diagonal) diagGiven = data.spores[s].count;
                }
            }
            bool diagonalEnabled = diagGiven > 0 || data.levelIndex >= 17;

            int minBasic, minDiag;
            bool solved = FindMinSolution(data, diagonalEnabled, out minBasic, out minDiag);

            if (!solved)
            {
                int cellsTotal = data.width * data.height;
                int blockCnt = CountType(data, CellType.Block);
                int playableCnt = cellsTotal - blockCnt;
                int maxRay = Mathf.Max(data.width, data.height);
                int heuristicMin = Mathf.Max(2, playableCnt / Mathf.Max(1, maxRay) + 1);
                if (diagonalEnabled)
                {
                    minBasic = heuristicMin - heuristicMin / 2;
                    minDiag = heuristicMin / 2;
                }
                else
                {
                    minBasic = heuristicMin;
                    minDiag = 0;
                }
                report.AppendLine("L" + data.levelIndex + ": solver gave up, using heuristic minSolve=" + heuristicMin);
            }

            int newBasic = minBasic;
            int newDiag = minDiag;
            if (newDiag > 0) newDiag += 1; else newBasic += 1;
            if (newBasic < 1 && newDiag < 1) newBasic = 1;

            var sporeList = new List<SporeStockEntry>();
            if (newBasic > 0) sporeList.Add(new SporeStockEntry { type = SporeType.Basic, count = newBasic });
            if (newDiag > 0) sporeList.Add(new SporeStockEntry { type = SporeType.Diagonal, count = newDiag });
            data.spores = sporeList.ToArray();

            int totalMin = minBasic + minDiag;
            int totalGiven = newBasic + newDiag;
            data.minSporesForThreeStars = Mathf.Max(1, totalMin);
            data.maxSporesForOneStar = totalGiven;

            int cells = data.width * data.height;
            int blockCount = CountType(data, CellType.Block);
            int playable = cells - blockCount;
            data.timeForThreeStars = playable * 3f;
            data.timeForOneStar = playable * 12f;
            data.coinsReward = 10 + cells * 3;

            EditorUtility.SetDirty(data);

            report.AppendLine("L" + data.levelIndex + ": minSolve=" + totalMin + " (basic " + minBasic + ", diag " + minDiag + "), given=" + totalGiven);
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();

        Debug.Log(report.ToString());
        EditorUtility.DisplayDialog("SporosGame", "Solver done.\nSee Console for full report.", "OK");
    }

    private static int CountType(LevelData data, CellType t)
    {
        int n = 0;
        if (data.rows == null) return 0;
        for (int y = 0; y < data.height; y++)
        {
            if (data.rows[y] == null || data.rows[y].cells == null) continue;
            for (int x = 0; x < data.width; x++)
                if (data.rows[y].cells[x] == t) n++;
        }
        return n;
    }

    private static bool FindMinSolution(LevelData data, bool diagonalEnabled, out int minBasic, out int minDiag)
    {
        minBasic = 0;
        minDiag = 0;

        var validCells = new List<Vector2Int>();
        for (int y = 0; y < data.height; y++)
        for (int x = 0; x < data.width; x++)
            if (data.GetCellType(x, y) != CellType.Block)
                validCells.Add(new Vector2Int(x, y));

        if (validCells.Count == 0) return true;

        for (int total = 1; total <= MaxSporeCount; total++)
        {
            int basicMax = diagonalEnabled ? total : total;
            int basicMin = 0;
            for (int basic = basicMin; basic <= basicMax; basic++)
            {
                int diag = total - basic;
                if (!diagonalEnabled && diag > 0) continue;

                if (TrySolve(data, validCells, basic, diag))
                {
                    minBasic = basic;
                    minDiag = diag;
                    return true;
                }
            }
        }
        return false;
    }

    private static bool TrySolve(LevelData data, List<Vector2Int> validCells, int basicCount, int diagCount)
    {
        if (basicCount + diagCount == 0)
            return CheckImmediateWin(data);

        long combos = Combinations(validCells.Count, basicCount + diagCount);
        if (combos > 5_000_000) return false;

        var placements = new Placement[basicCount + diagCount];
        var indices = new int[basicCount + diagCount];
        for (int i = 0; i < indices.Length; i++) indices[i] = i;

        return EnumerateCombos(validCells, indices, 0, placements, basicCount, diagCount, data);
    }

    private static bool CheckImmediateWin(LevelData data)
    {
        for (int y = 0; y < data.height; y++)
        for (int x = 0; x < data.width; x++)
            if (data.GetCellType(x, y) != CellType.Block) return false;
        return true;
    }

    private static long Combinations(int n, int k)
    {
        if (k < 0 || k > n) return 0;
        if (k == 0 || k == n) return 1;
        if (k > n - k) k = n - k;
        long c = 1;
        for (int i = 0; i < k; i++)
        {
            c = c * (n - i) / (i + 1);
            if (c > 10_000_000) return c;
        }
        return c;
    }

    private static bool EnumerateCombos(List<Vector2Int> cells, int[] idx, int depth, Placement[] placements, int basicLeft, int diagLeft, LevelData data)
    {
        if (depth == idx.Length)
        {
            return TryAssignTypes(cells, idx, placements, basicLeft, diagLeft, data);
        }

        int start = depth == 0 ? 0 : idx[depth - 1] + 1;
        int slotsLeft = idx.Length - depth - 1;
        int maxStart = cells.Count - 1 - slotsLeft;

        for (int i = start; i <= maxStart; i++)
        {
            idx[depth] = i;
            if (EnumerateCombos(cells, idx, depth + 1, placements, basicLeft, diagLeft, data))
                return true;
        }
        return false;
    }

    private static bool TryAssignTypes(List<Vector2Int> cells, int[] idx, Placement[] placements, int basicCount, int diagCount, LevelData data)
    {
        int total = basicCount + diagCount;
        var typeAssign = new SporeType[total];

        return AssignRecursive(cells, idx, placements, typeAssign, 0, basicCount, diagCount, data);
    }

    private static bool AssignRecursive(List<Vector2Int> cells, int[] idx, Placement[] placements, SporeType[] typeAssign, int depth, int basicLeft, int diagLeft, LevelData data)
    {
        if (depth == typeAssign.Length)
        {
            for (int i = 0; i < typeAssign.Length; i++)
            {
                placements[i] = new Placement { x = cells[idx[i]].x, y = cells[idx[i]].y, type = typeAssign[i] };
            }
            return Simulate(data, placements);
        }

        if (basicLeft > 0)
        {
            typeAssign[depth] = SporeType.Basic;
            if (AssignRecursive(cells, idx, placements, typeAssign, depth + 1, basicLeft - 1, diagLeft, data)) return true;
        }
        if (diagLeft > 0)
        {
            typeAssign[depth] = SporeType.Diagonal;
            if (AssignRecursive(cells, idx, placements, typeAssign, depth + 1, basicLeft, diagLeft - 1, data)) return true;
        }
        return false;
    }

    private static bool Simulate(LevelData data, Placement[] placements)
    {
        int w = data.width;
        int h = data.height;
        var state = new CellState[w, h];
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
            state[x, y] = CellState.Inactive;

        for (int i = 0; i < placements.Length; i++)
        {
            var p = placements[i];
            if (data.GetCellType(p.x, p.y) == CellType.Block) return false;
            for (int j = 0; j < i; j++)
                if (placements[j].x == p.x && placements[j].y == p.y) return false;
        }

        for (int i = 0; i < placements.Length; i++)
        {
            var p = placements[i];
            state[p.x, p.y] = CellState.Occupied;
        }

        for (int i = 0; i < placements.Length; i++)
        {
            var p = placements[i];
            var dirs = p.type == SporeType.Diagonal ? DirsDiagonal : DirsBasic;
            for (int d = 0; d < dirs.Length; d++)
                CastRay(data, state, p.x, p.y, dirs[d]);
        }

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            var t = data.GetCellType(x, y);
            if (t == CellType.Block) continue;
            if (state[x, y] == CellState.Inactive) return false;
        }
        return true;
    }

    private static void CastRay(LevelData data, CellState[,] state, int sx, int sy, Vector2Int dir)
    {
        int x = sx;
        int y = sy;
        while (true)
        {
            x += dir.x;
            y += dir.y;
            if (x < 0 || y < 0 || x >= data.width || y >= data.height) break;
            var t = data.GetCellType(x, y);
            if (t == CellType.Block) break;

            bool wasLimitedActive = t == CellType.Limited && state[x, y] != CellState.Inactive;

            if (state[x, y] == CellState.Inactive) state[x, y] = CellState.Active;

            if (t == CellType.Limited && wasLimitedActive) break;
        }
    }
}
