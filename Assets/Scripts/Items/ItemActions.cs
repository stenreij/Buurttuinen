using UnityEngine;
using System.Collections.Generic;

public class ItemActions : MonoBehaviour
{
    [Header("References")]
    public Inventory playerInventory;
    public ItemDatabase itemDatabase;

    [Header("Start Items")]
    public int startItemCount = 4;


    public void GiveRandomItem()
    {
        if (itemDatabase == null)
        {
            Debug.LogError("❌ ItemDatabase is NULL for " + gameObject.name);
            return;
        }

        if (playerInventory == null)
        {
            Debug.LogError("❌ PlayerInventory is NULL for " + gameObject.name);
            return;
        }

        List<ItemData> availableItems = new List<ItemData>(itemDatabase.allItems);

        for (int i = availableItems.Count - 1; i >= 0; i--)
        {
            ItemData item = availableItems[i];
            int count = playerInventory.CountItem(item);
            if (count >= item.maxAmount)
            {
                availableItems.RemoveAt(i);
                Debug.Log($"❌ Removed {item.itemName} (max reached)");
            }
        }

        if (availableItems.Count == 0)
        {
            Debug.LogWarning("⚠️ No more items available for " + gameObject.name);
            return;
        }

        int randomIndex = Random.Range(0, availableItems.Count);
        ItemData randomItem = availableItems[randomIndex];

        playerInventory.AddItem(randomItem);
    }

    // PASS, PLACE, TRADE, REMOVE
}