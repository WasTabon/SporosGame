using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum DayBoxState
{
    Future,
    Current,
    Claimed
}

public class DayBoxView : MonoBehaviour
{
    [SerializeField] private Image bgImage;
    [SerializeField] private Image innerImage;
    [SerializeField] private TMP_Text dayLabel;
    [SerializeField] private TMP_Text rewardLabel;
    [SerializeField] private Image coinIcon;
    [SerializeField] private Image checkMark;
    [SerializeField] private Image pulseRing;

    private Tween pulseTween;

    private static readonly Color ColorBgFuture = new Color(0.227f, 0.263f, 0.408f, 1f);
    private static readonly Color ColorBgCurrent = new Color(0f, 0.898f, 1f, 1f);
    private static readonly Color ColorBgClaimed = new Color(0f, 1f, 0.533f, 0.7f);
    private static readonly Color ColorBgJackpot = new Color(1f, 0f, 0.898f, 1f);
    private static readonly Color ColorTextNormal = Color.white;
    private static readonly Color ColorTextDim = new Color(0.7f, 0.7f, 0.8f, 0.6f);
    private static readonly Color ColorCoin = new Color(1f, 0.823f, 0.220f, 1f);

    private bool isJackpot;

    public void Init(int day, int reward, bool jackpot)
    {
        isJackpot = jackpot;
        if (dayLabel != null) dayLabel.text = "Day " + day;
        if (rewardLabel != null) rewardLabel.text = reward.ToString();
        if (checkMark != null) checkMark.gameObject.SetActive(false);
        if (pulseRing != null) pulseRing.gameObject.SetActive(false);
        if (coinIcon != null) coinIcon.color = ColorCoin;
    }

    public void SetState(DayBoxState state)
    {
        pulseTween?.Kill();

        switch (state)
        {
            case DayBoxState.Future:
                if (bgImage != null) bgImage.color = isJackpot ? new Color(ColorBgJackpot.r, ColorBgJackpot.g, ColorBgJackpot.b, 0.4f) : ColorBgFuture;
                if (dayLabel != null) dayLabel.color = ColorTextDim;
                if (rewardLabel != null) rewardLabel.color = ColorTextDim;
                if (checkMark != null) checkMark.gameObject.SetActive(false);
                if (pulseRing != null) pulseRing.gameObject.SetActive(false);
                transform.localScale = Vector3.one;
                break;
            case DayBoxState.Current:
                if (bgImage != null) bgImage.color = isJackpot ? ColorBgJackpot : ColorBgCurrent;
                if (dayLabel != null) dayLabel.color = ColorTextNormal;
                if (rewardLabel != null) rewardLabel.color = ColorTextNormal;
                if (checkMark != null) checkMark.gameObject.SetActive(false);
                if (pulseRing != null)
                {
                    pulseRing.gameObject.SetActive(true);
                    pulseRing.transform.localScale = Vector3.one;
                    pulseTween = pulseRing.transform.DOScale(1.15f, 0.7f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
                }
                transform.localScale = Vector3.one * 1.1f;
                break;
            case DayBoxState.Claimed:
                if (bgImage != null) bgImage.color = ColorBgClaimed;
                if (dayLabel != null) dayLabel.color = ColorTextNormal;
                if (rewardLabel != null) rewardLabel.color = ColorTextNormal;
                if (checkMark != null) checkMark.gameObject.SetActive(true);
                if (pulseRing != null) pulseRing.gameObject.SetActive(false);
                transform.localScale = Vector3.one;
                break;
        }
    }

    private void OnDestroy()
    {
        pulseTween?.Kill();
    }
}
