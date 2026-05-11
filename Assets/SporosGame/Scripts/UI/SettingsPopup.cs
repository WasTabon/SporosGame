using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPopup : PopupBase
{
    [SerializeField] private SliderRow sfxRow;
    [SerializeField] private SliderRow musicRow;
    [SerializeField] private Toggle hapticsToggle;
    [SerializeField] private Button restoreButton;
    [SerializeField] private Button closeButton;

    protected override void Awake()
    {
        base.Awake();

        closeButton.onClick.AddListener(Hide);
        hapticsToggle.onValueChanged.AddListener(OnHapticsToggle);

        if (sfxRow != null) sfxRow.OnValueChanged += OnSfxChanged;
        if (musicRow != null) musicRow.OnValueChanged += OnMusicChanged;
    }

    private void OnDestroy()
    {
        if (sfxRow != null) sfxRow.OnValueChanged -= OnSfxChanged;
        if (musicRow != null) musicRow.OnValueChanged -= OnMusicChanged;
    }

    public override void Show()
    {
        base.Show();

        if (SoundManager.Instance != null)
        {
            if (sfxRow != null) sfxRow.Init("SFX", SoundManager.Instance.GetSfxVolume());
            if (musicRow != null) musicRow.Init("MUSIC", SoundManager.Instance.GetMusicVolume());
        }
        if (HapticManager.Instance != null)
            hapticsToggle.SetIsOnWithoutNotify(HapticManager.Instance.IsEnabled());
    }

    private void OnSfxChanged(float v)
    {
        if (SoundManager.Instance != null) SoundManager.Instance.SetSfxVolume(v);
    }

    private void OnMusicChanged(float v)
    {
        if (SoundManager.Instance != null) SoundManager.Instance.SetMusicVolume(v);
    }

    private void OnHapticsToggle(bool v)
    {
        if (HapticManager.Instance != null) HapticManager.Instance.SetEnabled(v);
        if (v && HapticManager.Instance != null) HapticManager.Instance.Play(HapticType.Light);
    }
}
