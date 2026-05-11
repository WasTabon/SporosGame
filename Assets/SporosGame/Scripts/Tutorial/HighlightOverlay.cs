using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HighlightOverlay : MonoBehaviour
{
    [SerializeField] private RectTransform overlayRoot;
    [SerializeField] private Image maskTop;
    [SerializeField] private Image maskBottom;
    [SerializeField] private Image maskLeft;
    [SerializeField] private Image maskRight;
    [SerializeField] private Image pulseRing;

    private RectTransform parentRect;
    private Tween pulseTween;

    private static readonly Color DarkColor = new Color(0f, 0f, 0f, 0.7f);
    private static readonly Color PulseColor = new Color(0f, 0.898f, 1f, 0.8f);

    private void EnsureParent()
    {
        if (parentRect != null) return;
        parentRect = transform.parent as RectTransform;
        if (parentRect == null) parentRect = transform as RectTransform;
    }

    private void Awake()
    {
        EnsureParent();
        SetColors();
    }

    private void SetColors()
    {
        if (maskTop != null) maskTop.color = DarkColor;
        if (maskBottom != null) maskBottom.color = DarkColor;
        if (maskLeft != null) maskLeft.color = DarkColor;
        if (maskRight != null) maskRight.color = DarkColor;
        if (pulseRing != null) pulseRing.color = PulseColor;
    }

    public void ShowAroundWorldPos(Vector3 worldPos, Vector2 holeSize, Camera worldCamera, float pulseSize)
    {
        EnsureParent();
        if (parentRect == null) return;
        Vector3 screen = worldCamera != null ? worldCamera.WorldToScreenPoint(worldPos) : worldPos;
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screen, null, out local);
        ShowAroundLocalPos(local, holeSize, pulseSize);
    }

    public void ShowAroundRect(RectTransform target, Vector2 padding, float pulseSize)
    {
        EnsureParent();
        if (parentRect == null) return;
        if (target == null) return;
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);

        Vector2 minLocal, maxLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect,
            RectTransformUtility.WorldToScreenPoint(null, corners[0]), null, out minLocal);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect,
            RectTransformUtility.WorldToScreenPoint(null, corners[2]), null, out maxLocal);

        Vector2 center = (minLocal + maxLocal) * 0.5f;
        Vector2 size = new Vector2(Mathf.Abs(maxLocal.x - minLocal.x), Mathf.Abs(maxLocal.y - minLocal.y)) + padding * 2f;
        ShowAroundLocalPos(center, size, pulseSize);
    }

    private void ShowAroundLocalPos(Vector2 centerLocal, Vector2 holeSize, float pulseSize)
    {
        EnsureParent();
        if (parentRect == null) return;
        gameObject.SetActive(true);
        SetColors();

        Vector2 parentSize = parentRect.rect.size;
        float halfW = holeSize.x * 0.5f;
        float halfH = holeSize.y * 0.5f;

        float holeLeft = centerLocal.x - halfW;
        float holeRight = centerLocal.x + halfW;
        float holeBottom = centerLocal.y - halfH;
        float holeTop = centerLocal.y + halfH;

        float halfPW = parentSize.x * 0.5f;
        float halfPH = parentSize.y * 0.5f;

        if (maskTop != null)
        {
            maskTop.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            maskTop.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            maskTop.rectTransform.pivot = new Vector2(0.5f, 0f);
            maskTop.rectTransform.sizeDelta = new Vector2(parentSize.x, Mathf.Max(0, halfPH - holeTop));
            maskTop.rectTransform.anchoredPosition = new Vector2(0, holeTop);
        }
        if (maskBottom != null)
        {
            maskBottom.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            maskBottom.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            maskBottom.rectTransform.pivot = new Vector2(0.5f, 1f);
            maskBottom.rectTransform.sizeDelta = new Vector2(parentSize.x, Mathf.Max(0, holeBottom + halfPH));
            maskBottom.rectTransform.anchoredPosition = new Vector2(0, holeBottom);
        }
        if (maskLeft != null)
        {
            maskLeft.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            maskLeft.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            maskLeft.rectTransform.pivot = new Vector2(1f, 0.5f);
            maskLeft.rectTransform.sizeDelta = new Vector2(Mathf.Max(0, holeLeft + halfPW), holeSize.y);
            maskLeft.rectTransform.anchoredPosition = new Vector2(holeLeft, centerLocal.y);
        }
        if (maskRight != null)
        {
            maskRight.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            maskRight.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            maskRight.rectTransform.pivot = new Vector2(0f, 0.5f);
            maskRight.rectTransform.sizeDelta = new Vector2(Mathf.Max(0, halfPW - holeRight), holeSize.y);
            maskRight.rectTransform.anchoredPosition = new Vector2(holeRight, centerLocal.y);
        }

        if (pulseRing != null)
        {
            pulseRing.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            pulseRing.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            pulseRing.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            pulseRing.rectTransform.anchoredPosition = centerLocal;
            pulseRing.rectTransform.sizeDelta = new Vector2(pulseSize, pulseSize);
            StartPulse();
        }
    }

    private void StartPulse()
    {
        pulseTween?.Kill();
        if (pulseRing == null) return;
        pulseRing.transform.localScale = Vector3.one * 0.85f;
        pulseTween = pulseRing.transform.DOScale(1.15f, 0.7f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
    }

    public void Hide()
    {
        pulseTween?.Kill();
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        pulseTween?.Kill();
    }
}
