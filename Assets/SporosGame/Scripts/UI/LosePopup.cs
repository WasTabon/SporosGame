using System;
using UnityEngine;
using UnityEngine.UI;

public class LosePopup : PopupBase
{
    [SerializeField] private Button retryButton;
    [SerializeField] private Button menuButton;

    public event Action OnRetry;
    public event Action OnMenu;

    protected override void Awake()
    {
        base.Awake();
        retryButton.onClick.AddListener(() => { Hide(); OnRetry?.Invoke(); });
        menuButton.onClick.AddListener(() => { Hide(); OnMenu?.Invoke(); });
    }
}
