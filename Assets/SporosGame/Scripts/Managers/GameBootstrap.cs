using UnityEngine;
using DG.Tweening;

public class GameBootstrap : MonoBehaviour
{
    private static bool spawned;

    private void Awake()
    {
        if (spawned)
        {
            Destroy(gameObject);
            return;
        }
        spawned = true;
        DontDestroyOnLoad(gameObject);

        Application.targetFrameRate = 60;
        Input.multiTouchEnabled = false;

        DOTween.Init(true, true, LogBehaviour.ErrorsOnly);
        DOTween.SetTweensCapacity(500, 50);

        if (FindObjectOfType<SoundManager>() == null)
        {
            var go = new GameObject("SoundManager");
            go.AddComponent<SoundManager>();
        }
        if (FindObjectOfType<HapticManager>() == null)
        {
            var go = new GameObject("HapticManager");
            go.AddComponent<HapticManager>();
        }
        if (FindObjectOfType<TransitionManager>() == null)
        {
            var go = new GameObject("TransitionManager");
            go.AddComponent<TransitionManager>();
        }
    }
}
