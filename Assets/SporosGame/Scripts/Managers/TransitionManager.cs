using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

    private Canvas canvas;
    private CanvasGroup group;
    private Image fadeImage;
    private bool transitioning;

    private const float FadeDuration = 0.35f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildOverlay();
    }

    private void BuildOverlay()
    {
        var canvasGo = new GameObject("TransitionCanvas");
        canvasGo.transform.SetParent(transform);
        canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 1f;
        canvasGo.AddComponent<GraphicRaycaster>();

        group = canvasGo.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;

        var imgGo = new GameObject("FadeImage");
        imgGo.transform.SetParent(canvasGo.transform, false);
        fadeImage = imgGo.AddComponent<Image>();
        fadeImage.color = new Color(0.039f, 0.055f, 0.153f, 1f);
        var rt = fadeImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    public void LoadScene(string sceneName)
    {
        if (transitioning) return;
        StartCoroutine(LoadRoutine(sceneName));
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        transitioning = true;
        group.blocksRaycasts = true;

        yield return group.DOFade(1f, FadeDuration).SetEase(Ease.InOutQuad).WaitForCompletion();

        var op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null;

        yield return new WaitForSeconds(0.05f);

        yield return group.DOFade(0f, FadeDuration).SetEase(Ease.InOutQuad).WaitForCompletion();

        group.blocksRaycasts = false;
        transitioning = false;
    }

    public bool IsTransitioning() => transitioning;
}
