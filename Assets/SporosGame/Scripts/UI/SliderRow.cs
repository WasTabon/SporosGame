using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderRow : MonoBehaviour
{
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text valueText;

    public event Action<float> OnValueChanged;

    public void Init(string label, float initialValue)
    {
        if (labelText != null) labelText.text = label;
        if (slider != null)
        {
            slider.onValueChanged.RemoveAllListeners();
            slider.value = initialValue;
            slider.onValueChanged.AddListener(HandleChanged);
        }
        UpdateValueText(initialValue);
    }

    private void HandleChanged(float v)
    {
        UpdateValueText(v);
        OnValueChanged?.Invoke(v);
    }

    private void UpdateValueText(float v)
    {
        if (valueText != null) valueText.text = Mathf.RoundToInt(v * 100f).ToString();
    }
}
