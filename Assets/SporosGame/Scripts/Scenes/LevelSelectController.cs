using UnityEngine;
using UnityEngine.UI;

public class LevelSelectController : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private Button level1Button;

    private void Start()
    {
        backButton.onClick.AddListener(OnBack);
        level1Button.onClick.AddListener(OnLevel1);
    }

    private void OnBack()
    {
        TransitionManager.Instance.LoadScene("MainMenu");
    }

    private void OnLevel1()
    {
        TransitionManager.Instance.LoadScene("Game");
    }
}
