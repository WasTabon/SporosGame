using System;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class ShopPopup : PopupBase
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button restoreButton;
    [SerializeField] private GameObject buyButtonGo;
    [SerializeField] private GameObject ownedLabelGo;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    public event Action OnPurchaseSuccess;

    protected override void Awake()
    {
        base.Awake();
        closeButton.onClick.AddListener(Hide);
    }

    public override void Show()
    {
        base.Show();
        RefreshState();
    }

    public void RefreshState()
    {
        bool owned = LevelManager.IsExtraPackUnlocked();
        if (buyButtonGo != null) buyButtonGo.SetActive(!owned);
        if (ownedLabelGo != null) ownedLabelGo.SetActive(owned);
    }

    public void OnPurchaseCompleted(Product product)
    {
        if (product == null) return;
        if (product.definition.id != IAPManager.ExtraPackProductId) return;
        IAPManager.HandlePurchaseComplete(product);
        RefreshState();
        OnPurchaseSuccess?.Invoke();
    }

    public void OnPurchaseFailedEvent(Product product, PurchaseFailureReason reason)
    {
        IAPManager.HandlePurchaseFailed(product, reason);
    }

    public void OnProductFetched(Product product)
    {
        if (product == null) return;
        if (product.definition.id != IAPManager.ExtraPackProductId) return;
        if (priceText != null && product.metadata != null)
            priceText.text = product.metadata.localizedPriceString;
    }
}
