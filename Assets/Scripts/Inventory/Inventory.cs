using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public List<ItemData> items = new List<ItemData>();

    public void AddItem(ItemData item)
    {
        items.Add(item);
        //Debug.Log(gameObject.name + " received " + item.itemName);
    }

    public void RemoveItem(ItemData item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
            Debug.Log(gameObject.name + " used " + item.itemName);
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