using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private HighlightOverlay overlay;
    [SerializeField] private HandPointer pointer;

    private const string KeyTutorialCompletedPrefix = "spo_tutorial_completed_";

    private int currentLevelIdx;
    private bool active;
    private TutorialStep step;

    private enum TutorialStep
    {
        ShowSpore,
        ShowCell,
        Done
    }

    public static bool IsCompleted(int levelIdx)
    {
        return PlayerPrefs.GetInt(KeyTutorialCompletedPrefix + levelIdx, 0) == 1;
    }

    public static void MarkCompleted(int levelIdx)
    {
        PlayerPrefs.SetInt(KeyTutorialCompletedPrefix + levelIdx, 1);
        PlayerPrefs.Save();
    }

    public bool IsActive => active;

    public void StartTutorial(int levelIdx, RectTransform sporeInventoryItemRect, Vector3 targetCellWorldPos, Camera worldCamera)
    {
        if (overlay == null || pointer == null) return;
        if (IsCompleted(levelIdx)) return;

        currentLevelIdx = levelIdx;
        active = true;
        step = TutorialStep.ShowSpore;

        ShowSporeStep(sporeInventoryItemRect, targetCellWorldPos, worldCamera);
    }

    private RectTransform cachedSporeRect;
    private Vector3 cachedTargetWorld;
    private Camera cachedCam;

    private void ShowSporeStep(RectTransform sporeRect, Vector3 targetWorld, Camera worldCamera)
    {
        cachedSporeRect = sporeRect;
        cachedTargetWorld = targetWorld;
        cachedCam = worldCamera;
        step = TutorialStep.ShowSpore;
        if (sporeRect != null)
        {
            overlay.ShowAroundRect(sporeRect, new Vector2(30, 30), 280f);
            pointer.ShowDragMotionRectToWorld(sporeRect, targetWorld, worldCamera);
        }
    }

    public void OnDragStarted()
    {
        if (!active) return;
        step = TutorialStep.ShowCell;
        overlay.Hide();
        if (cachedCam != null) pointer.ShowAtPoint(cachedTargetWorld, cachedCam);
        if (cachedCam != null) overlay.ShowAroundWorldPos(cachedTargetWorld, new Vector2(200, 200), cachedCam, 280f);
    }

    public void OnPlacementSucceeded()
    {
        if (!active) return;
        step = TutorialStep.Done;
        active = false;
        overlay.Hide();
        pointer.Hide();
        MarkCompleted(currentLevelIdx);
    }

    public void Stop()
    {
        active = false;
        if (overlay != null) overlay.Hide();
        if (pointer != null) pointer.Hide();
    }

    public void RefreshIfActive(RectTransform sporeRect)
    {
        if (!active) return;
        if (step == TutorialStep.ShowSpore)
        {
            ShowSporeStep(sporeRect, cachedTargetWorld, cachedCam);
        }
    }
}
