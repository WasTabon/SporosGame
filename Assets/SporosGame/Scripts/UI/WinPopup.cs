using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinPopup : PopupBase
{
    [SerializeField] private Button nextButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button menuButton;
    [SerializeField] private Image[] starIcons;
    [SerializeField] private TMP_Text coinRewardText;
    [SerializeField] private Image coinRewardIcon;
    [SerializeField] private RectTransform coinFlySource;
    [SerializeField] private CoinFlyEffect coinFlyEffect;

    public event Action OnNext;
    public event Action OnRetry;
    public event Action OnMenu;

    private static readonly Color StarOn  = new Color(0f, 0.898f, 1f, 1f);
    private static readonly Color StarOff = new Color(0.227f, 0.263f, 0.408f, 0.6f);

    protected override void Awake()
    {
        base.Awake();
        nextButton.onClick.AddListener(() => { Hide(); OnNext?.Invoke(); });
        retryButton.onClick.AddListener(() => { Hide(); OnRetry?.Invoke(); });
        menuButton.onClick.AddListener(() => { Hide(); OnMenu?.Invoke(); });
    }

    public void ShowWithResults(int stars, int coinsEarned, RectTransform coinTarget)
    {
        for (int i = 0; i < starIcons.Length; i++)
        {
            starIcons[i].color = StarOff;
            starIcons[i].transform.localScale = Vector3.zero;
        }
        if (coinRewardText != null)
        {
            coinRewardText.text = "+0";
            coinRewardText.transform.localScale = Vector3.one;
        }
        if (coinRewardIcon != null) coinRewardIcon.transform.localScale = Vector3.one;

        Show();
        AnimateStars(stars);

        if (coinsEarned <= 0) return;

        bool canFly = coinTarget != null && coinFlyEffect != null && coinFlySource != null;
        float starDelay = 0.35f + starIcons.Length * 0.18f + 0.1f;

        DOVirtual.DelayedCall(starDelay, () =>
        {
            if (coinRewardText != null)
            {
                coinRewardText.text = "+" + coinsEarned;
                coinRewardText.transform.DOKill();
                coinRewardText.transform.localScale = Vector3.zero;
                coinRewardText.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
            }

            if (canFly)
            {
                coinFlyEffect.Fly(Mathf.Min(coinsEarned, 8), coinFlySource, coinTarget, null, null);
                DOVirtual.DelayedCall(0.4f, () => CurrencyManager.AddCoins(coinsEarned)).SetUpdate(true);
            }
            else
            {
                CurrencyManager.AddCoins(coinsEarned);
            }
        }).SetUpdate(true);
    }

    private void AnimateStars(int stars)
    {
        for (int i = 0; i < starIcons.Length; i++)
        {
            int idx = i;
            float delay = 0.35f + idx * 0.18f;
            DOVirtual.DelayedCall(delay, () =>
            {
                if (idx < stars) starIcons[idx].color = StarOn;
                starIcons[idx].transform.localScale = Vector3.zero;
                starIcons[idx].transform.DOScale(1f, 0.32f).SetEase(Ease.OutBack).SetUpdate(true);
                if (idx < stars && SoundManager.Instance != null) SoundManager.Instance.PlaySfx(SfxType.Pop);
            }).SetUpdate(true);
        }
    }
}
