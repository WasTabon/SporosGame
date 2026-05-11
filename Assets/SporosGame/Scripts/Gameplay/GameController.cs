using UnityEngine;
using UnityEngine.EventSystems;

public class GameController : MonoBehaviour
{
    [SerializeField] private GridSystem grid;
    [SerializeField] private SporeInventory inventory;
    [SerializeField] private HUDController hud;
    [SerializeField] private GameObject sporePrefab;
    [SerializeField] private Transform sporeParent;
    [SerializeField] private Camera gameCamera;

    private Spore activeDragSpore;
    private SporeInventoryItem activeItem;
    private LevelConfig currentLevel;
    private bool resolving;
    private bool levelWon;

    private void Start()
    {
        if (gameCamera == null) gameCamera = Camera.main;

        currentLevel = LevelConfig.CreateLevel1();
        grid.Build(currentLevel.Width, currentLevel.Height, currentLevel.Cells);
        FitCameraToGrid();
        inventory.Build(currentLevel.Spores);
        hud.SetLevel(1);
        hud.StartTimer();

        inventory.OnItemDragBegin += HandleDragBegin;
        inventory.OnItemDragMove += HandleDragMove;
        inventory.OnItemDragEnd += HandleDragEnd;

        hud.OnBackPressed += HandleBack;
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnItemDragBegin -= HandleDragBegin;
            inventory.OnItemDragMove -= HandleDragMove;
            inventory.OnItemDragEnd -= HandleDragEnd;
        }
        if (hud != null) hud.OnBackPressed -= HandleBack;
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
        if (resolving || levelWon) return;
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
            levelWon = true;
            hud.StopTimer();
            Debug.Log("[SporosGame] WIN! Time: " + hud.GetElapsed());
            if (SoundManager.Instance != null) SoundManager.Instance.PlaySfx(SfxType.Success);
            if (HapticManager.Instance != null) HapticManager.Instance.Play(HapticType.Success);
        }
        else if (!inventory.HasAny())
        {
            Debug.Log("[SporosGame] LOSE — no spores left, board incomplete");
            if (SoundManager.Instance != null) SoundManager.Instance.PlaySfx(SfxType.Fail);
        }
    }

    private void HandleBack()
    {
        TransitionManager.Instance.LoadScene("MainMenu");
    }
}
