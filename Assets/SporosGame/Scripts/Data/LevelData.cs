using System;
using UnityEngine;

[Serializable]
public class CellTypeRow
{
    public CellType[] cells;
}

[Serializable]
public class SporeStockEntry
{
    public SporeType type;
    public int count;
}

[CreateAssetMenu(menuName = "SporosGame/LevelData")]
public class LevelData : ScriptableObject
{
    public int levelIndex = 1;
    public int width = 3;
    public int height = 3;
    public CellTypeRow[] rows;
    public SporeStockEntry[] spores;
    public bool isExtraPack;

    public CellType GetCellType(int x, int y)
    {
        if (rows == null || y < 0 || y >= rows.Length) return CellType.Normal;
        var row = rows[y];
        if (row == null || row.cells == null || x < 0 || x >= row.cells.Length) return CellType.Normal;
        return row.cells[x];
    }

    public CellType[,] GetCellsArray()
    {
        var arr = new CellType[width, height];
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
            arr[x, y] = GetCellType(x, y);
        return arr;
    }
}
