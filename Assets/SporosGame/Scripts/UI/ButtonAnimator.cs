using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ButtonAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerExitHandler
{
    [SerializeField] private float pressedScale = 0.92f;
    [SerializeField] private float duration = 0.10f;
    [SerializeField] private bool playSound = true;
    [SerializeField] private bool playHaptic = true;
    [SerializeField] private SfxType sfxType = SfxType.Click;

    private Vector3 originalScale;
    private Tween scaleTween;
    private bool pressed;
    private Button button;

    private void Awake()
    {
        originalScale = transform.localScale;
        button = GetComponent<Button>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;
        pressed = true;
        scaleTween?.Kill();
        scaleTween = transform.DOScale(originalScale * pressedScale, duration).SetEase(Ease.OutQuad).SetUpdate(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!pressed) return;
        pressed = false;
        scaleTween?.Kill();
        scaleTween = transform.DOScale(originalScale, duration).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!pressed) return;
        pressed = false;
        scaleTween?.Kill();
        scaleTween = transform.DOScale(originalScale, duration).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;
        if (playSound && SoundManager.Instance != null) SoundManager.Instance.PlaySfx(sfxType);
        if (playHaptic && HapticManager.Instance != null) HapticManager.Instance.Play(HapticType.Light);
    }

    private void OnDisable()
    {
        scaleTween?.Kill();
        transform.localScale = originalScale;
        pressed = false;
    }
}
