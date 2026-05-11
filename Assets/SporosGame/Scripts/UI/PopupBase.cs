using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PopupBase : MonoBehaviour
{
    [SerializeField] protected RectTransform content;
    [SerializeField] protected Image backdrop;
    [SerializeField] protected CanvasGroup canvasGroup;

    private const float OpenDuration = 0.30f;
    private const float CloseDuration = 0.22f;
    private const float BackdropAlpha = 0.65f;

    protected virtual void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }

    public virtual void Show()
    {
        gameObject.SetActive(true);

        if (backdrop != null)
        {
            var c = backdrop.color; c.a = 0f; backdrop.color = c;
            backdrop.DOKill();
            backdrop.DOFade(BackdropAlpha, OpenDuration).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        if (content != null)
        {
            content.DOKill();
            content.localScale = Vector3.zero;
            content.DOScale(1f, OpenDuration).SetEase(Ease.OutBack).SetUpdate(true);
        }

        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, OpenDuration * 0.6f).SetEase(Ease.OutQuad).SetUpdate(true);
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        if (SoundManager.Instance != null) SoundManager.Instance.PlaySfx(SfxType.PopupOpen);
        if (HapticManager.Instance != null) HapticManager.Instance.Play(HapticType.Light);

        OnShown();
    }

    public virtual void Hide()
    {
        if (backdrop != null)
        {
            backdrop.DOKill();
            backdrop.DOFade(0f, CloseDuration).SetEase(Ease.InQuad).SetUpdate(true);
        }

        if (content != null)
        {
            content.DOKill();
            content.DOScale(0f, CloseDuration).SetEase(Ease.InBack).SetUpdate(true)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    OnHidden();
                });
        }
        else
        {
            gameObject.SetActive(false);
            OnHidden();
        }

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        if (SoundManager.Instance != null) SoundManager.Instance.PlaySfx(SfxType.PopupClose);
    }

    protected virtual void OnShown() { }
    protected virtual void OnHidden() { }
}
