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
        int playerIndex = 0;
        foreach (Player player in players)
        {
            Debug.Log($"🔍 Checking Player {playerIndex}: {player.gameObject.name}");

            ItemActions actions = player.GetComponent<ItemActions>();
            if (actions != null)
            {
                Debug.Log($"✅ ItemActions found on {player.gameObject.name}");
                Debug.Log($"🔍 StartItemCount = {actions.startItemCount}");

                for (int i = 0; i < actions.startItemCount; i++)
                {
                    Debug.Log($"🔄 Calling GiveRandomItem() {i+1}/{actions.startItemCount} for {player.gameObject.name}");
                    actions.GiveRandomItem();
                }
                Debug.Log("✅ Items given to " + player.gameObject.name);
            }
            else
            {
                Debug.LogError($"❌ No ItemActions found on {player.gameObject.name}!");
            }
            playerIndex++;
        }

        // 2. Update the UI after all items are added
        if (inventoryUI != null)
        {
            inventoryUI.RefreshUI();
            Debug.Log("🔄 UI refreshed after all items were added");
        }
        else
        {
            Debug.LogWarning("⚠️ No InventoryUI found in GameManager");
        }

        Debug.Log("🎮 GAME READY!");
    }
}