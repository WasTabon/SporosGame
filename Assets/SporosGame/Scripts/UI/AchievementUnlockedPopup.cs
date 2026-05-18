using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementUnlockedPopup : MonoBehaviour
{
    [SerializeField] private RectTransform root;
    [SerializeField] private Image bgImage;
    [SerializeField] private Image trophyIcon;
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private TMP_Text descriptionLabel;
    [SerializeField] private TMP_Text rewardLabel;
    [SerializeField] private GameObject rewardBadge;

    private Queue<AchievementDef> queue = new Queue<AchievementDef>();
    private bool isShowing;
    private Vector2 hiddenPos;
    private Vector2 shownPos;
    private Tween activeTween;

    private static readonly Color ColorCyan = new Color(0f, 0.898f, 1f, 1f);
    private static readonly Color ColorMagenta = new Color(1f, 0f, 0.898f, 1f);
    private static readonly Color ColorGold = new Color(1f, 0.7f, 0.15f, 1f);
    private static readonly Color ColorBg = new Color(0.039f, 0.055f, 0.153f, 0.97f);

    private void Awake()
    {
        if (root != null)
        {
            hiddenPos = new Vector2(0, -300);
            shownPos = new Vector2(0, 120);
            root.anchoredPosition = hiddenPos;
        }
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        AchievementsManager.OnAchievementUnlocked += HandleUnlocked;
    }

    private void OnDisable()
    {
        AchievementsManager.OnAchievementUnlocked -= HandleUnlocked;
    }

    public void Enable()
    {
        gameObject.SetActive(true);
        if (root != null) root.anchoredPosition = hiddenPos;
    }

    private void HandleUnlocked(AchievementDef def)
    {
        queue.Enqueue(def);
        if (!isShowing) ShowNext();
    }

    private void ShowNext()
    {
        if (queue.Count == 0) { isShowing = false; return; }
        isShowing = true;
        var def = queue.Dequeue();
        DisplayDef(def);
    }

    private void DisplayDef(AchievementDef def)
    {
        if (root == null) return;
        gameObject.SetActive(true);
        Color tint = ColorForAch(def.Color);
        if (bgImage != null) bgImage.color = ColorBg;
        if (trophyIcon != null) trophyIcon.color = tint;
        if (titleLabel != null) { titleLabel.text = def.Title; titleLabel.color = tint; }
        if (descriptionLabel != null) descriptionLabel.text = def.Description;
        if (rewardLabel != null) rewardLabel.text = "+" + def.RewardCoins;
        if (rewardBadge != null) rewardBadge.SetActive(def.RewardCoins > 0);

        activeTween?.Kill();
        root.anchoredPosition = hiddenPos;

        var seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(root.DOAnchorPos(shownPos, 0.45f).SetEase(Ease.OutBack));
        if (trophyIcon != null)
        {
            trophyIcon.transform.localScale = Vector3.zero;
            seq.Join(trophyIcon.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack));
        }
        seq.AppendCallback(() =>
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlaySfx(SfxType.Success);
            if (HapticManager.Instance != null) HapticManager.Instance.Play(HapticType.Success);
        });
        seq.AppendInterval(2.4f);
        seq.Append(root.DOAnchorPos(hiddenPos, 0.35f).SetEase(Ease.InQuad));
        seq.AppendCallback(() =>
        {
            if (queue.Count > 0) ShowNext();
            else { isShowing = false; gameObject.SetActive(false); }
        });
        activeTween = seq;
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

    private void OnDestroy()
    {
        activeTween?.Kill();
    }
}
