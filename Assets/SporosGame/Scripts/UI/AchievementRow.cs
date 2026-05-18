using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementRow : MonoBehaviour
{
    [SerializeField] private Image bgImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private TMP_Text descLabel;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text progressLabel;
    [SerializeField] private GameObject unlockedBadge;
    [SerializeField] private TMP_Text rewardLabel;

    private static readonly Color ColorBgLocked = new Color(0.078f, 0.102f, 0.231f, 0.85f);
    private static readonly Color ColorBgUnlocked = new Color(0.078f, 0.102f, 0.231f, 1f);
    private static readonly Color ColorTextLocked = new Color(0.7f, 0.7f, 0.8f, 0.7f);
    private static readonly Color ColorTextNormal = Color.white;
    private static readonly Color ColorCyan = new Color(0f, 0.898f, 1f, 1f);
    private static readonly Color ColorMagenta = new Color(1f, 0f, 0.898f, 1f);
    private static readonly Color ColorGold = new Color(1f, 0.7f, 0.15f, 1f);
    private static readonly Color ColorDim = new Color(0.35f, 0.4f, 0.52f, 1f);

    public void Bind(AchievementDef def)
    {
        bool unlocked = AchievementsManager.IsUnlocked(def.Id);
        int progress = Mathf.Min(AchievementsManager.GetProgress(def.Id), def.TargetValue);

        Color accent = ColorForAch(def.Color);
        Color tint = unlocked ? accent : ColorDim;

        if (bgImage != null) bgImage.color = unlocked ? ColorBgUnlocked : ColorBgLocked;
        if (iconImage != null) iconImage.color = tint;
        if (titleLabel != null) { titleLabel.text = def.Title; titleLabel.color = unlocked ? accent : ColorTextLocked; }
        if (descLabel != null) { descLabel.text = def.Description; descLabel.color = unlocked ? ColorTextNormal : ColorTextLocked; }

        if (rewardLabel != null) rewardLabel.text = "+" + def.RewardCoins;

        if (unlockedBadge != null) unlockedBadge.SetActive(unlocked);

        if (def.Type == AchievementType.Progressive && !unlocked)
        {
            if (progressBar != null)
            {
                progressBar.gameObject.SetActive(true);
                progressBar.minValue = 0;
                progressBar.maxValue = def.TargetValue;
                progressBar.value = progress;
            }
            if (progressLabel != null)
            {
                progressLabel.gameObject.SetActive(true);
                progressLabel.text = progress + " / " + def.TargetValue;
            }
        }
        else
        {
            if (progressBar != null) progressBar.gameObject.SetActive(false);
            if (progressLabel != null) progressLabel.gameObject.SetActive(false);
        }
    }

    private Color ColorForAch(AchievementColor c)
    {
        switch (c)
        {
            case AchievementColor.Magenta: return ColorMagenta;
            case AchievementColor.Gold: return ColorGold;
            default: return ColorCyan;
        }
    }
}
