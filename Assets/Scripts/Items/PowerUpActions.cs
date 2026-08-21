using UnityEngine;
using System.Collections.Generic;

public class PowerUpActions : MonoBehaviour
{
    [Header("References")]
    public Inventory playerInventory;
    public ItemDatabase itemDatabase;
    public TurnManager turnManager;

    private ItemData selectedPowerUp;
    private bool isWaitingForTile = false;
    private string pendingPowerUpName = "";
    public Sprite shieldSprite;

    void Start()
    {
        if (turnManager == null)
            turnManager = FindFirstObjectByType<TurnManager>();

        if (playerInventory == null)
            playerInventory = GetComponent<Inventory>();
    }

    public void SelectPowerUp(ItemData powerUpItem)
    {
        if (powerUpItem.type != ItemType.PowerUp) return;

        selectedPowerUp = powerUpItem;
        Debug.Log($"Power-up selected: {powerUpItem.itemName}");
    }

    public ItemData GetSelectedPowerUp()
    {
        return selectedPowerUp;
    }

    public void ClearSelectedPowerUp()
    {
        selectedPowerUp = null;
    }

    public void UseExtraItem()
    {
        if (!ValidatePowerUp("Extra Item")) return;

        GiveRandomItem();
        Debug.Log($"{gameObject.name} used Extra Item and received a new item");

        ConsumePowerUp();
    }

    public void UseSoilBoost()
    {
        if (!ValidatePowerUp("Soil")) return;

        Player currentPlayer = turnManager.GetCurrentPlayer();
        if (currentPlayer == null) return;

        currentPlayer.soilBoost += 2;
        Debug.Log($"Soil boost activated! +{currentPlayer.soilBoost} points per item");

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

        ConsumePowerUp();
    }

    private bool ValidatePowerUp(string expectedName)
    {
        if (selectedPowerUp == null) return false;
        if (selectedPowerUp.itemName != expectedName) return false;
        return true;
    }

    private void ConsumePowerUp()
    {
        if (selectedPowerUp == null) return;

        playerInventory.RemoveItem(selectedPowerUp);
        ClearSelectedPowerUp();

        InventoryUI ui = FindFirstObjectByType<InventoryUI>();
        if (ui != null)
        {
            ui.ClearSelectedItem();
            ui.RefreshUI();
        }
    }

    private void GiveRandomItem()
    {
        if (playerInventory == null || itemDatabase == null) return;

        List<ItemData> availableItems = itemDatabase.GetAvailableItems();
        if (availableItems.Count == 0) return;

        int randomIndex = Random.Range(0, availableItems.Count);
        ItemData randomItem = availableItems[randomIndex];

        if (itemDatabase.TryTakeItem(randomItem))
        {
            playerInventory.AddItem(randomItem);
        }
    }

    private void PlaceRandomItemOnTile(GardenTile targetTile)
    {
        if (itemDatabase == null) return;

        List<ItemData> availableItems = itemDatabase.GetAvailableItems();
        if (availableItems.Count == 0) return;

        int randomIndex = Random.Range(0, availableItems.Count);
        ItemData randomItem = availableItems[randomIndex];

        if (itemDatabase.TryTakeItem(randomItem))
        {
            GameObject placed = Instantiate(randomItem.prefab, targetTile.transform.position, Quaternion.identity);
            placed.transform.parent = targetTile.transform;
            placed.transform.localPosition = Vector3.zero;

            SpriteRenderer sr = placed.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = true;
                sr.sortingOrder = 10;
            }

            targetTile.placedItem = placed;
            targetTile.placedItemData = randomItem;
            targetTile.occupied = true;

            Debug.Log($"{randomItem.itemName} placed on {targetTile.name}");
        }
    }

    public void ExecuteSelectedPowerUp()
    {
        if (selectedPowerUp == null) return;

        switch (selectedPowerUp.itemName)
        {
            case "Extra Item":
                UseExtraItem();
                break;
            case "Place Anywhere":
                isWaitingForTile = true;
                pendingPowerUpName = "Place Anywhere";
                Debug.Log("Click on an empty tile to use Place Anywhere");
                break;
            case "Swap Items":
                Debug.Log("Swap Items: Click on two tiles to swap");
                break;
            case "Protection":
                isWaitingForTile = true;
                pendingPowerUpName = "Protection";
                Debug.Log("Click on an item to protect it");
                break;
            case "Soil":
                UseSoilBoost();
                break;
            case "Double Placing":
                Debug.Log("Double Placing activated - you can place two items this turn");
                break;
            default:
                Debug.Log($"Unknown power-up: {selectedPowerUp.itemName}");
                break;
        }
    }

    public void UsePlaceAnywhere(GardenTile targetTile)
    {
        if (!ValidatePowerUp("Place Anywhere")) return;
        if (targetTile == null || targetTile.occupied) return;

        ItemActions actions = GetComponent<ItemActions>();
        if (actions != null)
        {
            ItemData selectedItem = actions.GetSelectedItem();
            if (selectedItem == null) return;
            actions.PlaceItemDirect(targetTile);
        }

        if (turnManager != null)
        {
            turnManager.hasPlacedItemThisTurn = true;
            turnManager.UpdateActionButtons();
        }

        ConsumePowerUp();
    }

    public void UseProtection(GardenTile targetTile)
    {
        if (!ValidatePowerUp("Protection")) return;
        if (targetTile == null || !targetTile.occupied) return;

        targetTile.isProtected = true;
        Debug.Log($"{targetTile.placedItemData.itemName} is now protected");

        AddProtectionVisual(targetTile);

        if (turnManager != null)
        {
            turnManager.hasPlacedItemThisTurn = true;
            turnManager.UpdateActionButtons();
        }

        ConsumePowerUp();
    }

    private void AddProtectionVisual(GardenTile targetTile)
    {
        if (targetTile.placedItem == null) return;

        GameObject shield = new GameObject("ShieldIndicator");
        shield.transform.parent = targetTile.placedItem.transform;
        shield.transform.localPosition = new Vector3(-0.6f, 0.6f, 0f);

        SpriteRenderer sr = shield.AddComponent<SpriteRenderer>();

        if (shieldSprite != null)
        {
            sr.sprite = shieldSprite;
        }
        else
        {
            Texture2D tex = new Texture2D(64, 64);
            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    tex.SetPixel(x, y, Color.white);
                }
            }
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        }

        sr.sortingOrder = 20;
    }

    public bool IsWaitingForTile()
    {
        return isWaitingForTile;
    }

    public void ExecutePendingPowerUp(GardenTile targetTile)
    {
        if (!isWaitingForTile) return;

        switch (pendingPowerUpName)
        {
            case "Place Anywhere":
                UsePlaceAnywhere(targetTile);
                break;
            case "Protection":
                UseProtection(targetTile);
                break;
            case "Swap Items":
                break;
        }

        isWaitingForTile = false;
        pendingPowerUpName = "";
    }
}