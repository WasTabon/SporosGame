using System.Collections.Generic;

public class LevelConfig
{
    public int Width;
    public int Height;
    public CellType[,] Cells;
    public List<SporeStock> Spores;
    public int LevelIndex;

    public class SporeStock
    {
        public SporeType Type;
        public int Count;
    }

    public static LevelConfig FromData(LevelData data)
    {
        if (data == null) return Fallback();

        var cfg = new LevelConfig
        {
            LevelIndex = data.levelIndex,
            Width = data.width,
            Height = data.height,
            Cells = data.GetCellsArray(),
            Spores = new List<SporeStock>()
        };

        if (data.spores != null)
        {
            for (int i = 0; i < data.spores.Length; i++)
            {
                cfg.Spores.Add(new SporeStock
                {
                    Type = data.spores[i].type,
                    Count = data.spores[i].count
                });
            }
        }

        if (cfg.Spores.Count == 0)
            cfg.Spores.Add(new SporeStock { Type = SporeType.Basic, Count = 3 });

        return cfg;
    }

    public static LevelConfig CreateByIndex(int idx)
    {
        var data = LevelManager.GetLevel(idx);
        return FromData(data);
    }

    private static LevelConfig Fallback()
    {
        return new LevelConfig
        {
            LevelIndex = 1,
            Width = 3,
            Height = 3,
            Cells = new CellType[3, 3],
            Spores = new List<SporeStock> { new SporeStock { Type = SporeType.Basic, Count = 3 } }
        };
    }
}

public static class LevelProgress
{
    public static int CurrentLevel
    {
        get { return LevelManager.CurrentLevel; }
        set { LevelManager.CurrentLevel = value; }
    }

    public static void AdvanceLevel() => LevelManager.AdvanceLevel();
}
