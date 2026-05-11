using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SporeInventoryItem : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private Image glow;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Image disabledOverlay;

    private SporeType type;
    private int count;
    private bool dragging;

    public event Action<SporeInventoryItem, Vector3> OnDragBeginEvent;
    public event Action<SporeInventoryItem, Vector3> OnDragMoveEvent;
    public event Action<SporeInventoryItem, Vector3> OnDragEndEvent;

    public SporeType Type => type;
    public int Count => count;

    public void Init(SporeType t, int c)
    {
        type = t;
        count = c;
        UpdateVisual();
    }

    public void SetCount(int c)
    {
        int prev = count;
        count = c;
        UpdateVisual();
        if (count < prev && count >= 0)
        {
            icon.transform.DOKill();
            icon.transform.localScale = Vector3.one * 0.8f;
            icon.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
        }
    }

    private void UpdateVisual()
    {
        countText.text = "x" + count;
        bool empty = count <= 0;
        disabledOverlay.gameObject.SetActive(empty);

        Color core = type == SporeType.Diagonal ? new Color(0f, 1f, 0.533f, 1f) : new Color(1f, 0f, 0.898f, 1f);
        Color glowCol = core; glowCol.a = 0.45f;
        icon.color = core;
        glow.color = glowCol;

        glow.transform.DOKill();
        glow.transform.localScale = Vector3.one;
        glow.transform.DOScale(1.15f, 0.9f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (count <= 0) return;
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySfx(SfxType.Hover);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (count <= 0) return;
        dragging = true;
        Vector3 wp = ScreenToWorld(eventData.position);
        OnDragBeginEvent?.Invoke(this, wp);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging) return;
        Vector3 wp = ScreenToWorld(eventData.position);
        OnDragMoveEvent?.Invoke(this, wp);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragging) return;
        dragging = false;
        Vector3 wp = ScreenToWorld(eventData.position);
        OnDragEndEvent?.Invoke(this, wp);
    }

    private Vector3 ScreenToWorld(Vector2 screen)
    {
        var cam = Camera.main;
        if (cam == null) return Vector3.zero;
        Vector3 sp = new Vector3(screen.x, screen.y, -cam.transform.position.z);
        return cam.ScreenToWorldPoint(sp);
    }

    private void OnDisable()
    {
        glow.transform.DOKill();
        icon.transform.DOKill();
    }
}
