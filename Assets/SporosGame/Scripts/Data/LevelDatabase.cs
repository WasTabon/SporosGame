using UnityEngine;

[CreateAssetMenu(menuName = "SporosGame/LevelDatabase")]
public class LevelDatabase : ScriptableObject
{
    public LevelData[] levels;

    public int Count => levels != null ? levels.Length : 0;

    public LevelData Get(int oneBasedIndex)
    {
        if (levels == null || oneBasedIndex < 1 || oneBasedIndex > levels.Length) return null;
        return levels[oneBasedIndex - 1];
    }
}
