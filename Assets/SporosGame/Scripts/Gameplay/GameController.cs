using DG.Tweening;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private GridSystem grid;
    [SerializeField] private SporeInventory inventory;
    [SerializeField] private HUDController hud;
    [SerializeField] private GameObject sporePrefab;
    [SerializeField] private Transform sporeParent;
    [SerializeField] private Camera gameCamera;
    [SerializeField] private WinPopup winPopup;
    [SerializeField] private LosePopup losePopup;
    [SerializeField] private PausePopup pausePopup;

    private Spore activeDragSpore;
    private SporeInventoryItem activeItem;
    private LevelConfig currentLevel;
    private UndoSystem undoSystem;
    private bool resolving;
    private bool levelEnded;

    private void Start()
    {
        if (gameCamera == null) gameCamera = Camera.main;

        Time.timeScale = 1f;
        undoSystem = new UndoSystem();

        int idx = LevelManager.CurrentLevel;
        currentLevel = LevelConfig.CreateByIndex(idx);
        grid.Build(currentLevel.Width, currentLevel.Height, currentLevel.Cells);
        FitCameraToGrid();

        ScreenShake.SetTarget(grid.transform);

        inventory.Build(currentLevel.Spores);
        hud.SetLevel(currentLevel.LevelIndex);
        hud.StartTimer();
        hud.SetUndoEnabled(false);

        inventory.OnItemDragBegin += HandleDragBegin;
        inventory.OnItemDragMove += HandleDragMove;
        inventory.OnItemDragEnd += HandleDragEnd;

        hud.OnBackPressed += HandleMenu;
        hud.OnPausePressed += HandlePause;
        hud.OnUndoPressed += HandleUndo;
        hud.OnResetPressed += HandleReset;

        winPopup.OnNext += HandleNext;
        winPopup.OnRetry += HandleReset;
        winPopup.OnMenu += HandleMenu;
        losePopup.OnRetry += HandleReset;
        losePopup.OnMenu += HandleMenu;
        pausePopup.OnResume += HandleResume;
        pausePopup.OnRestart += HandleReset;
        pausePopup.OnMenu += HandleMenu;
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnItemDragBegin -= HandleDragBegin;
            inventory.OnItemDragMove -= HandleDragMove;
            inventory.OnItemDragEnd -= HandleDragEnd;
        }
        if (hud != null)
        {
            hud.OnBackPressed -= HandleMenu;
            hud.OnPausePressed -= HandlePause;
            hud.OnUndoPressed -= HandleUndo;
            hud.OnResetPressed -= HandleReset;
        }
        if (winPopup != null)
        {
            winPopup.OnNext -= HandleNext;
            winPopup.OnRetry -= HandleReset;
            winPopup.OnMenu -= HandleMenu;
        }
        if (losePopup != null)
        {
            losePopup.OnRetry -= HandleReset;
            losePopup.OnMenu -= HandleMenu;
        }
        if (pausePopup != null)
        {
            pausePopup.OnResume -= HandleResume;
            pausePopup.OnRestart -= HandleReset;
            pausePopup.OnMenu -= HandleMenu;
        }
        Time.timeScale = 1f;
    }

    private void FitCameraToGrid()
    {
        var b = grid.GetBounds();
        float pad = 1.2f;
        float aspect = (float)Screen.width / Screen.height;
        float vert = b.extents.y + pad;
        float horiz = (b.extents.x + pad) / aspect;
        gameCamera.orthographicSize = Mathf.Max(vert, horiz, 4f);
        var p = gameCamera.transform.position;
        gameCamera.transform.position = new Vector3(b.center.x, b.center.y, p.z);
    }

    private void HandleDragBegin(SporeInventoryItem item, Vector3 wp)
    {
        if (resolving || levelEnded) return;
        if (item.Count <= 0) return;

        float sporeScale = grid.CellSize * 0.45f;

        var go = Instantiate(sporePrefab, sporeParent);
        activeDragSpore = go.GetComponent<Spore>();
        activeDragSpore.Init(item.Type);
        activeDragSpore.SetPlacedScale(sporeScale);
        activeDragSpore.transform.position = wp;
        activeDragSpore.transform.localScale = Vector3.one * sporeScale * 0.95f;
        activeItem = item;
    }

    private void HandleDragMove(SporeInventoryItem item, Vector3 wp)
    {
        if (activeDragSpore == null) return;
        activeDragSpore.FollowDrag(wp);
    }

    private void HandleDragEnd(SporeInventoryItem item, Vector3 wp)
    {
        if (activeDragSpore == null) return;

        var target = grid.FindClosestCell(wp, grid.CellSize * 0.7f);
        if (target != null && target.State == CellState.Inactive && target.Type != CellType.Block)
        {
            undoSystem.SaveSnapshot(grid, inventory, activeDragSpore.gameObject, target.GridX, target.GridY, item.Type);
            hud.SetUndoEnabled(true);

            inventory.ConsumeOne(item.Type);
            resolving = true;
            var spore = activeDragSpore;
            activeDragSpore = null;
            activeItem = null;
            StartCoroutine(spore.PlaceAndEmit(target, grid, OnSporeResolved));
        }
        else
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlaySfx(SfxType.Fail);
            if (HapticManager.Instance != null) HapticManager.Instance.Play(HapticType.Warning);
            activeDragSpore.DestroySelf();
            activeDragSpore = null;
            activeItem = null;
        }
    }

    private void OnSporeResolved()
    {
        resolving = false;
        if (grid.AreAllActivated())
        {
            levelEnded = true;
            hud.StopTimer();
            ScreenShake.Shake(0.28f, 0.5f);
            if (HapticManager.Instance != null) HapticManager.Instance.Play(HapticType.Success);
            int stars = 3;
            LevelManager.SetStars(currentLevel.LevelIndex, stars);
            DOVirtual.DelayedCall(0.55f, () => ShowWin(stars)).SetUpdate(true);
        }
        else if (!inventory.HasAny())
        {
            levelEnded = true;
            hud.StopTimer();
            if (HapticManager.Instance != null) HapticManager.Instance.Play(HapticType.Failure);
            DOVirtual.DelayedCall(0.4f, () =>
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlaySfx(SfxType.Fail);
                losePopup.Show();
            }).SetUpdate(true);
        }
    }

    private void ShowWin(int stars)
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySfx(SfxType.Success);
        winPopup.ShowWithStars(stars);
    }

    private void HandleUndo()
    {
        if (!undoSystem.CanUndo || resolving || levelEnded) return;

        var snap = undoSystem.Consume();

        for (int y = 0; y < grid.Height; y++)
        for (int x = 0; x < grid.Width; x++)
        {
            var c = grid.GetCell(x, y);
            if (c != null) c.ForceSetState(snap.CellStates[x, y]);
        }

        foreach (var kv in snap.InventoryCounts)
            inventory.SetCount(kv.Key, kv.Value);

        for (int i = 0; i < snap.PlacedSpores.Count; i++)
        {
            var p = snap.PlacedSpores[i];
            if (p.SporeGameObject != null)
            {
                var s = p.SporeGameObject.GetComponent<Spore>();
                if (s != null) s.DestroySelf();
                else Destroy(p.SporeGameObject);
            }
        }

        hud.SetUndoEnabled(false);
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySfx(SfxType.PopupClose);
        if (HapticManager.Instance != null) HapticManager.Instance.Play(HapticType.Light);
    }

    private void HandleReset()
    {
        Time.timeScale = 1f;
        TransitionManager.Instance.LoadScene("Game");
    }

    private void HandleNext()
    {
        Time.timeScale = 1f;
        LevelManager.AdvanceLevel();
        TransitionManager.Instance.LoadScene("Game");
    }

    private void HandleMenu()
    {
        Time.timeScale = 1f;
        TransitionManager.Instance.LoadScene("MainMenu");
    }

    private void HandlePause()
    {
        if (levelEnded || resolving) return;
        hud.PauseTimer();
        pausePopup.Show();
    }

    private void HandleResume()
    {
        hud.ResumeTimer();
    }
}
