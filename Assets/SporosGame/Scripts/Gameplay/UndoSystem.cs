using System.Collections.Generic;
using UnityEngine;

public class UndoSystem
{
    private LevelStateSnapshot snapshot;
    private GameObject lastPlacedSpore;

    public bool CanUndo => snapshot != null;

    public void SaveSnapshot(GridSystem grid, SporeInventory inventory, GameObject placedSporeGo, int placedX, int placedY, SporeType placedType)
    {
        snapshot = new LevelStateSnapshot
        {
            Width = grid.Width,
            Height = grid.Height,
            CellStates = new CellState[grid.Width, grid.Height],
            CellTypes = new CellType[grid.Width, grid.Height],
            InventoryCounts = new Dictionary<SporeType, int>(),
            PlacedSpores = new List<LevelStateSnapshot.PlacedSporeInfo>()
        };

        for (int y = 0; y < grid.Height; y++)
        for (int x = 0; x < grid.Width; x++)
        {
            var c = grid.GetCell(x, y);
            if (c != null)
            {
                snapshot.CellStates[x, y] = c.State;
                snapshot.CellTypes[x, y] = c.Type;
            }
        }

        var items = inventory.GetItems();
        for (int i = 0; i < items.Count; i++)
            snapshot.InventoryCounts[items[i].Type] = items[i].Count;

        lastPlacedSpore = placedSporeGo;
        snapshot.PlacedSpores.Add(new LevelStateSnapshot.PlacedSporeInfo
        {
            Type = placedType,
            CellX = placedX,
            CellY = placedY,
            SporeGameObject = placedSporeGo
        });
    }

    public LevelStateSnapshot Consume()
    {
        var s = snapshot;
        snapshot = null;
        lastPlacedSpore = null;
        return s;
    }

    public void Clear()
    {
        snapshot = null;
        lastPlacedSpore = null;
    }
}
