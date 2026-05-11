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
    [SerializeField] private SpriteRenderer blockMarkRenderer;
    [SerializeField] private SpriteRenderer fixedInnerRenderer;
    [SerializeField] private SpriteRenderer limitedOverlayRenderer;

    private static readonly Color ColorInactive    = new Color(0.078f, 0.102f, 0.231f, 1f);
    private static readonly Color ColorOutline     = new Color(0.227f, 0.263f, 0.408f, 1f);
    private static readonly Color ColorActive      = new Color(0f, 0.898f, 1f, 1f);
    private static readonly Color ColorGlow        = new Color(0f, 0.898f, 1f, 0.55f);
    private static readonly Color ColorOccupied    = new Color(1f, 0f, 0.898f, 1f);
    private static readonly Color ColorBlockFill   = new Color(0.024f, 0.035f, 0.094f, 1f);
    private static readonly Color ColorBlockMark   = new Color(0.350f, 0.400f, 0.520f, 1f);
    private static readonly Color ColorFixedOutline = new Color(1f, 0.7f, 0.15f, 1f);
    private static readonly Color ColorFixedFill   = new Color(0.08f, 0.07f, 0.10f, 1f);
    private static readonly Color ColorFixedInner  = new Color(1f, 0.7f, 0.15f, 0.85f);
    private static readonly Color ColorFixedActive = new Color(1f, 0.85f, 0.3f, 1f);
    private static readonly Color ColorFixedActiveGlow = new Color(1f, 0.7f, 0.15f, 0.7f);
    private static readonly Color ColorLimitedOutline = new Color(1f, 0.53f, 0.0f, 1f);
    private static readonly Color ColorLimitedOverlay = new Color(1f, 0.53f, 0.0f, 0.55f);
    private static readonly Color ColorLimitedActive  = new Color(1f, 0.65f, 0.2f, 1f);
    private static readonly Color ColorLimitedGlow    = new Color(1f, 0.53f, 0.0f, 0.6f);

    private Vector3 baseScale;
    private Tween pulseTween;
    private Tween fixedIdleTween;

    public bool IsBlockingRay()
    {
        if (Type == CellType.Block) return true;
        if (Type == CellType.Limited && State != CellState.Inactive) return true;
        return false;
    }

    public bool CanBeActivated()
    {
        if (Type == CellType.Block) return false;
        return true;
    }

    public bool CountsForWin()
    {
        return Type != CellType.Block;
    }

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
        if (type == CellType.Fixed) StartFixedIdlePulse();
    }

    public void Activate()
    {
        if (State == CellState.Active) return;
        if (!CanBeActivated()) return;
        State = CellState.Active;

        ApplyActivatedVisual();

        transform.DOKill();
        transform.localScale = baseScale * (Type == CellType.Fixed ? 1.28f : 1.18f);
        transform.DOScale(baseScale, 0.32f).SetEase(Ease.OutBack);

        glowRenderer.transform.DOKill();
        glowRenderer.transform.localScale = Vector3.one * 1.5f;
        glowRenderer.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutQuad);

        if (EffectsManager.Instance != null)
        {
            Color burstColor = Type == CellType.Fixed ? ColorFixedActive
                              : Type == CellType.Limited ? ColorLimitedActive
                              : ColorActive;
            EffectsManager.Instance.SpawnBurst(transform.position, burstColor);
        }

        if (SoundManager.Instance != null) SoundManager.Instance.PlaySfx(SfxType.Pop);

        if (Type == CellType.Fixed)
        {
            ScreenShake.Shake(0.10f, 0.18f);
            if (HapticManager.Instance != null) HapticManager.Instance.Play(HapticType.Heavy);
        }
    }

    public void MarkOccupied()
    {
        if (Type == CellType.Block) return;
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
        fixedIdleTween?.Kill();
        transform.DOKill();
        transform.localScale = baseScale;
        ApplyVisual();
        if (Type == CellType.Fixed) StartFixedIdlePulse();
    }

    public void ForceSetState(CellState s)
    {
        pulseTween?.Kill();
        fixedIdleTween?.Kill();
        transform.DOKill();
        transform.localScale = baseScale;

        State = s;
        if (s == CellState.Inactive)
        {
            ApplyVisual();
            if (Type == CellType.Fixed) StartFixedIdlePulse();
            return;
        }
        if (s == CellState.Active)
        {
            ApplyActivatedVisual();
            return;
        }
        if (s == CellState.Occupied)
        {
            fillRenderer.color = ColorInactive;
            outlineRenderer.color = ColorOccupied;
            glowRenderer.gameObject.SetActive(false);
            if (blockMarkRenderer != null) blockMarkRenderer.gameObject.SetActive(Type == CellType.Block);
            if (fixedInnerRenderer != null) fixedInnerRenderer.gameObject.SetActive(Type == CellType.Fixed);
            if (limitedOverlayRenderer != null) limitedOverlayRenderer.gameObject.SetActive(false);
        }
    }

    private void ApplyVisual()
    {
        switch (Type)
        {
            case CellType.Block:
                fillRenderer.color = ColorBlockFill;
                outlineRenderer.color = ColorOutline;
                glowRenderer.gameObject.SetActive(false);
                if (blockMarkRenderer != null)
                {
                    blockMarkRenderer.gameObject.SetActive(true);
                    blockMarkRenderer.color = ColorBlockMark;
                }
                if (fixedInnerRenderer != null) fixedInnerRenderer.gameObject.SetActive(false);
                if (limitedOverlayRenderer != null) limitedOverlayRenderer.gameObject.SetActive(false);
                break;
            case CellType.Fixed:
                fillRenderer.color = ColorFixedFill;
                outlineRenderer.color = ColorFixedOutline;
                glowRenderer.gameObject.SetActive(false);
                if (blockMarkRenderer != null) blockMarkRenderer.gameObject.SetActive(false);
                if (fixedInnerRenderer != null)
                {
                    fixedInnerRenderer.gameObject.SetActive(true);
                    fixedInnerRenderer.color = ColorFixedInner;
                }
                if (limitedOverlayRenderer != null) limitedOverlayRenderer.gameObject.SetActive(false);
                break;
            case CellType.Limited:
                fillRenderer.color = ColorInactive;
                outlineRenderer.color = ColorLimitedOutline;
                glowRenderer.gameObject.SetActive(false);
                if (blockMarkRenderer != null) blockMarkRenderer.gameObject.SetActive(false);
                if (fixedInnerRenderer != null) fixedInnerRenderer.gameObject.SetActive(false);
                if (limitedOverlayRenderer != null)
                {
                    limitedOverlayRenderer.gameObject.SetActive(true);
                    limitedOverlayRenderer.color = ColorLimitedOverlay;
                }
                break;
            default:
                fillRenderer.color = ColorInactive;
                outlineRenderer.color = ColorOutline;
                glowRenderer.gameObject.SetActive(false);
                glowRenderer.color = ColorGlow;
                glowRenderer.transform.localScale = Vector3.one;
                if (blockMarkRenderer != null) blockMarkRenderer.gameObject.SetActive(false);
                if (fixedInnerRenderer != null) fixedInnerRenderer.gameObject.SetActive(false);
                if (limitedOverlayRenderer != null) limitedOverlayRenderer.gameObject.SetActive(false);
                break;
        }
    }

    private void ApplyActivatedVisual()
    {
        glowRenderer.gameObject.SetActive(true);

        switch (Type)
        {
            case CellType.Fixed:
                fillRenderer.color = ColorFixedActive;
                outlineRenderer.color = ColorFixedActive;
                glowRenderer.color = ColorFixedActiveGlow;
                if (fixedInnerRenderer != null) fixedInnerRenderer.color = ColorFixedFill;
                pulseTween?.Kill();
                pulseTween = fillRenderer.DOFade(0.7f, 0.7f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
                break;
            case CellType.Limited:
                fillRenderer.color = ColorLimitedActive;
                outlineRenderer.color = ColorLimitedActive;
                glowRenderer.color = ColorLimitedGlow;
                if (limitedOverlayRenderer != null) limitedOverlayRenderer.gameObject.SetActive(false);
                pulseTween?.Kill();
                pulseTween = fillRenderer.DOFade(0.85f, 0.9f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
                break;
            default:
                fillRenderer.color = ColorActive;
                outlineRenderer.color = ColorOutline;
                glowRenderer.color = ColorGlow;
                pulseTween?.Kill();
                pulseTween = fillRenderer.DOFade(0.85f, 0.9f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
                break;
        }
    }

    private void StartFixedIdlePulse()
    {
        if (fixedInnerRenderer == null) return;
        fixedIdleTween?.Kill();
        fixedIdleTween = fixedInnerRenderer.transform.DOScale(1.15f, 0.9f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }

    public Vector3 WorldPos => transform.position;

    private void OnDestroy()
    {
        pulseTween?.Kill();
        fixedIdleTween?.Kill();
        transform.DOKill();
    }
}
