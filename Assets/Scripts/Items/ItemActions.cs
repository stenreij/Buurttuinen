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
        // Find TurnManager if not assigned
        if (turnManager == null)
        {
            turnManager = FindFirstObjectByType<TurnManager>();
            if (turnManager == null)
            {
                Debug.LogWarning("⚠️ TurnManager not found in ItemActions!");
            }
        }

        // Find Inventory if not assigned
        if (playerInventory == null)
        {
            playerInventory = GetComponent<Inventory>();
            if (playerInventory == null)
            {
                Debug.LogError($"❌ No Inventory found on {gameObject.name}!");
            }
        }
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

        // Get available items from the item database
        List<ItemData> availableItems = itemDatabase.GetAvailableItems();

        if (availableItems.Count == 0)
        {
            Debug.LogWarning("⚠️ No more items available for " + gameObject.name);
            return;
        }

        int randomIndex = Random.Range(0, availableItems.Count);
        ItemData randomItem = availableItems[randomIndex];

        // Check if the item can be taken from the pool
        if (itemDatabase.TryTakeItem(randomItem))
        {
            playerInventory.AddItem(randomItem);
            //Debug.Log($"✅ {gameObject.name} received {randomItem.itemName} ({itemDatabase.GetRemaining(randomItem)} left)");
        }
        else
        {
            Debug.LogWarning($"⚠️ Could not take {randomItem.itemName} from pool!");
        }
    }

    // GET TOTAL ITEM COUNT IN ALL INVENTORIES
    private int GetTotalItemCount(ItemData item)
    {
        int total = 0;

        Inventory[] allInventories = FindObjectsByType<Inventory>(FindObjectsSortMode.None);

        foreach (Inventory inv in allInventories)
        {
            total += inv.CountItem(item);
        }

        return total;
    }


    // SELECT ITEM
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

    // PLACE ITEM
    public void PlaceItem(GardenTile targetTile)
    {
        // 1. Check if playerInventory exists
        if (playerInventory == null)
        {
            Debug.LogError($"❌ PlayerInventory is NULL on {gameObject.name}!");
            return;
        }

        // 2. Check if there is a selected item
        if (selectedItem == null)
        {
            Debug.Log("⚠️ No item selected!");
            return;
        }

        // 3. Check if the tile is occupied
        if (targetTile.occupied)
        {
            Debug.Log("⚠️ This tile is already occupied!");
            return;
        }

        // 4. Check if the player has the item in inventory
        if (!playerInventory.HasItem(selectedItem))
        {
            Debug.Log($"⚠️ {gameObject.name} does not have {selectedItem.itemName} in inventory!");
            return;
        }

        // 5. Find TurnManager if not already set
        if (turnManager == null)
        {
            turnManager = FindFirstObjectByType<TurnManager>();
            if (turnManager == null)
            {
                Debug.LogWarning("⚠️ TurnManager not found!");
            }
        }

        // 6. Check if the player has already placed an item this turn
        if (turnManager != null && turnManager.hasPlacedItemThisTurn)
        {
            Debug.Log("⚠️ You have already placed an item this turn!");
            return;
        }

        // 7. Remove item from inventory
        playerInventory.RemoveItem(selectedItem);

        // 8. Place item on tile
        if (selectedItem.prefab != null)
        {
            GameObject placed = Instantiate(selectedItem.prefab, targetTile.transform.position, Quaternion.identity);
            placed.transform.parent = targetTile.transform;
            placed.transform.localPosition = Vector3.zero;

            // Force the SpriteRenderer to be on top of the ground
            SpriteRenderer sr = placed.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = true;
                sr.sortingOrder = 10;
                //Debug.Log($"🎨 SpriteRenderer enabled for {selectedItem.itemName}");
            }
            else
            {
                Debug.LogWarning($"⚠️ Geen SpriteRenderer op prefab van {selectedItem.itemName}!");
            }

            targetTile.placedItem = placed;
            targetTile.placedItemData = selectedItem;
        }
        else
        {
            Debug.LogWarning($"⚠️ No prefab for {selectedItem.itemName}!");
        }

        // 9. Mark tile as occupied
        targetTile.occupied = true;

        // 10. Mark that the player has placed an item this turn
        if (turnManager != null)
        {
            turnManager.hasPlacedItemThisTurn = true;
            turnManager.UpdateActionButtons();
        }

        // 11. Log the placement BEFORE clearing the selection
        Debug.Log($"✅ {selectedItem.itemName} placed on tile at {targetTile.transform.position}!");

        // 12. Clear selection
        ClearSelectedItem();

        // 13. Update UI
        InventoryUI ui = FindFirstObjectByType<InventoryUI>();
        if (ui != null)
        {
            ui.RefreshUI();
        }
    }

    // PASS TURN
    public void PassTurn()
    {
        TurnManager turnManager = FindFirstObjectByType<TurnManager>();
        if (turnManager != null)
        {
            GiveRandomItem();
            Debug.Log($"⏭️ {gameObject.name} passed and received a random item!");

            InventoryUI ui = FindFirstObjectByType<InventoryUI>();
            if (ui != null)
            {
                ui.RefreshUI();
            }

            turnManager.EndTurn();
        }
        else
        {
            Debug.LogWarning("⚠️ TurnManager not found!");
        }
    }
}