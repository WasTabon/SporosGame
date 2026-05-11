using UnityEngine;

public static class StarCalculator
{
    public static int Calculate(LevelData data, int sporesUsed, float timeSeconds)
    {
        if (data == null) return 3;

        int sporeStars = ScoreByThreshold(sporesUsed, data.minSporesForThreeStars, data.maxSporesForOneStar, lessIsBetter: true);
        int timeStars = ScoreByThreshold(timeSeconds, data.timeForThreeStars, data.timeForOneStar, lessIsBetter: true);

        int avg = Mathf.RoundToInt((sporeStars + timeStars) * 0.5f);
        return Mathf.Clamp(avg, 1, 3);
    }

    private static int ScoreByThreshold(float value, float bestThreshold, float worstThreshold, bool lessIsBetter)
    {
        if (lessIsBetter)
        {
            if (value <= bestThreshold) return 3;
            if (value >= worstThreshold) return 1;
            return 2;
        }
        else
        {
            if (value >= bestThreshold) return 3;
            if (value <= worstThreshold) return 1;
            return 2;
        }
    }

    public static int CoinsForStars(LevelData data, int stars)
    {
        if (data == null) return 0;
        if (stars <= 0) return 0;
        if (stars >= 3) return data.coinsReward;
        if (stars == 2) return Mathf.RoundToInt(data.coinsReward * 0.6f);
        return Mathf.RoundToInt(data.coinsReward * 0.3f);
    }
}
