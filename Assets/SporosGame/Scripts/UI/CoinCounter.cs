using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CoinCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private Image coinIcon;

    private int displayedValue;
    private Tween counterTween;

    public RectTransform IconRect => coinIcon != null ? coinIcon.rectTransform : null;

    private void OnEnable()
    {
        CurrencyManager.OnCoinsChanged += HandleChanged;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SetImmediate(CurrencyManager.Coins);
    }

    private void OnDisable()
    {
        CurrencyManager.OnCoinsChanged -= HandleChanged;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        counterTween?.Kill();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetImmediate(CurrencyManager.Coins);
    }

    public void SetImmediate(int value)
    {
        counterTween?.Kill();
        displayedValue = value;
        UpdateText();
    }

    private void HandleChanged(int oldVal, int newVal)
    {
        AnimateTo(newVal);
    }

    public void AnimateTo(int target)
    {
        counterTween?.Kill();
        int from = displayedValue;
        counterTween = DOTween.To(() => from, v =>
        {
            displayedValue = v;
            UpdateText();
        }, target, 0.7f).SetEase(Ease.OutCubic).SetUpdate(true)
            .OnComplete(() => { displayedValue = target; UpdateText(); });

        if (coinIcon != null)
        {
            coinIcon.transform.DOKill();
            coinIcon.transform.localScale = Vector3.one;
            coinIcon.transform.DOPunchScale(Vector3.one * 0.25f, 0.35f, 6, 0.6f).SetUpdate(true);
        }
    }

    private void UpdateText()
    {
        if (valueText != null) valueText.text = displayedValue.ToString();
    }
}
