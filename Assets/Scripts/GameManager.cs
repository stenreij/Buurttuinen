using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public InventoryUI inventoryUI;
    public Transform playersParent;
    public ItemDatabase itemDatabase;

    private List<Player> players = new List<Player>();

    void Start()
    {
        GameSetup setup = FindObjectOfType<GameSetup>();
        if (setup != null && setup.playerNames.Count > 0)
        {
            StartGame(setup.playerNames);
        }
        else
        {
            List<string> defaultNames = new List<string> { "Player_1" };
            StartGame(defaultNames);
        }
    }

    void StartGame(List<string> playerNames)
    {
        Debug.Log("🎮 GAME STARTING...");

        // 1. Add players to the game
        CreatePlayers(playerNames);

        // 2. Configure board based on the number of players
        BoardManager board = FindObjectOfType<BoardManager>();
        if (board != null)
        {
            board.numberOfGardens = playerNames.Count;
            board.CreateBoard();
        }

        // 3. Give each player their starting items
        foreach (Player player in players)
        {
            ItemActions actions = player.GetComponent<ItemActions>();
            if (actions != null)
            {
                for (int i = 0; i < actions.startItemCount; i++)
                {
                    actions.GiveRandomItem();
                }

                // Log items
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

        // 4. Update UI
        if (inventoryUI != null && players.Count > 0)
        {
            inventoryUI.playerInventory = players[0].GetComponent<Inventory>();
            inventoryUI.RefreshUI();
        }

        // 5. Set player names in PlayerTagManager
        PlayerTagManager tagManager = FindObjectOfType<PlayerTagManager>();
        if (tagManager != null)
        {
            tagManager.SetPlayerTags(playerNames);
            Debug.Log("🏷️ Playertags added!");
        }

        Debug.Log("🎮 GAME READY!");
    }

    void CreatePlayers(List<string> playerNames)
    {
        if (playersParent != null)
        {
            foreach (Transform child in playersParent)
            {
                Destroy(child.gameObject);
            }
            players.Clear();
        }

        for (int i = 0; i < playerNames.Count; i++)
        {
            GameObject playerGO = new GameObject(playerNames[i]);
            playerGO.transform.parent = playersParent;

            Player player = playerGO.AddComponent<Player>();
            player.playerName = playerNames[i];
            player.playerIndex = i;

            Inventory inv = playerGO.AddComponent<Inventory>();
            ItemActions actions = playerGO.AddComponent<ItemActions>();

            actions.playerInventory = inv;
            actions.itemDatabase = itemDatabase;
            actions.startItemCount = 4;

            players.Add(player);
        }
    }
}