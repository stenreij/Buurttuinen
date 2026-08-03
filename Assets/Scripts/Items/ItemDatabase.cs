using UnityEngine;
using System.Collections.Generic;

public class ItemDatabase : MonoBehaviour
{
    public List<ItemData> allItems;
    private Dictionary<ItemData, int> itemPool;

    void Start()
    {
        InitializePool();
    }

    public void InitializePool()
    {
        itemPool = new Dictionary<ItemData, int>();
        foreach (ItemData item in allItems)
        {
            itemPool[item] = item.maxAmount;
            Debug.Log($"📦 {item.itemName}: {item.maxAmount} available");
        }
    }

    public bool TryTakeItem(ItemData item)
    {
        if (!itemPool.ContainsKey(item)) return false;
        if (itemPool[item] <= 0) return false;

        itemPool[item]--;
        Debug.Log($"📦 {item.itemName} taken! {itemPool[item]} left");
        return true;
    }

    public int GetRemaining(ItemData item)
    {
        return itemPool.ContainsKey(item) ? itemPool[item] : 0;
    }

    public bool IsAvailable(ItemData item)
    {
        return itemPool.ContainsKey(item) && itemPool[item] > 0;
    }

    public List<ItemData> GetAvailableItems()
    {
        List<ItemData> available = new List<ItemData>();
        foreach (ItemData item in allItems)
        {
            if (IsAvailable(item))
            {
                available.Add(item);
            }
        }
        return available;
    }
}