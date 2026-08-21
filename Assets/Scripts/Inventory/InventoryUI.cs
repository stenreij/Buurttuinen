using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    public Inventory playerInventory;
    public GameObject itemButtonPrefab;
    public Transform contentParent;
    private ItemData selectedItem;
    public TooltipManager tooltipManager;

    [Header("Selection Visuals")]
    public Color selectedColor = new Color(1f, 1f, 0.5f, 1f);
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

    bool ValidateReferences()
    {
        if (contentParent == null)
        {
            Debug.LogError("Content Parent is not assigned!");
            return false;
        }

        if (playerInventory == null)
        {
            Debug.LogError("No Inventory linked to InventoryUI!");
            return false;
        }

        if (itemButtonPrefab == null)
        {
            Debug.LogError("Item Button Prefab is not assigned!");
            return false;
        }

        return true;
    }

    void ClearOldButtons()
    {
        int childCount = contentParent.childCount;
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }

    bool HasItems()
    {
        int totalItems = playerInventory.items.Count;

        if (totalItems == 0)
        {
            return false;
        }
        return true;
    }

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

            EnsureButtonHasBackground(newButton);
            AddHoverEvents(newButton, item);

            buttonIndex++;
        }
    }

    void AddHoverEvents(GameObject buttonObj, ItemData item)
    {
        EventTrigger trigger = buttonObj.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = buttonObj.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) =>
        {
            ShowInventoryTooltip(item);
        });
        trigger.triggers.Add(enterEntry);

        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) =>
        {
            HideInventoryTooltip();
        });
        trigger.triggers.Add(exitEntry);
    }

    void ShowInventoryTooltip(ItemData item)
    {
        if (tooltipManager == null || item == null) return;

        if (item.type == ItemType.PowerUp)
        {
            tooltipManager.ShowInventoryTooltip(
                item.itemName,
                "PowerUp",
                isPowerUp: true
            );
            return;
        }

        if (item.type == ItemType.Sabotage)
        {
            tooltipManager.ShowInventoryTooltip(
                item.itemName,
                item.type.ToString(),
                isSabotage: true
            );
            return;
        }

        bool isObstacle = (item.type == ItemType.Obstacle);

        tooltipManager.ShowInventoryTooltip(
            item.itemName,
            item.type.ToString(),
            item.score,
            item.water,
            item.soilHealth,
            item.biodiversity,
            item.aesthetic,
            isObstacle: isObstacle
        );
    }

    void HideInventoryTooltip()
    {
        if (tooltipManager != null)
        {
            tooltipManager.HideTooltip();
        }
    }

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
        }
    }

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
    }

    void SetButtonClickEvent(GameObject button, ItemData item)
    {
        Button btn = button.GetComponent<Button>();
        if (btn != null)
        {
            ItemData capturedItem = item;
            btn.onClick.AddListener(() => OnItemClicked(capturedItem));
        }
    }

    void OnItemClicked(ItemData item)
    {
        TurnManager turnManager = FindFirstObjectByType<TurnManager>();
        if (turnManager == null) return;

        if (turnManager.IsGamePaused()) return;

        if (turnManager.hasPlacedItemThisTurn) return;

        Player currentPlayer = turnManager.GetCurrentPlayer();
        if (currentPlayer == null) return;

        if (selectedItem == item)
        {
            selectedItem = null;

            ItemActions actions = currentPlayer.GetComponent<ItemActions>();
            if (actions != null) actions.ClearSelectedItem();

            PowerUpActions powerActions = currentPlayer.GetComponent<PowerUpActions>();
            if (powerActions != null) powerActions.ClearSelectedPowerUp();

            UpdateUI();
            turnManager.UpdateActionButtons();
            return;
        }

        selectedItem = item;
        UpdateItemSelection(item);

        if (item.type == ItemType.PowerUp)
        {
            PowerUpActions powerActions = currentPlayer.GetComponent<PowerUpActions>();
            if (powerActions != null)
            {
                powerActions.SelectPowerUp(item);
                Debug.Log($"Power-up selected: {item.itemName}");
            }
        }
        else if (item.type == ItemType.Sabotage)
        {
            ItemActions actions = currentPlayer.GetComponent<ItemActions>();
            if (actions != null)
            {
                actions.SelectItem(item);
                Debug.Log($"Sabotage selected: {item.itemName}");
            }
        }
        else
        {
            ItemActions actions = currentPlayer.GetComponent<ItemActions>();
            if (actions == null) return;

            actions.SelectItem(item);
            Debug.Log($"Item selected: {item.itemName}");
        }

        turnManager.UpdateActionButtons();
    }

    private void UpdateItemSelection(ItemData selectedItem)
    {
        foreach (Transform child in contentParent)
        {
            Image img = child.GetComponent<Image>();
            if (img != null)
            {
                if (child.name == selectedItem.itemName)
                {
                    img.color = selectedColor;
                    child.localScale = new Vector3(selectedScale, selectedScale, 1f);
                }
                else
                {
                    img.color = defaultColor;
                    child.localScale = new Vector3(defaultScale, defaultScale, 1f);
                }
            }
        }
    }

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