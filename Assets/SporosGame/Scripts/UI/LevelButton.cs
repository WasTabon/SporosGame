using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image bgImage;
    [SerializeField] private Image innerImage;
    [SerializeField] private TMP_Text label;
    [SerializeField] private Image lockIcon;
    [SerializeField] private Image[] stars;
    [SerializeField] private Image extraBadge;

    private int levelIndex;
    private bool unlocked;
    private Tween sparkleTween;

    public event Action<int> OnClicked;

    private static readonly Color ColorOuterUnlocked = new Color(0f, 0.898f, 1f, 1f);
    private static readonly Color ColorOuterLocked   = new Color(0.227f, 0.263f, 0.408f, 1f);
    private static readonly Color ColorInnerUnlocked = new Color(0.039f, 0.055f, 0.153f, 1f);
    private static readonly Color ColorInnerLocked   = new Color(0.039f, 0.055f, 0.153f, 0.85f);
    private static readonly Color ColorStarOn        = new Color(0f, 0.898f, 1f, 1f);
    private static readonly Color ColorStarOff       = new Color(0.227f, 0.263f, 0.408f, 0.5f);
    private static readonly Color ColorTextUnlocked  = new Color(0f, 0.898f, 1f, 1f);
    private static readonly Color ColorTextLocked    = new Color(0.6f, 0.6f, 0.7f, 0.5f);
    private static readonly Color ColorExtra         = new Color(1f, 0f, 0.898f, 1f);

    private void Awake()
    {
        button.onClick.AddListener(HandleClick);
    }

    public void Bind(int idx, int starCount, bool isUnlocked, bool isExtra)
    {
        levelIndex = idx;
        unlocked = isUnlocked;

        label.text = idx.ToString();

        bgImage.color = unlocked ? (isExtra ? ColorExtra : ColorOuterUnlocked) : ColorOuterLocked;
        innerImage.color = unlocked ? ColorInnerUnlocked : ColorInnerLocked;
        label.color = unlocked ? (isExtra ? ColorExtra : ColorTextUnlocked) : ColorTextLocked;

        if (lockIcon != null) lockIcon.gameObject.SetActive(!unlocked);
        if (extraBadge != null) extraBadge.gameObject.SetActive(isExtra && unlocked);

        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].color = (i < starCount && unlocked) ? ColorStarOn : ColorStarOff;
            stars[i].gameObject.SetActive(unlocked);
        }

        sparkleTween?.Kill();
        if (unlocked && starCount >= 3)
        {
            float delay = UnityEngine.Random.Range(0f, 3f);
            sparkleTween = transform.DOScale(1.04f, 1.2f).SetDelay(delay).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void HandleClick()
    {
        if (!unlocked)
        {
            transform.DOKill();
            transform.DOShakePosition(0.25f, new Vector3(8f, 0f, 0f), 18, 90, false, true);
            if (SoundManager.Instance != null) SoundManager.Instance.PlaySfx(SfxType.Fail);
            if (HapticManager.Instance != null) HapticManager.Instance.Play(HapticType.Warning);
            return;
        }
        OnClicked?.Invoke(levelIndex);
    }

    private void OnDestroy()
    {
        sparkleTween?.Kill();
    }
}
