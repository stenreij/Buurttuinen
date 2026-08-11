using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    public Inventory playerInventory;
    public GameObject itemButtonPrefab;
    public Transform contentParent;
    private ItemData selectedItem;

    public void RefreshUI()
    {
        UpdateUI();

        if (selectedItem != null)
        {
            UpdateItemSelection(selectedItem);
        }
    }

    void UpdateUI()
    {
        if (!ValidateReferences()) return;

        ClearOldButtons();

        if (!HasItems()) return;

        Dictionary<ItemData, int> itemCounts = GetItemCounts();

        CreateButtons(itemCounts);
    }

    // VALIDATE REFERENCES
    bool ValidateReferences()
    {
        if (contentParent == null)
        {
            Debug.LogError("❌ Content Parent is NOT assigned!");
            return false;
        }

        if (playerInventory == null)
        {
            Debug.LogError("❌ No Inventory linked to InventoryUI!");
            return false;
        }

        if (itemButtonPrefab == null)
        {
            Debug.LogError("❌ Item Button Prefab is NOT assigned!");
            return false;
        }

        return true;
    }

    // CLEAR OLD BUTTONS
    void ClearOldButtons()
    {
        int childCount = contentParent.childCount;
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }

    // CHECK IF INVENTORY HAS ITEMS
    bool HasItems()
    {
        int totalItems = playerInventory.items.Count;

        if (totalItems == 0)
        {
            Debug.LogWarning("⚠️ Inventory is empty! No buttons will be created.");
            return false;
        }
        return true;
    }

    // GET ITEM COUNTS
    Dictionary<ItemData, int> GetItemCounts()
    {
        Dictionary<ItemData, int> itemCounts = new Dictionary<ItemData, int>();
        foreach (ItemData item in playerInventory.items)
        {
            if (itemCounts.ContainsKey(item))
                itemCounts[item]++;
            else
                itemCounts[item] = 1;
        }

        return itemCounts;
    }

    // CREATE BUTTONS
    void CreateButtons(Dictionary<ItemData, int> itemCounts)
    {
        int buttonIndex = 0;
        foreach (KeyValuePair<ItemData, int> entry in itemCounts)
        {
            ItemData item = entry.Key;
            int count = entry.Value;

            GameObject newButton = Instantiate(itemButtonPrefab, contentParent);
            newButton.name = item.itemName;

            Debug.Log($"✅ Created button for: {item.itemName} with count: {count}");

            SetButtonIcon(newButton, item);
            SetButtonCountText(newButton, count);
            SetButtonClickEvent(newButton, item);

            buttonIndex++;
        }
    }

    // SET BUTTON ICON
    void SetButtonIcon(GameObject button, ItemData item)
    {
        Image iconImage = button.GetComponent<Image>();
        if (iconImage != null)
        {
            if (item.icon != null)
            {
                iconImage.sprite = item.icon;
            }
            else
            {
                Debug.LogWarning($"⚠️ No icon found for: {item.itemName}");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ No Image component on ItemButton prefab!");
        }
    }

    // SET BUTTON COUNT TEXT
    void SetButtonCountText(GameObject button, int count)
    {
        Text countText = button.GetComponentInChildren<Text>();
        if (countText != null)
        {
            if (count > 1)
            {
                countText.text = count + "x";
            }
            else
            {
                countText.text = "";
            }
        }
        else
        {
            Debug.LogError($"❌ No Text component found on ItemButton prefab!");
        }
    }

    // SET BUTTON CLICK EVENT
    void SetButtonClickEvent(GameObject button, ItemData item)
    {
        Button btn = button.GetComponent<Button>();
        if (btn != null)
        {
            ItemData capturedItem = item;
            btn.onClick.AddListener(() => OnItemClicked(capturedItem));
        }
    }

    // ON ITEM CLICKED
    void OnItemClicked(ItemData item)
    {
        Debug.Log($"🖱️ CLICKED ON: {item.itemName}");
        Debug.Log($"🔍 item.type = {item.type}");

        TurnManager turnManager = FindFirstObjectByType<TurnManager>();
        if (turnManager == null)
        {
            Debug.Log("⚠️ TurnManager not found!");
            return;
        }

        if (turnManager.hasPlacedItemThisTurn)
        {
            Debug.Log("⚠️ Je hebt deze beurt al een actie gedaan! Je kunt geen items meer selecteren.");
            return;
        }

        Player currentPlayer = turnManager.GetCurrentPlayer();
        if (currentPlayer == null)
        {
            Debug.Log("⚠️ No current player!");
            return;
        }

        selectedItem = item;

        UpdateItemSelection(item);

        if (item.type == ItemType.PowerUp)
        {
            Debug.Log($"🔍 PowerUp detected! Looking for PowerUpActions...");

            PowerUpActions powerActions = currentPlayer.GetComponent<PowerUpActions>();
            if (powerActions != null)
            {
                Debug.Log($"✅ PowerUpActions found!");
                powerActions.SelectPowerUp(item);
                Debug.Log($"⚡ Power-up geselecteerd: {item.itemName}");

                turnManager.UpdateActionButtons();
            }
            else
            {
                Debug.Log($"❌ PowerUpActions NOT found on {currentPlayer.gameObject.name}!");
            }
            return;
        }

        Debug.Log($"🔍 Normaal item detected, using ItemActions");
        ItemActions actions = currentPlayer.GetComponent<ItemActions>();
        if (actions == null)
        {
            Debug.Log("⚠️ No ItemActions found!");
            return;
        }

        actions.SelectItem(item);
    }

    public ItemData GetSelectedItem()
    {
        return selectedItem;
    }

    private void UpdateItemSelection(ItemData selectedItem)
    {
        foreach (Transform child in contentParent)
        {
            if (child.name == selectedItem.itemName)
            {
                child.localScale = new Vector3(1.15f, 1.15f, 1f);
            }
            else
            {
                child.localScale = new Vector3(1f, 1f, 1f);
            }
        }
    }

    public void ClearSelectedItem()
    {
        selectedItem = null;
    }
}