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

    public const int TotalLevels = 3;

    public static LevelConfig CreateByIndex(int idx)
    {
        if (idx <= 1) return Level1();
        if (idx == 2) return Level2();
        return Level3();
    }

    private static LevelConfig Level1()
    {
        return new LevelConfig
        {
            LevelIndex = 1,
            Width = 3,
            Height = 3,
            Cells = new CellType[3, 3],
            Spores = new List<SporeStock>
            {
                new SporeStock { Type = SporeType.Basic, Count = 3 }
            }
        };
    }

    private static LevelConfig Level2()
    {
        return new LevelConfig
        {
            LevelIndex = 2,
            Width = 4,
            Height = 3,
            Cells = new CellType[4, 3],
            Spores = new List<SporeStock>
            {
                new SporeStock { Type = SporeType.Basic, Count = 4 }
            }
        };
    }

    private static LevelConfig Level3()
    {
        return new LevelConfig
        {
            LevelIndex = 3,
            Width = 4,
            Height = 4,
            Cells = new CellType[4, 4],
            Spores = new List<SporeStock>
            {
                new SporeStock { Type = SporeType.Basic, Count = 5 }
            }
        };
    }
}

public static class LevelProgress
{
    private const string KeyCurrent = "spo_current_level";

    public static int CurrentLevel
    {
        get { return UnityEngine.PlayerPrefs.GetInt(KeyCurrent, 1); }
        set { UnityEngine.PlayerPrefs.SetInt(KeyCurrent, value); UnityEngine.PlayerPrefs.Save(); }
    }

    public static void AdvanceLevel()
    {
        int n = CurrentLevel + 1;
        if (n > LevelConfig.TotalLevels) n = 1;
        CurrentLevel = n;
    }
}
