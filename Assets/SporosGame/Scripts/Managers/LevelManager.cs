using UnityEngine;

public static class LevelManager
{
    private static LevelDatabase database;

    private const string KeyCurrent = "spo_current_level";
    private const string KeyStarsPrefix = "spo_level_stars_";
    private const string KeyCoinsAwardedPrefix = "spo_level_coins_awarded_";
    private const string KeyExtraUnlocked = "spo_extra_unlocked";

    public static LevelDatabase Database
    {
        get
        {
            if (database == null) database = Resources.Load<LevelDatabase>("LevelDatabase");
            return database;
        }
    }

    public static int TotalLevels => Database != null ? Database.Count : 0;

    public static LevelData GetLevel(int oneBased)
    {
        return Database != null ? Database.Get(oneBased) : null;
    }

    public static int CurrentLevel
    {
        get { return PlayerPrefs.GetInt(KeyCurrent, 1); }
        set { PlayerPrefs.SetInt(KeyCurrent, value); PlayerPrefs.Save(); }
    }

    public static void AdvanceLevel()
    {
        int total = TotalLevels;
        if (total <= 0) return;
        int n = CurrentLevel + 1;
        if (n > total) n = 1;
        var data = GetLevel(n);
        if (data != null && data.isExtraPack && !IsExtraPackUnlocked()) n = 1;
        CurrentLevel = n;
    }

    public static int GetStars(int levelIdx)
    {
        return PlayerPrefs.GetInt(KeyStarsPrefix + levelIdx, 0);
    }

    public static void SetStars(int levelIdx, int stars)
    {
        int existing = GetStars(levelIdx);
        if (stars > existing)
        {
            PlayerPrefs.SetInt(KeyStarsPrefix + levelIdx, stars);
            PlayerPrefs.Save();
        }
    }

    public static int GetCoinsAwarded(int levelIdx)
    {
        return PlayerPrefs.GetInt(KeyCoinsAwardedPrefix + levelIdx, 0);
    }

    public static int AwardCoinsForLevel(int levelIdx, int totalForCurrentStars)
    {
        int already = GetCoinsAwarded(levelIdx);
        int delta = totalForCurrentStars - already;
        if (delta <= 0) return 0;
        PlayerPrefs.SetInt(KeyCoinsAwardedPrefix + levelIdx, totalForCurrentStars);
        PlayerPrefs.Save();
        CurrencyManager.AddCoins(delta);
        return delta;
    }

    public static bool IsUnlocked(int levelIdx)
    {
        if (levelIdx <= 1) return true;
        var data = GetLevel(levelIdx);
        if (data == null) return false;
        if (data.isExtraPack)
        {
            return IsExtraPackUnlocked();
        }
        return GetStars(levelIdx - 1) > 0;
    }

    public static bool IsExtraPackUnlocked()
    {
        return PlayerPrefs.GetInt(KeyExtraUnlocked, 0) == 1;
    }

    public static void SetExtraPackUnlocked(bool v)
    {
        PlayerPrefs.SetInt(KeyExtraUnlocked, v ? 1 : 0);
        PlayerPrefs.Save();
    }
}
