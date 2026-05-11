using DG.Tweening;
using UnityEngine;

public static class ScreenShake
{
    private static Tween activeTween;
    private static Transform shakeTarget;
    private static Vector3 originalPos;

    public static void SetTarget(Transform t)
    {
        if (shakeTarget == t) return;
        if (shakeTarget != null && activeTween != null) activeTween.Kill(true);
        shakeTarget = t;
        if (t != null) originalPos = t.localPosition;
    }

    public static void Shake(float intensity, float duration)
    {
        if (shakeTarget == null) return;
        activeTween?.Kill(true);
        shakeTarget.localPosition = originalPos;
        activeTween = shakeTarget.DOShakePosition(duration, new Vector3(intensity, intensity, 0f), 20, 90, false, true)
            .OnComplete(() => { if (shakeTarget != null) shakeTarget.localPosition = originalPos; });
    }

    public static void MicroShake()
    {
        Shake(0.04f, 0.1f);
    }
}
