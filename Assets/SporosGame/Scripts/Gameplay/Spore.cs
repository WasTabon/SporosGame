using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Spore : MonoBehaviour
{
    [SerializeField] private SpriteRenderer coreRenderer;
    [SerializeField] private SpriteRenderer glowRenderer;
    [SerializeField] private GameObject rayPrefab;

    private SporeType type;
    private Cell originCell;
    private GridSystem grid;
    private bool placed;
    private float placedScale = 0.6f;
    private Tween idleTween;

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
            if (next.State == CellState.Inactive) next.Activate();
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

    public void DestroySelf()
    {
        idleTween?.Kill();
        transform.DOKill();
        glowRenderer.transform.DOKill();
        Destroy(gameObject);
    }
}
