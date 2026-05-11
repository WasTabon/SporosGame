using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    private RectTransform rt;
    private Rect lastSafeArea;
    private Vector2Int lastScreen;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        Apply();
    }

    private void Update()
    {
        if (Screen.safeArea != lastSafeArea || Screen.width != lastScreen.x || Screen.height != lastScreen.y)
            Apply();
    }

    private void Apply()
    {
        var safe = Screen.safeArea;
        lastSafeArea = safe;
        lastScreen = new Vector2Int(Screen.width, Screen.height);

        var anchorMin = safe.position;
        var anchorMax = safe.position + safe.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
