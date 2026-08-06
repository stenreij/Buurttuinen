using UnityEngine;
using System.Collections.Generic;

public class ItemDatabase : MonoBehaviour
{
    public List<ItemData> allItems;
    private Dictionary<ItemData, int> itemPool;

    void Awake()
    {
        ItemDatabase[] databases = FindObjectsByType<ItemDatabase>(FindObjectsSortMode.None);
        if (databases.Length > 1)
        {
            Debug.LogWarning("⚠️ Multiple ItemDatabases found! Destroying this one.");
            Destroy(gameObject);
            return;
        }

        InitializePool();
    }

    void Start()
    {
        if (itemPool == null || itemPool.Count == 0)
        {
            Debug.LogWarning("⚠️ ItemDatabase pool is empty! Initializing pool.");
            InitializePool();
        }
    }

    public void InitializePool()
    {
        itemPool = new Dictionary<ItemData, int>();
        if (allItems == null || allItems.Count == 0)
        {
            Debug.LogWarning("⚠️ Geen items in ItemDatabase!");
            return;
        }

        foreach (ItemData item in allItems)
        {
            if (item != null)
            {
                itemPool[item] = item.maxAmount;
            }
        }
    }

    public void ResetPool()
    {
        InitializePool();
        Debug.Log("🔄 ItemDatabase pool gereset!");
    }

    public bool TryTakeItem(ItemData item)
    {
        if (item == null)
        {
            Debug.LogWarning("⚠️ TryTakeItem: item is null!");
            return false;
        }

        if (!itemPool.ContainsKey(item))
        {
            Debug.LogWarning($"⚠️ Item {item.itemName} niet gevonden in pool!");
            return false;
        }

        if (itemPool[item] <= 0)
        {
            return false;
        }

        itemPool[item]--;
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