using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    public Inventory playerInventory;
    public GameObject itemButtonPrefab;
    public Transform contentParent;

    void Start()
    {
        Debug.Log("=== INVENTORY UI START ===");
        UpdateUI();
    }

    void UpdateUI()
    {
        Debug.Log("=== UPDATE UI STARTED ===");

        // Check if contentParent is assigned
        if (contentParent == null)
        {
            Debug.LogError("❌ Content Parent is NOT assigned!");
            return;
        }
        else
        {
            Debug.Log("✅ Content Parent is assigned: " + contentParent.name);
        }

        // Remove old buttons
        int childCount = contentParent.childCount;
        Debug.Log("🗑️ Removing " + childCount + " old buttons...");
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Check if there is an Inventory linked
        if (playerInventory == null)
        {
            Debug.LogError("❌ No Inventory linked to InventoryUI!");
            return;
        }
        else
        {
            Debug.Log("✅ Inventory is linked: " + playerInventory.gameObject.name);
        }

        // Check if the Inventory has items
        int totalItems = playerInventory.items.Count;
        Debug.Log("📦 Total items in inventory: " + totalItems);

        if (totalItems == 0)
        {
            Debug.LogWarning("⚠️ Inventory is empty! No buttons will be created.");
            return;
        }

        // Group items by type
        Dictionary<ItemData, int> itemCounts = new Dictionary<ItemData, int>();
        foreach (ItemData item in playerInventory.items)
        {
            if (itemCounts.ContainsKey(item))
                itemCounts[item]++;
            else
                itemCounts[item] = 1;
        }

        Debug.Log("📊 Count unique items: " + itemCounts.Count);

        // Create buttons for each unique item
        int buttonIndex = 0;
        foreach (KeyValuePair<ItemData, int> entry in itemCounts)
        {
            ItemData item = entry.Key;
            int count = entry.Value;

            Debug.Log($"🔄 Button {buttonIndex + 1}: {item.itemName} ({count}x)");

            // Check if the prefab exists
            if (itemButtonPrefab == null)
            {
                Debug.LogError("❌ Item Button Prefab is NOT assigned in the Inspector!");
                return;
            }

            GameObject newButton = Instantiate(itemButtonPrefab, contentParent);
            Debug.Log($"✅ Button created for: {item.itemName}");

            // Icon
            Image iconImage = newButton.GetComponent<Image>();
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
                Debug.LogWarning($"⚠️ No Image component found on ItemButton prefab!");
            }

            // Count Text
            Text countText = newButton.GetComponentInChildren<Text>();
            if (countText != null)
            {
                if (count > 1)
                {
                    countText.text = count + "x";
                    Debug.Log($"🔢 Aantal ingesteld: {countText.text}");
                }
                else
                {
                    countText.text = "";
                    Debug.Log($"🔢 Geen aantal (1x) - tekst leeg");
                }
            }
            else
            {
                Debug.LogError($"❌ Geen Text component gevonden op ItemButton prefab!");
            }

            // Click event
            Button button = newButton.GetComponent<Button>();
            if (button != null)
            {
                ItemData capturedItem = item;
                button.onClick.AddListener(() => OnItemClicked(capturedItem));
                Debug.Log($"🖱️ Added click event for: {item.itemName}");
            }

            buttonIndex++;
        }

        Debug.Log($"✅ UpdateUI() completed! {buttonIndex} buttons created.");
        Debug.Log("=== END UPDATE UI ===");
    }

    void OnItemClicked(ItemData item)
    {
        Debug.Log($"🖱️ CLICKED ON: {item.itemName}");
    }

    public void RefreshUI()
    {
        Debug.Log("🔄 RefreshUI() called!");
        UpdateUI();
    }
}