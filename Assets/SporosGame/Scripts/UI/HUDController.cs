using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button undoButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text timerText;

    private float elapsed;
    private bool running;

    public event Action OnBackPressed;
    public event Action OnPausePressed;
    public event Action OnUndoPressed;
    public event Action OnResetPressed;

    private void Start()
    {
        backButton.onClick.AddListener(() => OnBackPressed?.Invoke());
        if (pauseButton != null) pauseButton.onClick.AddListener(() => OnPausePressed?.Invoke());
        if (undoButton != null) undoButton.onClick.AddListener(() => OnUndoPressed?.Invoke());
        if (resetButton != null) resetButton.onClick.AddListener(() => OnResetPressed?.Invoke());
        UpdateTimer();
        SetUndoEnabled(false);
    }

    public void SetLevel(int idx)
    {
        levelText.text = "LEVEL " + idx;
    }

    public void StartTimer()
    {
        elapsed = 0f;
        running = true;
        UpdateTimer();
    }

    public void StopTimer() => running = false;
    public void ResumeTimer() => running = true;
    public void PauseTimer() => running = false;

    public float GetElapsed() => elapsed;

    public void SetUndoEnabled(bool e)
    {
        if (undoButton == null) return;
        undoButton.interactable = e;
        var img = undoButton.GetComponent<Image>();
        if (img != null)
        {
            var c = img.color;
            c.a = e ? 1f : 0.4f;
            img.color = c;
        }
    }

    private void Update()
    {
        if (!running) return;
        elapsed += Time.deltaTime;
        UpdateTimer();
    }

    private void UpdateTimer()
    {
        int m = (int)(elapsed / 60f);
        int s = (int)(elapsed % 60f);
        timerText.text = m.ToString("00") + ":" + s.ToString("00");
    }
}
