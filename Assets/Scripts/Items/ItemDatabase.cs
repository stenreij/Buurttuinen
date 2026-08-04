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
        Debug.Log("📦 ItemDatabase Awake() - Pool geïnitialiseerd!");
    }

    void Start()
    {
        Debug.Log("📦 ItemDatabase Start() - Klaar voor gebruik!");
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
                Debug.Log($"📦 {item.itemName}: {item.maxAmount} available");
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
            Debug.Log($"❌ {item.itemName} is niet meer beschikbaar (0 left)");
            return false;
        }

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