using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HandPointer : MonoBehaviour
{
    [SerializeField] private RectTransform handRect;
    [SerializeField] private Image handImage;

    private Sequence motionSeq;
    private RectTransform parentRect;

    private void EnsureParent()
    {
        if (parentRect != null) return;
        parentRect = transform.parent as RectTransform;
        if (parentRect == null) parentRect = transform as RectTransform;
    }

    private void Awake()
    {
        EnsureParent();
    }

    public void ShowAtPoint(Vector3 worldPos, Camera worldCamera)
    {
        EnsureParent();
        if (parentRect == null) return;
        Vector3 screen = worldCamera != null ? worldCamera.WorldToScreenPoint(worldPos) : worldPos;
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screen, null, out local);
        ShowStatic(local);
    }

    public void ShowAtRect(RectTransform target)
    {
        EnsureParent();
        if (parentRect == null) return;
        if (target == null) return;
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);
        Vector3 worldCenter = (corners[0] + corners[2]) * 0.5f;
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect,
            RectTransformUtility.WorldToScreenPoint(null, worldCenter), null, out local);
        ShowStatic(local);
    }

    private void ShowStatic(Vector2 localPos)
    {
        gameObject.SetActive(true);
        motionSeq?.Kill();
        if (handRect != null)
        {
            handRect.anchoredPosition = localPos;
            handRect.localScale = Vector3.one;
        }
        if (handImage != null) { var c = handImage.color; c.a = 1f; handImage.color = c; }

        if (handRect == null) return;
        motionSeq = DOTween.Sequence().SetUpdate(true).SetLoops(-1);
        motionSeq.Append(handRect.DOScale(0.85f, 0.4f).SetEase(Ease.InOutSine));
        motionSeq.Append(handRect.DOScale(1f, 0.4f).SetEase(Ease.InOutSine));
    }

    public void ShowDragMotion(Vector2 fromLocal, Vector2 toLocal)
    {
        gameObject.SetActive(true);
        motionSeq?.Kill();
        if (handRect == null) return;
        handRect.anchoredPosition = fromLocal;
        handRect.localScale = Vector3.one;
        if (handImage != null) { var c = handImage.color; c.a = 1f; handImage.color = c; }

        motionSeq = DOTween.Sequence().SetUpdate(true).SetLoops(-1);
        motionSeq.AppendCallback(() =>
        {
            handRect.anchoredPosition = fromLocal;
            handRect.localScale = Vector3.one;
            if (handImage != null) { var c = handImage.color; c.a = 1f; handImage.color = c; }
        });
        motionSeq.Append(handRect.DOScale(0.82f, 0.25f).SetEase(Ease.OutQuad));
        motionSeq.AppendInterval(0.1f);
        motionSeq.Append(handRect.DOAnchorPos(toLocal, 0.9f).SetEase(Ease.InOutQuad));
        motionSeq.AppendInterval(0.1f);
        motionSeq.Append(handRect.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
        if (handImage != null) motionSeq.Join(handImage.DOFade(0f, 0.3f));
        motionSeq.AppendInterval(0.3f);
    }

    public void ShowDragMotionWorld(Vector3 fromWorld, Vector3 toWorld, Camera worldCamera)
    {
        EnsureParent();
        if (parentRect == null) return;
        Vector3 fromScreen = worldCamera != null ? worldCamera.WorldToScreenPoint(fromWorld) : fromWorld;
        Vector3 toScreen = worldCamera != null ? worldCamera.WorldToScreenPoint(toWorld) : toWorld;
        Vector2 fromLocal, toLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, fromScreen, null, out fromLocal);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, toScreen, null, out toLocal);
        ShowDragMotion(fromLocal, toLocal);
    }

    public void ShowDragMotionRectToWorld(RectTransform fromRect, Vector3 toWorld, Camera worldCamera)
    {
        EnsureParent();
        if (parentRect == null) return;
        if (fromRect == null) return;
        Vector3[] corners = new Vector3[4];
        fromRect.GetWorldCorners(corners);
        Vector3 fromWorld = (corners[0] + corners[2]) * 0.5f;
        Vector3 fromScreen = RectTransformUtility.WorldToScreenPoint(null, fromWorld);
        Vector3 toScreen = worldCamera != null ? worldCamera.WorldToScreenPoint(toWorld) : toWorld;
        Vector2 fromLocal, toLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, fromScreen, null, out fromLocal);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, toScreen, null, out toLocal);
        ShowDragMotion(fromLocal, toLocal);
    }

    public void Hide()
    {
        motionSeq?.Kill();
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        motionSeq?.Kill();
    }
}
