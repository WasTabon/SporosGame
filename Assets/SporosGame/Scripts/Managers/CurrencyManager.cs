using System;
using UnityEngine;

public static class CurrencyManager
{
    private const string KeyCoins = "spo_coins";

    public static event Action<int, int> OnCoinsChanged;

    public static int Coins
    {
        get { return PlayerPrefs.GetInt(KeyCoins, 0); }
        private set
        {
            int old = Coins;
            PlayerPrefs.SetInt(KeyCoins, Mathf.Max(0, value));
            PlayerPrefs.Save();
            OnCoinsChanged?.Invoke(old, Mathf.Max(0, value));
        }
    }

    public static void AddCoins(int amount)
    {
        if (amount <= 0) return;
        Coins = Coins + amount;
        AchievementsManager.OnCoinsEarned(amount);
    }

    public static void AddCoinsWithoutTracking(int amount)
    {
        if (amount <= 0) return;
        Coins = Coins + amount;
    }

    public static bool SpendCoins(int amount)
    {
        if (amount <= 0) return true;
        int cur = Coins;
        if (cur < amount) return false;
        Coins = cur - amount;
        return true;
    }
}
