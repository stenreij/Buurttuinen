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
        if (powerUpItem.type != ItemType.PowerUp)
        {
            Debug.Log("⚠️ Dit is geen power-up!");
            return;
        }

        selectedPowerUp = powerUpItem;
        Debug.Log($"⚡ Power-up geselecteerd: {powerUpItem.itemName}");
    }

    public ItemData GetSelectedPowerUp()
    {
        return selectedPowerUp;
    }

    public void ClearSelectedPowerUp()
    {
        selectedPowerUp = null;
        Debug.Log($"🧹 Cleared selected power-up");
    }

    public void UseExtraItem()
    {
        if (!ValidatePowerUp("Extra Item")) return;

        GiveRandomItem();
        Debug.Log($"🎁 {gameObject.name} gebruikt Extra Item en kreeg een nieuw item!");

        ConsumePowerUp();
    }

    public void UseSoilBoost()
    {
        if (!ValidatePowerUp("Soil")) return;

        Player currentPlayer = turnManager.GetCurrentPlayer();
        if (currentPlayer == null) return;

        currentPlayer.soilBoost += 2;
        Debug.Log($"🌱 Soil boost geactiveerd! +{currentPlayer.soilBoost} punten per item.");

        if (turnManager != null)
        {
            turnManager.hasPlacedItemThisTurn = true;
            turnManager.UpdateActionButtons();
            Debug.Log($"⏹️ Actie uitgevoerd! Geen andere acties meer deze beurt.");
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
        if (selectedPowerUp == null)
        {
            Debug.Log("⚠️ Geen power-up geselecteerd!");
            return false;
        }

        if (selectedPowerUp.itemName != expectedName)
        {
            Debug.Log($"⚠️ Dit is geen {expectedName} power-up!");
            return false;
        }

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
        if (availableItems.Count == 0)
        {
            Debug.Log("⚠️ Geen items meer beschikbaar!");
            return;
        }

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
        if (availableItems.Count == 0)
        {
            Debug.Log("⚠️ Geen items meer beschikbaar!");
            return;
        }

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

            Debug.Log($"✅ {randomItem.itemName} geplaatst op {targetTile.name}!");
        }
    }

    public void ExecuteSelectedPowerUp()
    {
        if (selectedPowerUp == null)
        {
            Debug.Log("⚠️ Geen power-up geselecteerd!");
            return;
        }

        switch (selectedPowerUp.itemName)
        {
            case "Extra Item":
                Debug.Log("🎁 Extra Item gebruikt! Je krijgt een nieuw item.");
                break;
            case "Place Anywhere":
                isWaitingForTile = true;
                pendingPowerUpName = "Place Anywhere";
                Debug.Log("📍 Klik op een lege tile om PlaceAnywhere te gebruiken!");
                break;
            case "Swap Items":
                Debug.Log("🔄 Klik op twee tiles om te wisselen!");
                break;
            case "Protection":
                isWaitingForTile = true;
                pendingPowerUpName = "Protection";
                Debug.Log("🛡️ Klik op een item om te beschermen!");
                break;
            case "Soil":
                UseSoilBoost();
                break;
            case "Double Placing":
                Debug.Log("⚡ Double Placing gebruikt! Je mag twee items plaatsen deze beurt.");
                break;
            default:
                Debug.Log($"⚠️ Onbekende power-up: {selectedPowerUp.itemName}");
                break;
        }
    }

    public void UsePlaceAnywhere(GardenTile targetTile)
    {
        if (!ValidatePowerUp("Place Anywhere")) return;

        if (targetTile == null || targetTile.occupied)
        {
            Debug.Log("⚠️ Kies een lege tile om een item te plaatsen!");
            return;
        }

        ItemActions actions = GetComponent<ItemActions>();
        if (actions != null)
        {
            ItemData selectedItem = actions.GetSelectedItem();
            if (selectedItem == null)
            {
                Debug.Log("⚠️ Selecteer eerst een item uit je inventory om te plaatsen!");
                return;
            }
            actions.PlaceItemDirect(targetTile);
        }

        if (turnManager != null)
        {
            turnManager.hasPlacedItemThisTurn = true;
            turnManager.UpdateActionButtons();
            Debug.Log($"⏹️ PlaceAnywhere gebruikt! Geen andere acties meer deze beurt.");
        }

        ConsumePowerUp();
    }

    public void UseProtection(GardenTile targetTile)
    {
        if (!ValidatePowerUp("Protection")) return;

        if (targetTile == null || !targetTile.occupied)
        {
            Debug.Log("⚠️ Kies een geplaatst item om te beschermen!");
            return;
        }

        targetTile.isProtected = true;
        Debug.Log($"🛡️ {targetTile.placedItemData.itemName} is beschermd!");

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

        Debug.Log($"🛡️ Shield indicator toegevoegd aan {targetTile.placedItem.name}");
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