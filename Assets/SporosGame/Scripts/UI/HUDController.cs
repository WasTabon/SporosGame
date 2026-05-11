using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text timerText;

    private float elapsed;
    private bool running;

    public event Action OnBackPressed;

    private void Start()
    {
        backButton.onClick.AddListener(() => OnBackPressed?.Invoke());
        UpdateTimer();
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

    public float GetElapsed() => elapsed;

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
