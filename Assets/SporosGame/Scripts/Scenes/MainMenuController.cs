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
    [SerializeField] private SettingsPopup settingsPopup;
    [SerializeField] private ShopPopup shopPopup;
    [SerializeField] private CoinCounter coinCounter;
    [SerializeField] private DailyRewardPopup dailyRewardPopup;

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

        if (shopPopup != null) shopPopup.OnPurchaseSuccess += HandlePurchaseSuccess;

        TryShowDailyReward();
    }

    private void OnDestroy()
    {
        if (shopPopup != null) shopPopup.OnPurchaseSuccess -= HandlePurchaseSuccess;
    }

    private void TryShowDailyReward()
    {
        if (dailyRewardPopup == null) return;
        if (!DailyRewardManager.IsRewardAvailable()) return;
        DOVirtual.DelayedCall(0.6f, () =>
        {
            RectTransform target = coinCounter != null ? coinCounter.IconRect : null;
            dailyRewardPopup.ShowWithCoinTarget(target);
        }).SetUpdate(true);
    }

    private void OnPlay() { TransitionManager.Instance.LoadScene("LevelSelect"); }
    private void OnSettings() { if (settingsPopup != null) settingsPopup.Show(); }
    private void OnShop() { if (shopPopup != null) shopPopup.Show(); }

    private void HandlePurchaseSuccess()
    {
        if (shopPopup != null) shopPopup.RefreshState();
    }
}
