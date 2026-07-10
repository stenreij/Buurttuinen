using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public List<ItemData> items = new List<ItemData>();

    [Header("Start Items")]
    public int startItemCount = 4;
    public ItemDatabase itemDatabase;

    void Start()
    {
        GiveRandomStartItems();
    }

    void GiveRandomStartItems()
    {
        if (itemDatabase == null)
        {
            Debug.LogWarning("Geen ItemDatabase gekoppeld aan " + gameObject.name);
            return;
        }

        // Make a copy of the item list to avoid modifying the original list
        List<ItemData> availableItems = new List<ItemData>(itemDatabase.allItems);
        
        for (int i = 0; i < startItemCount; i++)
        {
            if (availableItems.Count == 0) break;

            int randomIndex = Random.Range(0, availableItems.Count);
            ItemData randomItem = availableItems[randomIndex];
            
            AddItem(randomItem);
            
            // Remove the item from availableItems if the player has reached the max amount
            int currentCount = CountItem(randomItem);
            if (currentCount >= randomItem.maxAmount)
            {
                availableItems.RemoveAt(randomIndex);
            }
        }
    }

    public void AddItem(ItemData item)
    {
        items.Add(item);
        Debug.Log(gameObject.name + " heeft " + item.itemName + " gekregen");
    }

    public void RemoveItem(ItemData item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
            Debug.Log(gameObject.name + " heeft " + item.itemName + " gebruikt");
        }
    }

    public bool HasItem(ItemData item)
    {
        return items.Contains(item);
    }

    public int CountItem(ItemData item)
    {
        int count = 0;
        foreach (ItemData i in items)
        {
            if (i == item) count++;
        }
        return count;
    }
}