using UnityEngine;
using UnityEngine.UI;

public class AchievementsPopup : PopupBase
{
    [SerializeField] private Button closeButton;
    [SerializeField] private RectTransform listContent;
    [SerializeField] private GameObject rowPrefab;

    private bool built;

    protected override void Awake()
    {
        base.Awake();
        if (closeButton != null) closeButton.onClick.AddListener(Hide);
    }

    public override void Show()
    {
        base.Show();
        Build();
    }

    private void Build()
    {
        if (listContent == null || rowPrefab == null) return;

        for (int i = listContent.childCount - 1; i >= 0; i--)
            Destroy(listContent.GetChild(i).gameObject);

        var list = AchievementsManager.All;
        for (int i = 0; i < list.Count; i++)
        {
            var go = Instantiate(rowPrefab, listContent);
            var row = go.GetComponent<AchievementRow>();
            if (row != null) row.Bind(list[i]);
        }
        built = true;
    }
}
