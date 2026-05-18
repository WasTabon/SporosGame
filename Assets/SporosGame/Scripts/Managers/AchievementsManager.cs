using System;
using System.Collections.Generic;
using UnityEngine;

public static class AchievementsManager
{
    private const string KeyUnlockedPrefix = "spo_ach_";
    private const string KeyProgressPrefix = "spo_ach_progress_";
    private const string KeyTotalCoinsEarned = "spo_ach_total_coins_earned";
    private const string KeyDailyClaimsCount = "spo_ach_daily_claims_count";

    public static event Action<AchievementDef> OnAchievementUnlocked;

    private static List<AchievementDef> all;

    public static List<AchievementDef> All
    {
        get
        {
            if (all == null) BuildList();
            return all;
        }
    }

    private static void BuildList()
    {
        all = new List<AchievementDef>
        {
            new AchievementDef("first_win", "First Steps", "Complete any level", 1, 20, AchievementType.Once, AchievementColor.Cyan),
            new AchievementDef("first_3star", "Perfectionist", "Get 3 stars on any level", 1, 30, AchievementType.Once, AchievementColor.Cyan),
            new AchievementDef("complete_l5", "Pioneer", "Complete level 5", 1, 30, AchievementType.Once, AchievementColor.Cyan),
            new AchievementDef("complete_l10", "Explorer", "Complete level 10", 1, 50, AchievementType.Once, AchievementColor.Cyan),
            new AchievementDef("complete_l20", "Champion", "Complete level 20", 1, 100, AchievementType.Once, AchievementColor.Magenta),
            new AchievementDef("complete_l30", "Legend", "Complete level 30", 1, 200, AchievementType.Once, AchievementColor.Magenta),
            new AchievementDef("no_undo_5", "Decisive", "Complete 5 levels without undo", 5, 50, AchievementType.Progressive, AchievementColor.Cyan),
            new AchievementDef("3star_10", "Star Hunter", "Get 3 stars on 10 levels", 10, 80, AchievementType.Progressive, AchievementColor.Magenta),
            new AchievementDef("coins_100", "Saver", "Earn 100 coins total", 100, 30, AchievementType.Progressive, AchievementColor.Cyan),
            new AchievementDef("coins_500", "Wealthy", "Earn 500 coins total", 500, 100, AchievementType.Progressive, AchievementColor.Magenta),
            new AchievementDef("daily_3", "Loyal", "Claim 3 daily rewards", 3, 50, AchievementType.Progressive, AchievementColor.Cyan),
            new AchievementDef("extra_pack", "Supporter", "Purchase extra levels pack", 1, 50, AchievementType.Once, AchievementColor.Gold),
        };
    }

    public static AchievementDef GetById(string id)
    {
        var list = All;
        for (int i = 0; i < list.Count; i++) if (list[i].Id == id) return list[i];
        return null;
    }

    public static bool IsUnlocked(string id)
    {
        return PlayerPrefs.GetInt(KeyUnlockedPrefix + id, 0) == 1;
    }

    public static int GetProgress(string id)
    {
        return PlayerPrefs.GetInt(KeyProgressPrefix + id, 0);
    }

    private static void SetProgress(string id, int value)
    {
        PlayerPrefs.SetInt(KeyProgressPrefix + id, value);
        PlayerPrefs.Save();
    }

    public static void IncrementProgress(string id, int amount = 1)
    {
        if (IsUnlocked(id)) return;
        var def = GetById(id);
        if (def == null) return;
        int p = GetProgress(id) + amount;
        SetProgress(id, p);
        if (p >= def.TargetValue) Unlock(id);
    }

    public static void SetProgressAtLeast(string id, int value)
    {
        if (IsUnlocked(id)) return;
        var def = GetById(id);
        if (def == null) return;
        int cur = GetProgress(id);
        if (value <= cur) return;
        SetProgress(id, value);
        if (value >= def.TargetValue) Unlock(id);
    }

    public static void Unlock(string id)
    {
        if (IsUnlocked(id)) return;
        var def = GetById(id);
        if (def == null) return;
        PlayerPrefs.SetInt(KeyUnlockedPrefix + id, 1);
        PlayerPrefs.SetInt(KeyProgressPrefix + id, def.TargetValue);
        PlayerPrefs.Save();

        if (def.RewardCoins > 0) CurrencyManager.AddCoinsWithoutTracking(def.RewardCoins);

        OnAchievementUnlocked?.Invoke(def);
    }

    public static int GetTotalCoinsEarned()
    {
        return PlayerPrefs.GetInt(KeyTotalCoinsEarned, 0);
    }

    public static void OnCoinsEarned(int amount)
    {
        if (amount <= 0) return;
        int total = GetTotalCoinsEarned() + amount;
        PlayerPrefs.SetInt(KeyTotalCoinsEarned, total);
        PlayerPrefs.Save();
        SetProgressAtLeast("coins_100", total);
        SetProgressAtLeast("coins_500", total);
    }

    public static void OnLevelCompleted(int levelIdx, int stars, bool usedUndo)
    {
        IncrementProgress("first_win");
        if (stars >= 3)
        {
            IncrementProgress("first_3star");
            IncrementProgress("3star_10");
        }
        if (!usedUndo) IncrementProgress("no_undo_5");

        if (levelIdx == 5) Unlock("complete_l5");
        else if (levelIdx == 10) Unlock("complete_l10");
        else if (levelIdx == 20) Unlock("complete_l20");
        else if (levelIdx == 30) Unlock("complete_l30");
    }

    public static void OnDailyClaim()
    {
        int count = PlayerPrefs.GetInt(KeyDailyClaimsCount, 0) + 1;
        PlayerPrefs.SetInt(KeyDailyClaimsCount, count);
        PlayerPrefs.Save();
        SetProgressAtLeast("daily_3", count);
    }

    public static void OnExtraPackPurchased()
    {
        Unlock("extra_pack");
    }

    public static void ResetForTesting()
    {
        var list = All;
        for (int i = 0; i < list.Count; i++)
        {
            PlayerPrefs.DeleteKey(KeyUnlockedPrefix + list[i].Id);
            PlayerPrefs.DeleteKey(KeyProgressPrefix + list[i].Id);
        }
        PlayerPrefs.DeleteKey(KeyTotalCoinsEarned);
        PlayerPrefs.DeleteKey(KeyDailyClaimsCount);
        PlayerPrefs.Save();
    }
}
