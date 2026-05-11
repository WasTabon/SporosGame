using UnityEngine;

public enum SporeType
{
    Basic,
    Diagonal
}

public static class SporeDirections
{
    public static readonly Vector2Int[] Basic = new[]
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };

    public static readonly Vector2Int[] Diagonal = new[]
    {
        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1)
    };

    public static Vector2Int[] Get(SporeType type)
    {
        switch (type)
        {
            case SporeType.Diagonal: return Diagonal;
            default: return Basic;
        }
    }
}
