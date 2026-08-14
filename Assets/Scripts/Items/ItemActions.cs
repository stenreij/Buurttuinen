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
        {
            turnManager = FindFirstObjectByType<TurnManager>();
            if (turnManager == null)
            {
                Debug.LogWarning("⚠️ TurnManager not found in ItemActions!");
            }
        }

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

        List<ItemData> availableItems = itemDatabase.GetAvailableItems();

        if (availableItems.Count == 0)
        {
            Debug.LogWarning("⚠️ No more items available for " + gameObject.name);
            return;
        }

        int randomIndex = Random.Range(0, availableItems.Count);
        ItemData randomItem = availableItems[randomIndex];

        if (itemDatabase.TryTakeItem(randomItem))
        {
            playerInventory.AddItem(randomItem);
        }
        else
        {
            Debug.LogWarning($"⚠️ Could not take {randomItem.itemName} from pool!");
        }
    }

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

    public void SelectItem(ItemData item)
    {
        selectedItem = item;
        Debug.Log($"✅ Item selected: {item.itemName} (Type: {item.type})");
    }

    public ItemData GetSelectedItem()
    {
        return selectedItem;
    }

    public void ClearSelectedItem()
    {
        selectedItem = null;
        Debug.Log($"🧹 Cleared selected item");
    }

    public void PlaceItem(GardenTile targetTile)
    {
        if (playerInventory == null)
        {
            Debug.LogError($"❌ PlayerInventory is NULL on {gameObject.name}!");
            return;
        }

        if (selectedItem == null)
        {
            Debug.Log("⚠️ No item selected!");
            return;
        }

        if (targetTile.occupied)
        {
            Debug.Log("⚠️ This tile is already occupied!");
            return;
        }

        if (!playerInventory.HasItem(selectedItem))
        {
            Debug.Log($"⚠️ {gameObject.name} does not have {selectedItem.itemName} in inventory!");
            return;
        }

        if (turnManager == null)
        {
            turnManager = FindFirstObjectByType<TurnManager>();
        }

        if (turnManager != null && turnManager.hasPlacedItemThisTurn)
        {
            Debug.Log("⚠️ You have already performed an action this turn!");
            return;
        }

        playerInventory.RemoveItem(selectedItem);

        if (selectedItem.prefab != null)
        {
            GameObject placed = Instantiate(selectedItem.prefab, targetTile.transform.position, Quaternion.identity);
            placed.transform.parent = targetTile.transform;
            placed.transform.localPosition = Vector3.zero;

            SpriteRenderer sr = placed.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = true;
                sr.sortingOrder = 10;
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

        targetTile.occupied = true;

        if (turnManager != null)
        {
            turnManager.hasPlacedItemThisTurn = true;
            turnManager.UpdateActionButtons();
        }

        Debug.Log($"✅ {selectedItem.itemName} placed on tile!");
        ClearSelectedItem();

        ScoreManager.RefreshScores();

        CommunityManager communityManager = FindFirstObjectByType<CommunityManager>();
        if (communityManager != null)
        {
            communityManager.UpdateCommunityGoalScore();
        }

        InventoryUI ui = FindFirstObjectByType<InventoryUI>();
        if (ui != null)
        {
            ui.ClearSelectedItem();
            ui.RefreshUI();
        }
    }

    public void PlaceItemDirect(GardenTile targetTile)
    {
        if (playerInventory == null)
        {
            Debug.LogError($"❌ PlayerInventory is NULL on {gameObject.name}!");
            return;
        }

        if (selectedItem == null)
        {
            Debug.Log("⚠️ No item selected!");
            return;
        }

        if (targetTile.occupied)
        {
            Debug.Log("⚠️ This tile is already occupied!");
            return;
        }

        if (!playerInventory.HasItem(selectedItem))
        {
            Debug.Log($"⚠️ {gameObject.name} does not have {selectedItem.itemName} in inventory!");
            return;
        }

        playerInventory.RemoveItem(selectedItem);

        if (selectedItem.prefab != null)
        {
            GameObject placed = Instantiate(selectedItem.prefab, targetTile.transform.position, Quaternion.identity);
            placed.transform.parent = targetTile.transform;
            placed.transform.localPosition = Vector3.zero;

            SpriteRenderer sr = placed.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = true;
                sr.sortingOrder = 10;
            }

            targetTile.placedItem = placed;
            targetTile.placedItemData = selectedItem;
        }

        targetTile.occupied = true;

        Debug.Log($"✅ {selectedItem.itemName} placed on tile (PlaceAnywhere)!");
        ClearSelectedItem();

        ScoreManager.RefreshScores();

        InventoryUI ui = FindFirstObjectByType<InventoryUI>();
        if (ui != null)
        {
            ui.ClearSelectedItem();
            ui.RefreshUI();
        }
    }

    public void RemoveItem(GardenTile targetTile)
    {
        if (playerInventory == null)
        {
            Debug.LogError($"❌ PlayerInventory is NULL on {gameObject.name}!");
            return;
        }

        if (selectedItem == null)
        {
            Debug.Log("⚠️ No item selected to remove!");
            return;
        }

        if (selectedItem.type != ItemType.Sabotage)
        {
            Debug.Log("⚠️ This is not a removal item!");
            return;
        }

        if (!targetTile.occupied || targetTile.placedItemData == null)
        {
            Debug.Log("⚠️ There is nothing to be removed on this tile!");
            return;
        }

        if (targetTile.isProtected)
        {
            Debug.Log("🛡️ This tile is protected and cannot be removed!");
            return;
        }

        if (targetTile.placedItem != null)
        {
            Destroy(targetTile.placedItem);
        }

        targetTile.occupied = false;
        targetTile.placedItem = null;
        targetTile.placedItemData = null;

        playerInventory.RemoveItem(selectedItem);
        ClearSelectedItem();

        if (turnManager != null)
        {
            turnManager.hasPlacedItemThisTurn = true;
            turnManager.UpdateActionButtons();
        }

        ScoreManager.RefreshScores();

        CommunityManager communityManager = FindFirstObjectByType<CommunityManager>();
        if (communityManager != null)
        {
            communityManager.UpdateCommunityGoalScore();
        }

        InventoryUI ui = FindFirstObjectByType<InventoryUI>();
        if (ui != null)
        {
            ui.ClearSelectedItem();
            ui.RefreshUI();
        }

        Debug.Log($"✅ {gameObject.name} removed an item from the garden!");
    }

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
                ui.ClearSelectedItem();
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