using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Spore : MonoBehaviour
{
    [SerializeField] private SpriteRenderer coreRenderer;
    [SerializeField] private SpriteRenderer glowRenderer;
    [SerializeField] private GameObject rayPrefab;
    [SerializeField] private SpriteRenderer[] arrowBasic;
    [SerializeField] private SpriteRenderer[] arrowDiagonal;

    private SporeType type;
    private Cell originCell;
    private GridSystem grid;
    private bool placed;
    private float placedScale = 0.6f;
    private Tween idleTween;
    private Tween glowRotateTween;
    private List<Tween> arrowPulseTweens = new List<Tween>();

    private static readonly Color CoreBasic    = new Color(1f, 0f, 0.898f, 1f);
    private static readonly Color GlowBasic    = new Color(1f, 0f, 0.898f, 0.55f);
    private static readonly Color CoreDiagonal = new Color(0f, 1f, 0.533f, 1f);
    private static readonly Color GlowDiagonal = new Color(0f, 1f, 0.533f, 0.55f);

    public SporeType Type => type;
    public bool Placed => placed;

    public void Init(SporeType t)
    {
        type = t;
        ApplyVisual();
        StartPulse();
        SetupArrows();
    }

    public void SetPlacedScale(float s) => placedScale = s;

    public Color GetColor()
    {
        return type == SporeType.Diagonal ? CoreDiagonal : CoreBasic;
    }

    private void ApplyVisual()
    {
        if (type == SporeType.Diagonal)
        {
            coreRenderer.color = CoreDiagonal;
            glowRenderer.color = GlowDiagonal;
        }
        else
        {
            coreRenderer.color = CoreBasic;
            glowRenderer.color = GlowBasic;
        }
    }

    private void SetupArrows()
    {
        KillArrowTweens();

        bool isDiag = type == SporeType.Diagonal;
        Color col = GetColor();

        if (arrowBasic != null)
        {
            for (int i = 0; i < arrowBasic.Length; i++)
            {
                if (arrowBasic[i] == null) continue;
                arrowBasic[i].gameObject.SetActive(!isDiag);
                if (!isDiag) { arrowBasic[i].color = col; StartArrowPulse(arrowBasic[i].transform); }
            }
        }
        if (arrowDiagonal != null)
        {
            for (int i = 0; i < arrowDiagonal.Length; i++)
            {
                if (arrowDiagonal[i] == null) continue;
                arrowDiagonal[i].gameObject.SetActive(isDiag);
                if (isDiag) { arrowDiagonal[i].color = col; StartArrowPulse(arrowDiagonal[i].transform); }
            }
        }
    }

    private void StartArrowPulse(Transform t)
    {
        Vector3 basePos = t.localPosition;
        Vector3 outward = basePos * 1.18f;
        var tween = t.DOLocalMove(outward, 0.6f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        arrowPulseTweens.Add(tween);
    }

    private void KillArrowTweens()
    {
        for (int i = 0; i < arrowPulseTweens.Count; i++)
            if (arrowPulseTweens[i] != null) arrowPulseTweens[i].Kill();
        arrowPulseTweens.Clear();
    }

    private void HideArrows()
    {
        KillArrowTweens();
        if (arrowBasic != null)
        {
            for (int i = 0; i < arrowBasic.Length; i++)
            {
                if (arrowBasic[i] == null) continue;
                arrowBasic[i].DOKill();
                arrowBasic[i].DOFade(0f, 0.2f).OnComplete(() => { if (arrowBasic[i] != null) arrowBasic[i].gameObject.SetActive(false); });
            }
        }
        if (arrowDiagonal != null)
        {
            for (int i = 0; i < arrowDiagonal.Length; i++)
            {
                if (arrowDiagonal[i] == null) continue;
                arrowDiagonal[i].DOKill();
                arrowDiagonal[i].DOFade(0f, 0.2f).OnComplete(() => { if (arrowDiagonal[i] != null) arrowDiagonal[i].gameObject.SetActive(false); });
            }
        }
    }

    private void StartPulse()
    {
        glowRenderer.transform.DOKill();
        glowRenderer.transform.localScale = Vector3.one;
        glowRenderer.transform.DOScale(1.25f, 0.8f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }

    public void FollowDrag(Vector3 worldPos)
    {
        transform.position = worldPos;
    }

    public IEnumerator PlaceAndEmit(Cell cell, GridSystem g, System.Action onComplete)
    {
        placed = true;
        originCell = cell;
        grid = g;

        HideArrows();

        transform.DOKill();
        transform.DOMove(cell.WorldPos, 0.18f).SetEase(Ease.OutBack);
        var targetScale = Vector3.one * placedScale;
        transform.localScale = targetScale * 0.7f;
        transform.DOScale(targetScale, 0.22f).SetEase(Ease.OutBack);

        cell.MarkOccupied();

        var sporeColor = GetColor();
        if (EffectsManager.Instance != null)
        {
            EffectsManager.Instance.SpawnRing(cell.WorldPos, sporeColor, grid.CellSize);
            EffectsManager.Instance.SpawnBurst(cell.WorldPos, sporeColor);
        }
        ScreenShake.Shake(0.08f, 0.18f);

        if (SoundManager.Instance != null) SoundManager.Instance.PlaySfx(SfxType.Click);
        if (HapticManager.Instance != null) HapticManager.Instance.Play(HapticType.Medium);

        yield return new WaitForSeconds(0.18f);

        var dirs = SporeDirections.Get(type);
        var rayCoroutines = new List<Coroutine>();
        for (int i = 0; i < dirs.Length; i++)
            rayCoroutines.Add(StartCoroutine(EmitRay(dirs[i])));

        for (int i = 0; i < rayCoroutines.Count; i++)
            yield return rayCoroutines[i];

        StartIdle();
        StartGlowRotation();
        onComplete?.Invoke();
    }

    private IEnumerator EmitRay(Vector2Int dir)
    {
        int x = originCell.GridX;
        int y = originCell.GridY;
        Cell prev = originCell;

        while (true)
        {
            x += dir.x;
            y += dir.y;
            var next = grid.GetCell(x, y);
            if (next == null) break;
            if (next.Type == CellType.Block) break;

            SpawnRaySegment(prev.WorldPos, next.WorldPos);
            yield return new WaitForSeconds(0.06f);

            bool wasLimitedActive = next.Type == CellType.Limited && next.State != CellState.Inactive;

            if (next.State == CellState.Inactive && next.CanBeActivated())
                next.Activate();

            if (next.IsBlockingRay() && next.Type == CellType.Limited && !wasLimitedActive)
                break;
            if (next.IsBlockingRay() && next.Type != CellType.Limited)
                break;

            prev = next;
        }
    }

    private void SpawnRaySegment(Vector3 from, Vector3 to)
    {
        var go = Instantiate(rayPrefab, transform.parent);
        var seg = go.GetComponent<RaySegment>();
        var col = GetColor();
        seg.Play(from, to, col);
    }

    private void StartIdle()
    {
        idleTween?.Kill();
        var s = transform.localScale;
        idleTween = transform.DOScale(s * 1.06f, 1.1f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }

    private void StartGlowRotation()
    {
        glowRotateTween?.Kill();
        if (glowRenderer == null) return;
        glowRenderer.transform.localEulerAngles = Vector3.zero;
        glowRotateTween = glowRenderer.transform.DORotate(new Vector3(0, 0, 360f), 8f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
    }

    public void DestroySelf()
    {
        idleTween?.Kill();
        glowRotateTween?.Kill();
        KillArrowTweens();
        transform.DOKill();
        glowRenderer.transform.DOKill();
        Destroy(gameObject);
    }
}
