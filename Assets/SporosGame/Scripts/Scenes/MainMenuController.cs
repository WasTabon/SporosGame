using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button achievementsButton;
    [SerializeField] private TMP_Text logoText;
    [SerializeField] private SettingsPopup settingsPopup;
    [SerializeField] private ShopPopup shopPopup;
    [SerializeField] private DailyRewardPopup dailyRewardPopup;
    [SerializeField] private AchievementsPopup achievementsPopup;
    [SerializeField] private AchievementUnlockedPopup achievementUnlockedPopup;
    [SerializeField] private CoinCounter coinCounter;

    private void Start()
    {
        playButton.onClick.AddListener(OnPlay);
        settingsButton.onClick.AddListener(OnSettings);
        shopButton.onClick.AddListener(OnShop);
        if (achievementsButton != null) achievementsButton.onClick.AddListener(OnAchievements);

        if (logoText != null)
        {
            logoText.transform.localScale = Vector3.one * 0.85f;
            logoText.transform.DOScale(1f, 1.8f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        }

        if (shopPopup != null) shopPopup.OnPurchaseSuccess += HandlePurchaseSuccess;
        if (dailyRewardPopup != null) dailyRewardPopup.OnClaimed += HandleDailyClaimed;

        if (achievementUnlockedPopup != null) achievementUnlockedPopup.Enable();

        TryShowDailyReward();
    }

    private void OnDestroy()
    {
        if (shopPopup != null) shopPopup.OnPurchaseSuccess -= HandlePurchaseSuccess;
        if (dailyRewardPopup != null) dailyRewardPopup.OnClaimed -= HandleDailyClaimed;
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
    private void OnAchievements() { if (achievementsPopup != null) achievementsPopup.Show(); }

    private void HandlePurchaseSuccess()
    {
        if (shopPopup != null) shopPopup.RefreshState();
        AchievementsManager.OnExtraPackPurchased();
    }

    private void HandleDailyClaimed()
    {
        AchievementsManager.OnDailyClaim();
    }
}
