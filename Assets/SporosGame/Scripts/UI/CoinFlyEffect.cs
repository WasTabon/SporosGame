using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CoinFlyEffect : MonoBehaviour
{
    [SerializeField] private GameObject coinPrefab;

    public void Fly(int count, RectTransform source, RectTransform target, Action onArrived, Action onComplete)
    {
        if (source == null || target == null) { onComplete?.Invoke(); return; }
        if (count < 1) count = 1;
        if (count > 12) count = 12;

        int arrived = 0;
        for (int i = 0; i < count; i++)
        {
            int idx = i;
            float delay = idx * 0.07f;
            DOVirtual.DelayedCall(delay, () =>
            {
                SpawnOne(source, target, () =>
                {
                    arrived++;
                    onArrived?.Invoke();
                    if (arrived >= count) onComplete?.Invoke();
                });
            }).SetUpdate(true);
        }
    }

    private void SpawnOne(RectTransform source, RectTransform target, Action onArrived)
    {
        var go = Instantiate(coinPrefab, transform);
        var rt = go.GetComponent<RectTransform>();

        Vector3 srcWorld = source.TransformPoint(Vector3.zero);
        Vector3 dstWorld = target.TransformPoint(Vector3.zero);

        var canvas = GetComponentInParent<Canvas>();
        var camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        Vector2 srcLocal, dstLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)transform,
            RectTransformUtility.WorldToScreenPoint(camera, srcWorld),
            camera, out srcLocal);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)transform,
            RectTransformUtility.WorldToScreenPoint(camera, dstWorld),
            camera, out dstLocal);

        rt.anchoredPosition = srcLocal;
        rt.localScale = Vector3.one * 0.4f;
        var img = go.GetComponent<Image>();
        if (img != null) { var c = img.color; c.a = 0f; img.color = c; }

        float angleSpread = UnityEngine.Random.Range(-1f, 1f);
        Vector2 midOffset = new Vector2(angleSpread * 180f, UnityEngine.Random.Range(40f, 140f));
        Vector2 midPoint = Vector2.Lerp(srcLocal, dstLocal, 0.5f) + midOffset;

        var seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(rt.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
        if (img != null) seq.Join(img.DOFade(1f, 0.18f));

        float flightTime = UnityEngine.Random.Range(0.55f, 0.75f);
        seq.Append(DOTween.To(() => 0f, t =>
        {
            Vector2 p = Bezier(srcLocal, midPoint, dstLocal, t);
            rt.anchoredPosition = p;
        }, 1f, flightTime).SetEase(Ease.InQuad));
        seq.Join(rt.DOScale(0.55f, flightTime).SetEase(Ease.InQuad));
        seq.OnComplete(() =>
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlaySfx(SfxType.Pop);
            onArrived?.Invoke();
            Destroy(go);
        });
    }

    private static Vector2 Bezier(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        Vector2 ab = Vector2.Lerp(a, b, t);
        Vector2 bc = Vector2.Lerp(b, c, t);
        return Vector2.Lerp(ab, bc, t);
    }

    public void Setup(GameObject prefab) => coinPrefab = prefab;
}
