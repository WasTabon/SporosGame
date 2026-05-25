using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SporeInventoryItem : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image glowImage;
    [SerializeField] private TMP_Text countLabel;
    [SerializeField] private Image[] arrowBasic;
    [SerializeField] private Image[] arrowDiagonal;

    public SporeType Type { get; private set; }
    public int Count { get; private set; }

    public event Action<SporeInventoryItem, Vector3> OnDragBeginEvent;
    public event Action<SporeInventoryItem, Vector3> OnDragMoveEvent;
    public event Action<SporeInventoryItem, Vector3> OnDragEndEvent;

    private Camera worldCamera;
    private static readonly Color CoreBasic    = new Color(1f, 0f, 0.898f, 1f);
    private static readonly Color GlowBasic    = new Color(1f, 0f, 0.898f, 0.55f);
    private static readonly Color CoreDiagonal = new Color(0f, 1f, 0.533f, 1f);
    private static readonly Color GlowDiagonal = new Color(0f, 1f, 0.533f, 0.55f);
    private static readonly Color ColorDisabled = new Color(0.4f, 0.4f, 0.4f, 0.3f);

    public void SetWorldCamera(Camera cam) => worldCamera = cam;

    public void Init(SporeType type, int count)
    {
        Type = type;
        SetCount(count);
        ApplyVisual();
        SetupArrows();
    }

    public void SetCount(int c)
    {
        Count = Mathf.Max(0, c);
        if (countLabel != null) countLabel.text = "x" + Count;
        UpdateInteractable();
    }

    private void UpdateInteractable()
    {
        bool enabled = Count > 0;
        if (iconImage != null) iconImage.color = enabled ? CoreColor() : ColorDisabled;
    }

    private Color CoreColor() => Type == SporeType.Diagonal ? CoreDiagonal : CoreBasic;
    private Color GlowColor() => Type == SporeType.Diagonal ? GlowDiagonal : GlowBasic;

    private void ApplyVisual()
    {
        if (iconImage != null) iconImage.color = CoreColor();
        if (glowImage != null) glowImage.color = GlowColor();
    }

    private void SetupArrows()
    {
        bool isDiag = Type == SporeType.Diagonal;
        Color col = CoreColor();

        if (arrowBasic != null)
        {
            for (int i = 0; i < arrowBasic.Length; i++)
            {
                if (arrowBasic[i] == null) continue;
                arrowBasic[i].gameObject.SetActive(!isDiag);
                if (!isDiag)
                {
                    StartArrowPulse(arrowBasic[i].rectTransform);
                }
            }
        }
        if (arrowDiagonal != null)
        {
            for (int i = 0; i < arrowDiagonal.Length; i++)
            {
                if (arrowDiagonal[i] == null) continue;
                arrowDiagonal[i].gameObject.SetActive(isDiag);
                if (isDiag)
                {
                    StartArrowPulse(arrowDiagonal[i].rectTransform);
                }
            }
        }
    }

    private void StartArrowPulse(RectTransform rt)
    {
        Vector2 basePos = rt.anchoredPosition;
        Vector2 outward = basePos * 1.18f;
        rt.DOKill();
        rt.DOAnchorPos(outward, 0.6f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Count <= 0) return;
        Vector3 wp = ScreenToWorld(eventData.position);
        OnDragBeginEvent?.Invoke(this, wp);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Count <= 0) return;
        Vector3 wp = ScreenToWorld(eventData.position);
        OnDragMoveEvent?.Invoke(this, wp);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Vector3 wp = ScreenToWorld(eventData.position);
        OnDragEndEvent?.Invoke(this, wp);
    }

    private Vector3 ScreenToWorld(Vector2 screen)
    {
        var cam = worldCamera != null ? worldCamera : Camera.main;
        if (cam == null) return Vector3.zero;
        Vector3 s = new Vector3(screen.x, screen.y, -cam.transform.position.z);
        return cam.ScreenToWorldPoint(s);
    }
}
