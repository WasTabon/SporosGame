using UnityEngine;
using UnityEngine.UI;

public class GameSceneController : MonoBehaviour
{
    [SerializeField] private Button backButton;

    private void Start()
    {
        backButton.onClick.AddListener(OnBack);
    }

    private void OnBack()
    {
        TransitionManager.Instance.LoadScene("MainMenu");
    }
}
