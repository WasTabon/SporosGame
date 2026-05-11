using DG.Tweening;
using UnityEngine;

public class RaySegment : MonoBehaviour
{
    [SerializeField] private SpriteRenderer lineRenderer;
    [SerializeField] private SpriteRenderer glowRenderer;

    private const float Width = 0.18f;
    private const float GlowWidth = 0.45f;

    public void Play(Vector3 from, Vector3 to, Color color)
    {
        Vector3 mid = (from + to) * 0.5f;
        Vector3 diff = to - from;
        float dist = diff.magnitude;
        float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

        transform.position = mid;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        lineRenderer.color = color;
        var glowCol = color; glowCol.a = 0.5f;
        glowRenderer.color = glowCol;

        lineRenderer.transform.localScale = new Vector3(0.01f, Width, 1f);
        glowRenderer.transform.localScale = new Vector3(0.01f, GlowWidth, 1f);

        var seq = DOTween.Sequence();
        seq.Append(lineRenderer.transform.DOScaleX(dist, 0.12f).SetEase(Ease.OutQuad));
        seq.Join(glowRenderer.transform.DOScaleX(dist, 0.12f).SetEase(Ease.OutQuad));
        seq.AppendInterval(0.6f);
        seq.Append(lineRenderer.DOFade(0f, 0.5f));
        seq.Join(glowRenderer.DOFade(0f, 0.5f));
        seq.OnComplete(() => Destroy(gameObject));
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}
