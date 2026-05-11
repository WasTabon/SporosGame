using UnityEngine;

public enum HapticType
{
    Light,
    Medium,
    Heavy,
    Success,
    Warning,
    Failure
}

public class HapticManager : MonoBehaviour
{
    public static HapticManager Instance { get; private set; }

    private bool enabled = true;
    private const string KeyHaptics = "spo_haptics";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        enabled = PlayerPrefs.GetInt(KeyHaptics, 1) == 1;
    }

    public void Play(HapticType type)
    {
        if (!enabled) return;
        if (Application.platform != RuntimePlatform.IPhonePlayer && Application.platform != RuntimePlatform.Android)
            return;

#if UNITY_IOS || UNITY_ANDROID
        Handheld.Vibrate();
#endif
    }

    public void SetEnabled(bool e)
    {
        enabled = e;
        PlayerPrefs.SetInt(KeyHaptics, e ? 1 : 0);
    }

    public bool IsEnabled() => enabled;
}
