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
        }

        if (playerInventory == null)
        {
            playerInventory = GetComponent<Inventory>();
        }
    }

    public void GiveRandomItem()
    {
        if (itemDatabase == null) return;
        if (playerInventory == null) return;

        List<ItemData> availableItems = itemDatabase.GetAvailableItems();

        if (availableItems.Count == 0) return;

        int randomIndex = Random.Range(0, availableItems.Count);
        ItemData randomItem = availableItems[randomIndex];

        if (itemDatabase.TryTakeItem(randomItem))
        {
            playerInventory.AddItem(randomItem);
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
        Debug.Log($"Item selected: {item.itemName} (Type: {item.type})");
    }

    public ItemData GetSelectedItem()
    {
        return selectedItem;
    }

    public void ClearSelectedItem()
    {
        selectedItem = null;
    }

    public void PlaceItem(GardenTile targetTile)
    {
        if (playerInventory == null) return;
        if (selectedItem == null) return;
        if (targetTile.occupied) return;
        if (!playerInventory.HasItem(selectedItem)) return;

        if (IsItemBlocked(selectedItem))
        {
            Debug.Log($"🚫 Cannot place {selectedItem.itemName} - {selectedItem.type} is blocked this round!");
            return;
        }

        if (turnManager == null)
        {
            turnManager = FindFirstObjectByType<TurnManager>();
        }

        if (turnManager != null && turnManager.hasPlacedItemThisTurn) return;

        // Gebruik ItemPlacer
        playerInventory.RemoveItem(selectedItem);

        if (!ItemPlacer.PlaceItemOnTile(targetTile, selectedItem, false))
        {
            // Als plaatsen mislukt, voeg item terug
            playerInventory.AddItem(selectedItem);
            return;
        }

        if (turnManager != null)
        {
            turnManager.hasPlacedItemThisTurn = true;
            turnManager.UpdateActionButtons();
        }

        Debug.Log($"{selectedItem.itemName} placed on tile by {gameObject.name}");

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
        if (playerInventory == null) return;
        if (selectedItem == null) return;
        if (targetTile.occupied) return;
        if (!playerInventory.HasItem(selectedItem)) return;

        playerInventory.RemoveItem(selectedItem);

        if (!ItemPlacer.PlaceItemOnTile(targetTile, selectedItem, false))
        {
            playerInventory.AddItem(selectedItem);
            return;
        }

        Debug.Log($"{selectedItem.itemName} placed on tile (PlaceAnywhere) by {gameObject.name}");

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
        if (playerInventory == null) return;
        if (selectedItem == null) return;
        if (selectedItem.type != ItemType.Sabotage) return;
        if (!targetTile.occupied || targetTile.placedItemData == null) return;
        if (targetTile.isProtected) return;

        ItemPlacer.RemoveItemFromTile(targetTile);

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

        Debug.Log($"{gameObject.name} removed an item from the garden");
    }

    public void PassTurn()
    {
        TurnManager turnManager = FindFirstObjectByType<TurnManager>();
        if (turnManager != null)
        {
            GiveRandomItem();
            Debug.Log($"{gameObject.name} passed and received a random item");

            InventoryUI ui = FindFirstObjectByType<InventoryUI>();
            if (ui != null)
            {
                ui.ClearSelectedItem();
                ui.RefreshUI();
            }

            turnManager.EndTurn();
        }
    }

    bool IsItemBlocked(ItemData item)
    {
        if (turnManager == null) return false;

        Player currentPlayer = turnManager.GetCurrentPlayer();
        if (currentPlayer == null) return false;

        //Debug.Log($"🔍🔍🔍 Checking: item={item.itemName}, type={item.type}, blockedType={currentPlayer.blockedItemType}, rounds={currentPlayer.blockRoundsRemaining}");

        return currentPlayer.blockRoundsRemaining > 0 && currentPlayer.blockedItemType == item.type;
    }
}