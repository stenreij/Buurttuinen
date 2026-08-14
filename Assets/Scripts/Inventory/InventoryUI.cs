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

    // Kleur voor selectie indicator
    [Header("Selection Visuals")]
    public Color selectedColor = new Color(1f, 1f, 0.5f, 1f);  // Geelachtig
    public Color defaultColor = Color.white;
    public float selectedScale = 1.15f;
    public float defaultScale = 1f;

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

            SetButtonIcon(newButton, item);
            SetButtonCountText(newButton, count);
            SetButtonClickEvent(newButton, item);

            // 🔥 NIEUW: Voeg een background image toe voor selectie feedback als die er niet is
            EnsureButtonHasBackground(newButton);

            buttonIndex++;
        }
    }

    // 🔥 NIEUW: Zorg dat de button een background Image heeft voor selectie
    void EnsureButtonHasBackground(GameObject button)
    {
        Image img = button.GetComponent<Image>();
        if (img == null)
        {
            img = button.AddComponent<Image>();
            img.color = defaultColor;
            img.raycastTarget = true;
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
                iconImage.color = defaultColor;
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

    // 🔥 AANGEPAST: ON ITEM CLICKED - Nu met betere feedback voor alle items
    void OnItemClicked(ItemData item)
    {
        Debug.Log($"🖱️ CLICKED ON: {item.itemName} (Type: {item.type})");

        TurnManager turnManager = FindFirstObjectByType<TurnManager>();
        if (turnManager == null)
        {
            Debug.Log("⚠️ TurnManager not found!");
            return;
        }

        if (turnManager.IsGamePaused())
        {
            Debug.Log("⏸️ Game is gepauzeerd (weer), je kunt geen items selecteren!");
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

        // 🔥 NIEUW: Als we op hetzelfde item klikken, deselecteer het dan
        if (selectedItem == item)
        {
            Debug.Log($"🔓 Deselecting: {item.itemName}");
            selectedItem = null;
            
            // Clear selection in actions
            ItemActions actions = currentPlayer.GetComponent<ItemActions>();
            if (actions != null) actions.ClearSelectedItem();
            
            PowerUpActions powerActions = currentPlayer.GetComponent<PowerUpActions>();
            if (powerActions != null) powerActions.ClearSelectedPowerUp();
            
            UpdateUI();
            turnManager.UpdateActionButtons();
            return;
        }

        // 🔥 Selecteer het item
        selectedItem = item;

        // Update de visuele selectie
        UpdateItemSelection(item);

        // 🔥 Verwerk op basis van item type
        if (item.type == ItemType.PowerUp)
        {
            Debug.Log($"⚡ PowerUp detected! Selecting power-up...");
            
            PowerUpActions powerActions = currentPlayer.GetComponent<PowerUpActions>();
            if (powerActions != null)
            {
                powerActions.SelectPowerUp(item);
                Debug.Log($"⚡ Power-up geselecteerd: {item.itemName}");
            }
            else
            {
                Debug.Log($"❌ PowerUpActions NOT found on {currentPlayer.gameObject.name}!");
            }
        }
        else if (item.type == ItemType.Sabotage)
        {
            Debug.Log($"🔧 Sabotage item detected! Selecting sabotage...");
            
            ItemActions actions = currentPlayer.GetComponent<ItemActions>();
            if (actions != null)
            {
                actions.SelectItem(item);
                Debug.Log($"🔧 Sabotage item geselecteerd: {item.itemName}");
            }
            else
            {
                Debug.Log($"❌ ItemActions NOT found on {currentPlayer.gameObject.name}!");
            }
        }
        else
        {
            Debug.Log($"🌱 Normal item detected, using ItemActions");
            ItemActions actions = currentPlayer.GetComponent<ItemActions>();
            if (actions == null)
            {
                Debug.Log("⚠️ No ItemActions found!");
                return;
            }

            actions.SelectItem(item);
        }

        turnManager.UpdateActionButtons();
    }

    // 🔥 VERBETERD: Update item selection met kleur EN schaal
    private void UpdateItemSelection(ItemData selectedItem)
    {
        foreach (Transform child in contentParent)
        {
            Image img = child.GetComponent<Image>();
            if (img != null)
            {
                if (child.name == selectedItem.itemName)
                {
                    // Geselecteerd: gele highlight + groter
                    img.color = selectedColor;
                    child.localScale = new Vector3(selectedScale, selectedScale, 1f);
                }
                else
                {
                    // Niet geselecteerd: normale kleur + normale grootte
                    img.color = defaultColor;
                    child.localScale = new Vector3(defaultScale, defaultScale, 1f);
                }
            }
        }
    }

    // 🔥 NIEUW: Reset alle selectie visuals
    public void ClearAllSelections()
    {
        selectedItem = null;
        foreach (Transform child in contentParent)
        {
            Image img = child.GetComponent<Image>();
            if (img != null)
            {
                img.color = defaultColor;
                child.localScale = new Vector3(defaultScale, defaultScale, 1f);
            }
        }
    }

    public ItemData GetSelectedItem()
    {
        return selectedItem;
    }

    public void ClearSelectedItem()
    {
        selectedItem = null;
        UpdateUI();
    }
}