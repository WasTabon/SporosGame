using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private TMP_Text logoText;
    [SerializeField] private PopupBase settingsPopup;
    [SerializeField] private PopupBase shopPopup;
    [SerializeField] private CoinCounter coinCounter;

    private void Start()
    {
        playButton.onClick.AddListener(OnPlay);
        settingsButton.onClick.AddListener(OnSettings);
        shopButton.onClick.AddListener(OnShop);

        if (logoText != null)
        {
            logoText.transform.localScale = Vector3.one * 0.85f;
            logoText.transform.DOScale(1f, 1.8f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void OnPlay()
    {
        TransitionManager.Instance.LoadScene("LevelSelect");
    }

    private void OnSettings()
    {
        if (settingsPopup != null) settingsPopup.Show();
    }

    private void OnShop()
    {
        if (shopPopup != null) shopPopup.Show();
    }
}
