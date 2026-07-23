using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public List<Player> players;
    public InventoryUI inventoryUI;

    void Start()
    {
        StartGame();
    }

    void StartGame()
    {
        Debug.Log("🎮 GAME STARTING...");

        // 1. Give each player their starting items
        foreach (Player player in players)
        {
            ItemActions actions = player.GetComponent<ItemActions>();
            if (actions != null)
            {
                for (int i = 0; i < actions.startItemCount; i++)
                {
                    actions.GiveRandomItem();
                }

                // 📦 Log all items a player received
                Inventory inv = player.GetComponent<Inventory>();
                if (inv != null && inv.items.Count > 0)
                {
                    string itemList = "";
                    foreach (ItemData item in inv.items)
                    {
                        itemList += item.itemName + ", ";
                    }
                    itemList = itemList.TrimEnd(',', ' ');
                    Debug.Log($"📦 {player.gameObject.name} received: {itemList}");
                }
            }
            else
            {
                Debug.LogError($"❌ No ItemActions found on {player.gameObject.name}!");
            }
        }

        // 2. Update the UI after all items are added
        if (inventoryUI != null)
        {
            inventoryUI.RefreshUI();
        }
        else
        {
            Debug.LogWarning("⚠️ No InventoryUI found in GameManager");
        }

        Debug.Log("🎮 GAME READY!");
    }
}