using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    public Inventory playerInventory;
    public GameObject itemButtonPrefab;
    public Transform contentParent;

    public void RefreshUI()
    {
        Debug.Log("🔄 RefreshUI() called!");
        UpdateUI();
    }

    void UpdateUI()
    {
        Debug.Log("=== UPDATE UI STARTED ===");

        if (!ValidateReferences()) return;

        ClearOldButtons();

        if (!HasItems()) return;

        Dictionary<ItemData, int> itemCounts = GetItemCounts();

        CreateButtons(itemCounts);

        Debug.Log("✅ UpdateUI() completed! " + itemCounts.Count + " buttons created.");
        Debug.Log("=== END UPDATE UI ===");
    }

    // ✅ VALIDATE REFERENCES
    bool ValidateReferences()
    {
        if (contentParent == null)
        {
            Debug.LogError("❌ Content Parent is NOT assigned!");
            return false;
        }
        Debug.Log("✅ Content Parent is assigned: " + contentParent.name);

        if (playerInventory == null)
        {
            Debug.LogError("❌ No Inventory linked to InventoryUI!");
            return false;
        }
        Debug.Log("✅ Inventory is linked: " + playerInventory.gameObject.name);

        if (itemButtonPrefab == null)
        {
            Debug.LogError("❌ Item Button Prefab is NOT assigned!");
            return false;
        }

        return true;
    }

    // ✅ CLEAR OLD BUTTONS
    void ClearOldButtons()
    {
        int childCount = contentParent.childCount;
        Debug.Log("🗑️ Removing " + childCount + " old buttons...");
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }

    // ✅ CHECK IF INVENTORY HAS ITEMS
    bool HasItems()
    {
        int totalItems = playerInventory.items.Count;
        Debug.Log("📦 Total items in inventory: " + totalItems);

        if (totalItems == 0)
        {
            Debug.LogWarning("⚠️ Inventory is empty! No buttons will be created.");
            return false;
        }
        return true;
    }

    // ✅ GET ITEM COUNTS
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

        Debug.Log("📊 Count unique items: " + itemCounts.Count);
        return itemCounts;
    }

    // ✅ CREATE BUTTONS
    void CreateButtons(Dictionary<ItemData, int> itemCounts)
    {
        int buttonIndex = 0;
        foreach (KeyValuePair<ItemData, int> entry in itemCounts)
        {
            ItemData item = entry.Key;
            int count = entry.Value;

            Debug.Log($"🔄 Button {buttonIndex + 1}: {item.itemName} ({count}x)");

            GameObject newButton = Instantiate(itemButtonPrefab, contentParent);
            Debug.Log($"✅ Button created for: {item.itemName}");

            SetButtonIcon(newButton, item);
            SetButtonCountText(newButton, count);
            SetButtonClickEvent(newButton, item);

            buttonIndex++;
        }
    }

    // ✅ SET BUTTON ICON
    void SetButtonIcon(GameObject button, ItemData item)
    {
        Image iconImage = button.GetComponent<Image>();
        if (iconImage != null)
        {
            if (item.icon != null)
            {
                iconImage.sprite = item.icon;
                Debug.Log($"🖼️ Icon added for: {item.itemName}");
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

    // ✅ SET BUTTON COUNT TEXT
    void SetButtonCountText(GameObject button, int count)
    {
        Text countText = button.GetComponentInChildren<Text>();
        if (countText != null)
        {
            if (count > 1)
            {
                countText.text = count + "x";
                Debug.Log($"🔢 Count set: {countText.text}");
            }
            else
            {
                countText.text = "";
                Debug.Log($"🔢 No count (1x) - text empty");
            }
        }
        else
        {
            Debug.LogError($"❌ No Text component found on ItemButton prefab!");
        }
    }

    // ✅ SET BUTTON CLICK EVENT
    void SetButtonClickEvent(GameObject button, ItemData item)
    {
        Button btn = button.GetComponent<Button>();
        if (btn != null)
        {
            ItemData capturedItem = item;
            btn.onClick.AddListener(() => OnItemClicked(capturedItem));
            Debug.Log($"🖱️ Click event added for: {item.itemName}");
        }
    }

    // ✅ ON ITEM CLICKED
    void OnItemClicked(ItemData item)
    {
        Debug.Log($"🖱️ CLICKED ON: {item.itemName}");
    }
}