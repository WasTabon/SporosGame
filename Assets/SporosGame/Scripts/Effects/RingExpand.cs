using DG.Tweening;
using UnityEngine;

public class RingExpand : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private float duration = 0.45f;
    [SerializeField] private float endScale = 2.2f;
    [SerializeField] private float startScale = 0.2f;

    public void Play(Color color)
    {
        sr.color = color;
        transform.localScale = Vector3.one * startScale;

        var seq = DOTween.Sequence();
        seq.Append(transform.DOScale(endScale, duration).SetEase(Ease.OutQuad));
        seq.Join(sr.DOFade(0f, duration).SetEase(Ease.OutQuad));
        seq.OnComplete(() => Destroy(gameObject));
    }

    public void Setup(SpriteRenderer renderer, float dur, float start, float end)
    {
        sr = renderer;
        duration = dur;
        startScale = start;
        endScale = end;
    }
}
