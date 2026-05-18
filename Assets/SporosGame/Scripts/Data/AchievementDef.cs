using UnityEngine;

public enum AchievementType
{
    Once,
    Progressive
}

public enum AchievementColor
{
    Cyan,
    Magenta,
    Gold
}

public class AchievementDef
{
    public string Id;
    public string Title;
    public string Description;
    public int TargetValue;
    public int RewardCoins;
    public AchievementType Type;
    public AchievementColor Color;

    public AchievementDef(string id, string title, string desc, int target, int reward, AchievementType type, AchievementColor color)
    {
        Id = id;
        Title = title;
        Description = desc;
        TargetValue = target;
        RewardCoins = reward;
        Type = type;
        Color = color;
    }
}
