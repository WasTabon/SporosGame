using DG.Tweening;
using UnityEngine;

public class ParticleBurst : MonoBehaviour
{
    [SerializeField] private Sprite particleSprite;
    [SerializeField] private int particleCount = 12;
    [SerializeField] private float radius = 1.3f;
    [SerializeField] private float duration = 0.65f;
    [SerializeField] private float particleSize = 0.18f;

    public void Play(Color color)
    {
        for (int i = 0; i < particleCount; i++)
        {
            float baseAngle = (360f / particleCount) * i;
            float angle = baseAngle + Random.Range(-12f, 12f);
            float rad = angle * Mathf.Deg2Rad;
            float r = radius * Random.Range(0.7f, 1.0f);
            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * r;
            float dur = duration * Random.Range(0.85f, 1.15f);
            SpawnParticle(dir, color, dur);
        }
        Destroy(gameObject, duration * 1.3f);
    }

    private void SpawnParticle(Vector3 endLocal, Color color, float dur)
    {
        var go = new GameObject("p");
        go.transform.SetParent(transform, false);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = particleSprite;
        sr.color = color;
        sr.sortingOrder = 20;
        go.transform.localScale = Vector3.one * particleSize;
        go.transform.localPosition = Vector3.zero;

        var seq = DOTween.Sequence();
        seq.Append(go.transform.DOLocalMove(endLocal, dur).SetEase(Ease.OutCubic));
        seq.Join(go.transform.DOScale(particleSize * 0.1f, dur).SetEase(Ease.InQuad));
        seq.Join(sr.DOFade(0f, dur).SetEase(Ease.InQuad));
    }

    public void Setup(Sprite sprite, int count, float r, float dur, float size)
    {
        particleSprite = sprite;
        particleCount = count;
        radius = r;
        duration = dur;
        particleSize = size;
    }
}
