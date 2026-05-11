using DG.Tweening;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public int GridX { get; private set; }
    public int GridY { get; private set; }
    public CellType Type { get; private set; }
    public CellState State { get; private set; }

    [SerializeField] private SpriteRenderer fillRenderer;
    [SerializeField] private SpriteRenderer outlineRenderer;
    [SerializeField] private SpriteRenderer glowRenderer;

    private static readonly Color ColorInactive = new Color(0.078f, 0.102f, 0.231f, 1f);
    private static readonly Color ColorOutline  = new Color(0.227f, 0.263f, 0.408f, 1f);
    private static readonly Color ColorActive   = new Color(0f, 0.898f, 1f, 1f);
    private static readonly Color ColorGlow     = new Color(0f, 0.898f, 1f, 0.55f);
    private static readonly Color ColorOccupied = new Color(1f, 0f, 0.898f, 1f);

    private Vector3 baseScale;
    private Tween pulseTween;

    public void Init(int x, int y, CellType type)
    {
        GridX = x;
        GridY = y;
        Type = type;
        State = CellState.Inactive;

        var s = transform.localScale;
        if (s.x <= 0.0001f || s.y <= 0.0001f) { s = Vector3.one; transform.localScale = s; }
        baseScale = s;

        ApplyVisual();
    }

    public void Activate()
    {
        if (State == CellState.Active) return;
        State = CellState.Active;

        fillRenderer.color = ColorActive;
        glowRenderer.color = ColorGlow;
        glowRenderer.gameObject.SetActive(true);

        transform.DOKill();
        transform.localScale = baseScale * 1.18f;
        transform.DOScale(baseScale, 0.28f).SetEase(Ease.OutBack);

        glowRenderer.transform.DOKill();
        glowRenderer.transform.localScale = Vector3.one * 1.5f;
        glowRenderer.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutQuad);

        pulseTween?.Kill();
        pulseTween = fillRenderer.DOFade(0.85f, 0.9f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);

        if (SoundManager.Instance != null) SoundManager.Instance.PlaySfx(SfxType.Pop);
    }

    public void MarkOccupied()
    {
        State = CellState.Occupied;
        outlineRenderer.color = ColorOccupied;

        transform.DOKill();
        transform.localScale = baseScale * 0.9f;
        transform.DOScale(baseScale, 0.25f).SetEase(Ease.OutBack);
    }

    public void ResetState()
    {
        State = CellState.Inactive;
        pulseTween?.Kill();
        transform.DOKill();
        transform.localScale = baseScale;
        ApplyVisual();
    }

    private void ApplyVisual()
    {
        fillRenderer.color = ColorInactive;
        outlineRenderer.color = ColorOutline;
        glowRenderer.gameObject.SetActive(false);
        glowRenderer.color = ColorGlow;
        glowRenderer.transform.localScale = Vector3.one;
    }

    public Vector3 WorldPos => transform.position;

    private void OnDestroy()
    {
        pulseTween?.Kill();
        transform.DOKill();
    }
}
