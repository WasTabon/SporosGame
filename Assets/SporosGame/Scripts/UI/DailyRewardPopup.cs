using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewardPopup : PopupBase
{
    [SerializeField] private Button claimButton;
    [SerializeField] private TMP_Text claimButtonLabel;
    [SerializeField] private RectTransform dayBoxesContainer;
    [SerializeField] private GameObject dayBoxPrefab;
    [SerializeField] private RectTransform coinFlySource;
    [SerializeField] private CoinFlyEffect coinFlyEffect;
    [SerializeField] private TMP_Text titleText;

    public event Action OnClaimed;
    private RectTransform coinTarget;
    private DayBoxView[] boxes;

    private static readonly Color ColorBoxLocked = new Color(0.227f, 0.263f, 0.408f, 0.5f);
    private static readonly Color ColorBoxClaimed = new Color(0f, 1f, 0.533f, 0.6f);
    private static readonly Color ColorBoxCurrent = new Color(0f, 0.898f, 1f, 1f);

    protected override void Awake()
    {
        base.Awake();
        claimButton.onClick.AddListener(HandleClaim);
    }

    public void ShowWithCoinTarget(RectTransform target)
    {
        coinTarget = target;
        BuildBoxes();
        Show();
        UpdateUI();
    }

    private void BuildBoxes()
    {
        if (dayBoxesContainer == null || dayBoxPrefab == null) return;

        for (int i = dayBoxesContainer.childCount - 1; i >= 0; i--)
            Destroy(dayBoxesContainer.GetChild(i).gameObject);

        int total = DailyRewardManager.Rewards.Length;
        boxes = new DayBoxView[total];
        for (int i = 0; i < total; i++)
        {
            var go = Instantiate(dayBoxPrefab, dayBoxesContainer);
            var box = go.GetComponent<DayBoxView>();
            int day = i + 1;
            int reward = DailyRewardManager.Rewards[i];
            bool isJackpot = day == total;
            box.Init(day, reward, isJackpot);
            boxes[i] = box;
        }
    }

    private void UpdateUI()
    {
        if (boxes == null) return;
        bool available = DailyRewardManager.IsRewardAvailable();
        int pendingDay = DailyRewardManager.GetPendingDay();
        int alreadyClaimedDay = available ? pendingDay - 1 : pendingDay;
        if (alreadyClaimedDay < 0) alreadyClaimedDay = 0;

        for (int i = 0; i < boxes.Length; i++)
        {
            int day = i + 1;
            DayBoxState st = DayBoxState.Future;
            if (available && day == pendingDay) st = DayBoxState.Current;
            else if (day <= alreadyClaimedDay) st = DayBoxState.Claimed;
            boxes[i].SetState(st);
        }

        claimButton.interactable = available;
        if (claimButtonLabel != null)
            claimButtonLabel.text = available ? "CLAIM" : "COME BACK TOMORROW";
    }

    private void HandleClaim()
    {
        if (!DailyRewardManager.IsRewardAvailable()) return;
        int reward = DailyRewardManager.Claim();
        if (reward <= 0) return;

        if (coinFlyEffect != null && coinFlySource != null && coinTarget != null)
        {
            coinFlyEffect.Fly(Mathf.Min(reward, 10), coinFlySource, coinTarget, null, null);
            DOVirtual.DelayedCall(0.4f, () => CurrencyManager.AddCoins(reward)).SetUpdate(true);
        }
        else
        {
            CurrencyManager.AddCoins(reward);
        }

        UpdateUI();
        OnClaimed?.Invoke();

        DOVirtual.DelayedCall(1.2f, Hide).SetUpdate(true);
    }
}
