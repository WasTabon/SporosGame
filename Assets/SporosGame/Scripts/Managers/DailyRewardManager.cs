using System;
using UnityEngine;

public static class DailyRewardManager
{
    private const string KeyLastClaimDate = "spo_daily_last_claim_date";
    private const string KeyStreak = "spo_daily_streak";

    public static readonly int[] Rewards = new int[] { 10, 15, 20, 30, 40, 60, 150 };

    public static int CurrentDayInCycle
    {
        get
        {
            int s = PlayerPrefs.GetInt(KeyStreak, 0);
            return Mathf.Clamp(s, 1, Rewards.Length);
        }
    }

    public static int RewardForDay(int dayInCycle)
    {
        int idx = Mathf.Clamp(dayInCycle, 1, Rewards.Length) - 1;
        return Rewards[idx];
    }

    public static bool IsRewardAvailable()
    {
        string last = PlayerPrefs.GetString(KeyLastClaimDate, "");
        string today = DateTime.UtcNow.ToString("yyyyMMdd");
        return last != today;
    }

    public static int GetPendingDay()
    {
        string last = PlayerPrefs.GetString(KeyLastClaimDate, "");
        if (string.IsNullOrEmpty(last)) return 1;

        DateTime lastDate;
        if (!DateTime.TryParseExact(last, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out lastDate))
            return 1;

        DateTime today = DateTime.UtcNow.Date;
        int daysGap = (today - lastDate.Date).Days;

        int prevStreak = PlayerPrefs.GetInt(KeyStreak, 0);

        if (daysGap == 0) return prevStreak;
        if (daysGap == 1)
        {
            int next = prevStreak + 1;
            if (next > Rewards.Length) next = 1;
            return next;
        }
        return 1;
    }

    public static int Claim()
    {
        if (!IsRewardAvailable()) return 0;

        int day = GetPendingDay();
        int reward = RewardForDay(day);

        PlayerPrefs.SetString(KeyLastClaimDate, DateTime.UtcNow.ToString("yyyyMMdd"));
        PlayerPrefs.SetInt(KeyStreak, day);
        PlayerPrefs.Save();

        return reward;
    }

    public static void ResetForTesting()
    {
        PlayerPrefs.DeleteKey(KeyLastClaimDate);
        PlayerPrefs.DeleteKey(KeyStreak);
        PlayerPrefs.Save();
    }
}
