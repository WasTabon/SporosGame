using System.Collections.Generic;

public class LevelConfig
{
    public int Width;
    public int Height;
    public CellType[,] Cells;
    public List<SporeStock> Spores;

    public class SporeStock
    {
        public SporeType Type;
        public int Count;
    }

    public static LevelConfig CreateLevel1()
    {
        var cfg = new LevelConfig
        {
            Width = 3,
            Height = 3,
            Cells = new CellType[3, 3],
            Spores = new List<SporeStock>
            {
                new SporeStock { Type = SporeType.Basic, Count = 3 }
            }
        };
        return cfg;
    }
}
