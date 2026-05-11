using System.Collections.Generic;
using UnityEngine;

public class LevelStateSnapshot
{
    public CellState[,] CellStates;
    public CellType[,] CellTypes;
    public int Width;
    public int Height;
    public Dictionary<SporeType, int> InventoryCounts;
    public List<PlacedSporeInfo> PlacedSpores;

    public class PlacedSporeInfo
    {
        public SporeType Type;
        public int CellX;
        public int CellY;
        public GameObject SporeGameObject;
    }
}
