using UnityEngine;
using System.Collections.Generic;

public class ItemActions : MonoBehaviour
{
    [Header("References")]
    public Inventory playerInventory;
    public ItemDatabase itemDatabase;
    public TurnManager turnManager;

    private ItemData selectedItem;

    [Header("Start Items")]
    public int startItemCount = 4;

    void Start()
    {
        if (turnManager == null)
            turnManager = FindFirstObjectByType<TurnManager>();
    }

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

    // ✅ SELECT ITEM
    public void SelectItem(ItemData item)
    {
        selectedItem = item;
        Debug.Log($"✅ Item selected: {item.itemName}");
    }

    public ItemData GetSelectedItem()
    {
        return selectedItem;
    }

    public void ClearSelectedItem()
    {
        selectedItem = null;
    }

    // ✅ PLACE ITEM
    public void PlaceItem(GardenTile targetTile)
    {
        // Check if the player has an inventory
        if (selectedItem == null)
        {
            Debug.Log("⚠️ No item selected!");
            return;
        }

        // Check if the target tile is already occupied
        if (targetTile.occupied)
        {
            Debug.Log("⚠️ this tile is already occupied!");
            return;
        }

        // Check if the player has the selected item in their inventory
        if (!playerInventory.HasItem(selectedItem))
        {
            Debug.Log($"⚠️ {gameObject.name} has {selectedItem.itemName} not in his inventory!");
            return;
        }

        // Check if the player has already placed an item this turn
        if (turnManager != null && turnManager.hasPlacedItemThisTurn)
        {
            Debug.Log("⚠️ You have already placed an item this turn!");
            return;
        }

        // Remove the item from the player's inventory
        playerInventory.RemoveItem(selectedItem);

        // Make sure the prefab exists before instantiating
        if (selectedItem.prefab != null)
        {
            GameObject placed = Instantiate(selectedItem.prefab, targetTile.transform.position, Quaternion.identity);
            placed.transform.parent = targetTile.transform;
            targetTile.placedItem = placed;
        }
        else
        {
            Debug.LogWarning($"⚠️ No prefab for {selectedItem.itemName}!");
        }

        // Mark the tile as occupied
        targetTile.occupied = true;

        // Mark that the player has placed an item this turn
        if (turnManager != null)
        {
            turnManager.hasPlacedItemThisTurn = true;
        }

        Debug.Log($"✅ {selectedItem.itemName} placed on tile!");

        // Reset selected item
        ClearSelectedItem();

        // Update UI
        InventoryUI ui = FindFirstObjectByType<InventoryUI>();
        if (ui != null)
        {
            ui.RefreshUI();
        }
    }

    // PASS, TRADE, REMOVE 
}