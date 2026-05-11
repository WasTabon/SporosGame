using System;
using UnityEngine;
using UnityEngine.Purchasing;

public static class IAPManager
{
    public const string ExtraPackProductId = "com.levelpack.inapp";

    public static event Action OnExtraPackUnlocked;

    public static bool IsExtraPackOwned => LevelManager.IsExtraPackUnlocked();

    public static void HandlePurchaseComplete(Product product)
    {
        if (product == null) return;
        if (product.definition.id == ExtraPackProductId)
        {
            UnlockExtraPack();
        }
    }

    public static void HandlePurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        Debug.LogWarning("[IAP] Purchase failed: " + (product != null ? product.definition.id : "unknown") + " reason=" + reason);
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySfx(SfxType.Fail);
        if (HapticManager.Instance != null) HapticManager.Instance.Play(HapticType.Warning);
    }

    public static void HandleProductFetched(Product product)
    {
    }

    public static void UnlockExtraPack()
    {
        LevelManager.SetExtraPackUnlocked(true);
        OnExtraPackUnlocked?.Invoke();
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySfx(SfxType.Success);
        if (HapticManager.Instance != null) HapticManager.Instance.Play(HapticType.Success);
    }
}
