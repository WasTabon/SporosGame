using UnityEngine;
using UnityEngine.UI;

public class LevelSelectController : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private Transform levelsContent;
    [SerializeField] private GameObject levelButtonPrefab;

    private void Start()
    {
        backButton.onClick.AddListener(OnBack);
        BuildLevels();
    }

    private void BuildLevels()
    {
        for (int i = levelsContent.childCount - 1; i >= 0; i--)
            Destroy(levelsContent.GetChild(i).gameObject);

        int total = LevelManager.TotalLevels;
        if (total <= 0) return;

        for (int i = 1; i <= total; i++)
        {
            var go = Instantiate(levelButtonPrefab, levelsContent);
            var btn = go.GetComponent<LevelButton>();
            int stars = LevelManager.GetStars(i);
            bool unlocked = LevelManager.IsUnlocked(i);
            var data = LevelManager.GetLevel(i);
            bool isExtra = data != null && data.isExtraPack;
            btn.Bind(i, stars, unlocked, isExtra);
            btn.OnClicked += OnLevelClicked;
        }
    }

    private void OnLevelClicked(int idx)
    {
        LevelManager.CurrentLevel = idx;
        TransitionManager.Instance.LoadScene("Game");
    }

    private void OnBack()
    {
        TransitionManager.Instance.LoadScene("MainMenu");
    }
}
