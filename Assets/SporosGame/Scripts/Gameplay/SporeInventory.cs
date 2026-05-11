using System;
using System.Collections.Generic;
using UnityEngine;

public class SporeInventory : MonoBehaviour
{
    [SerializeField] private RectTransform itemsParent;
    [SerializeField] private GameObject itemPrefab;

    private List<SporeInventoryItem> items = new List<SporeInventoryItem>();

    public event Action<SporeInventoryItem, Vector3> OnItemDragBegin;
    public event Action<SporeInventoryItem, Vector3> OnItemDragMove;
    public event Action<SporeInventoryItem, Vector3> OnItemDragEnd;

    public void Build(List<LevelConfig.SporeStock> stocks)
    {
        Clear();
        for (int i = 0; i < stocks.Count; i++)
        {
            var go = Instantiate(itemPrefab, itemsParent);
            var item = go.GetComponent<SporeInventoryItem>();
            item.Init(stocks[i].Type, stocks[i].Count);
            item.OnDragBeginEvent += HandleBegin;
            item.OnDragMoveEvent += HandleMove;
            item.OnDragEndEvent += HandleEnd;
            items.Add(item);
        }
    }

    private void HandleBegin(SporeInventoryItem item, Vector3 wp) => OnItemDragBegin?.Invoke(item, wp);
    private void HandleMove(SporeInventoryItem item, Vector3 wp) => OnItemDragMove?.Invoke(item, wp);
    private void HandleEnd(SporeInventoryItem item, Vector3 wp) => OnItemDragEnd?.Invoke(item, wp);

    public void ConsumeOne(SporeType type)
    {
        for (int i = 0; i < items.Count; i++)
            if (items[i].Type == type) { items[i].SetCount(items[i].Count - 1); return; }
    }

    public bool HasAny()
    {
        for (int i = 0; i < items.Count; i++) if (items[i].Count > 0) return true;
        return false;
    }

    public void Clear()
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i].OnDragBeginEvent -= HandleBegin;
            items[i].OnDragMoveEvent -= HandleMove;
            items[i].OnDragEndEvent -= HandleEnd;
            Destroy(items[i].gameObject);
        }
        items.Clear();
    }

    private void OnDestroy() => Clear();
}
