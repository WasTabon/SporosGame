using UnityEngine;
using UnityEngine.SceneManagement;

public class EffectsManager : MonoBehaviour
{
    public static EffectsManager Instance { get; private set; }

    [SerializeField] private GameObject particleBurstPrefab;
    [SerializeField] private GameObject ringExpandPrefab;

    private Transform fxRoot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
        EnsureRoot();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureRoot();
    }

    private void EnsureRoot()
    {
        if (fxRoot == null)
        {
            var go = new GameObject("FxRoot");
            fxRoot = go.transform;
        }
    }

    public void SpawnBurst(Vector3 worldPos, Color color)
    {
        if (particleBurstPrefab == null) return;
        EnsureRoot();
        var go = Instantiate(particleBurstPrefab, fxRoot);
        go.transform.position = worldPos;
        var burst = go.GetComponent<ParticleBurst>();
        burst.Play(color);
    }

    public void SpawnRing(Vector3 worldPos, Color color, float scale = 1f)
    {
        if (ringExpandPrefab == null) return;
        EnsureRoot();
        var go = Instantiate(ringExpandPrefab, fxRoot);
        go.transform.position = worldPos;
        go.transform.localScale = Vector3.one * scale;
        var ring = go.GetComponent<RingExpand>();
        ring.Play(color);
    }

    public void Setup(GameObject burstPrefab, GameObject ringPrefab)
    {
        particleBurstPrefab = burstPrefab;
        ringExpandPrefab = ringPrefab;
    }
}
