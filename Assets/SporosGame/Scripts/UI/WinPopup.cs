using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class WinPopup : PopupBase
{
    [SerializeField] private Button nextButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button menuButton;
    [SerializeField] private Image[] starIcons;

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

    public void ShowWithStars(int stars)
    {
        for (int i = 0; i < starIcons.Length; i++)
        {
            starIcons[i].color = StarOff;
            starIcons[i].transform.localScale = Vector3.zero;
        }
        Show();
        AnimateStars(stars);
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
