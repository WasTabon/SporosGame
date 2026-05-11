using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private float cellSize = 1.5f;
    [SerializeField] private float rowOffsetFactor = 0.5f;

    private Cell[,] cells;
    private int width;
    private int height;

    public int Width => width;
    public int Height => height;

    public void Build(int w, int h, CellType[,] types)
    {
        Clear();
        width = w;
        height = h;
        cells = new Cell[w, h];

        float spacingX = cellSize;
        float spacingY = cellSize * 0.866f;

        float totalW = (w - 1) * spacingX + rowOffsetFactor * spacingX;
        float totalH = (h - 1) * spacingY;
        Vector3 origin = new Vector3(-totalW * 0.5f, -totalH * 0.5f, 0f);

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            var go = Instantiate(cellPrefab, transform);
            go.name = "Cell_" + x + "_" + y;
            go.transform.localScale = Vector3.one * cellSize;
            float offX = (y % 2 == 1) ? rowOffsetFactor * spacingX : 0f;
            Vector3 pos = origin + new Vector3(x * spacingX + offX, y * spacingY, 0f);
            go.transform.localPosition = pos;
            var cell = go.GetComponent<Cell>();
            cell.Init(x, y, types[x, y]);
            cells[x, y] = cell;
        }
    }

    public void Build(int w, int h)
    {
        var t = new CellType[w, h];
        Build(w, h, t);
    }

    public Cell GetCell(int x, int y)
    {
        if (x < 0 || y < 0 || x >= width || y >= height) return null;
        return cells[x, y];
    }

    public List<Cell> GetAllCells()
    {
        var list = new List<Cell>();
        if (cells == null) return list;
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
            if (cells[x, y] != null) list.Add(cells[x, y]);
        return list;
    }

    public bool AreAllActivated()
    {
        var all = GetAllCells();
        for (int i = 0; i < all.Count; i++)
        {
            var c = all[i];
            if (!c.CountsForWin()) continue;
            if (c.State == CellState.Inactive) return false;
        }
        return true;
    }

    public Cell FindClosestCell(Vector3 worldPos, float maxDistance)
    {
        Cell best = null;
        float bestDist = maxDistance;
        var all = GetAllCells();
        for (int i = 0; i < all.Count; i++)
        {
            float d = Vector3.Distance(all[i].WorldPos, worldPos);
            if (d < bestDist)
            {
                bestDist = d;
                best = all[i];
            }
        }
        return best;
    }

    public Bounds GetBounds()
    {
        var all = GetAllCells();
        if (all.Count == 0) return new Bounds(Vector3.zero, Vector3.zero);
        var b = new Bounds(all[0].WorldPos, Vector3.zero);
        for (int i = 1; i < all.Count; i++) b.Encapsulate(all[i].WorldPos);
        b.Expand(cellSize);
        return b;
    }

    public void Clear()
    {
        if (cells == null) return;
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
            if (cells[x, y] != null) Destroy(cells[x, y].gameObject);
        cells = null;
    }

    public float CellSize => cellSize;
}
