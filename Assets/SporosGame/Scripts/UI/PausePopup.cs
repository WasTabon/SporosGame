using System;
using UnityEngine;
using UnityEngine.UI;

public class PausePopup : PopupBase
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;

    public event Action OnResume;
    public event Action OnRestart;
    public event Action OnMenu;

    protected override void Awake()
    {
        base.Awake();
        resumeButton.onClick.AddListener(() => { Hide(); OnResume?.Invoke(); });
        restartButton.onClick.AddListener(() => { Hide(); OnRestart?.Invoke(); });
        menuButton.onClick.AddListener(() => { Hide(); OnMenu?.Invoke(); });
    }

    public override void Show()
    {
        base.Show();
        Time.timeScale = 0f;
    }

    public override void Hide()
    {
        Time.timeScale = 1f;
        base.Hide();
    }
}
