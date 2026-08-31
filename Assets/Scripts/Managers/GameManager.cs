using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public InventoryUI inventoryUI;
    public Transform playersParent;
    public ItemDatabase itemDatabase;
    public ScoreManager scoreManager;
    public Sprite shieldSprite;
    public WeatherManager weatherManager;
    public CommunityManager communityManager;

    public List<Player> players = new List<Player>();
    private TurnManager turnManager;

    void Start()
    {
        itemDatabase = FindFirstObjectByType<ItemDatabase>();
        if (itemDatabase != null)
        {
            itemDatabase.ResetPool();
        }

        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.ResetScores();
        }

        GameSetup setup = FindFirstObjectByType<GameSetup>();
        if (setup != null && setup.playerNames.Count > 0)
        {
            StartGame(setup.playerNames);
        }
        else
        {
            List<string> defaultNames = new List<string> { "Player 1", "Player 2" };
            StartGame(defaultNames);
        }
    }

    void StartGame(List<string> playerNames)
    {
        Debug.Log("🎮 Game starting...");

        CreatePlayers(playerNames);

        BoardManager board = FindFirstObjectByType<BoardManager>();
        if (board != null)
        {
            board.numberOfGardens = playerNames.Count;
            board.CreateBoard();
        }
        else
        {
            Debug.LogError("BoardManager not found!");
            return;
        }

        if (board != null)
        {
            for (int i = 0; i < players.Count && i < board.gardens.Count; i++)
            {
                players[i].assignedGarden = board.gardens[i];
            }
        }

        foreach (Player player in players)
        {
            ItemActions actions = player.GetComponent<ItemActions>();
            if (actions != null)
            {
                for (int i = 0; i < actions.startItemCount; i++)
                {
                    actions.GiveRandomItem();
                }

                Inventory inv = player.GetComponent<Inventory>();
                if (inv != null && inv.items.Count > 0)
                {
                    string itemList = "";
                    foreach (ItemData item in inv.items)
                    {
                        itemList += item.itemName + ", ";
                    }
                    itemList = itemList.TrimEnd(',', ' ');
                    Debug.Log($"{player.gameObject.name} received: {itemList}");
                }
            }
            else
            {
                Debug.LogError($"No ItemActions found on {player.gameObject.name}!");
            }
        }

        if (inventoryUI != null && players.Count > 0)
        {
            inventoryUI.playerInventory = players[0].GetComponent<Inventory>();
            UIManager.RefreshAllUI();
        }

        PlayerTagManager tagManager = FindFirstObjectByType<PlayerTagManager>();
        if (tagManager != null)
        {
            tagManager.SetPlayerTags(playerNames);
        }

        turnManager = FindFirstObjectByType<TurnManager>();
        if (turnManager != null)
        {
            turnManager.Initialize(players);
            turnManager.OnRoundStarted += OnRoundStarted;
        }

        if (weatherManager != null)
        {
            weatherManager.InitializeWeatherSystem(10);
        }

        if (communityManager != null)
        {
            communityManager.InitializeGoals();
        }

        Debug.Log("🎮 Game ready");
    }

    void OnRoundStarted(int roundNumber)
    {
        Debug.Log($"🔄 Round {roundNumber} started");

        bool communityTriggered = false;

        if (communityManager != null)
        {
            communityManager.OnRoundStarted(roundNumber);

            if (communityManager.IsCommunityActive())
            {
                communityTriggered = true;
            }
        }

        if (!communityTriggered && weatherManager != null)
        {
            weatherManager.OnRoundStarted(roundNumber);
        }
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

        TurnManager turnManager = FindFirstObjectByType<TurnManager>();

        for (int i = 0; i < playerNames.Count; i++)
        {
            GameObject playerGO = new GameObject(playerNames[i]);
            playerGO.transform.parent = playersParent;

            Player player = playerGO.AddComponent<Player>();
            player.playerName = playerNames[i];
            player.playerIndex = i;

            Inventory inv = playerGO.AddComponent<Inventory>();
            ItemActions actions = playerGO.AddComponent<ItemActions>();
            PowerUpActions powerActions = playerGO.AddComponent<PowerUpActions>();

            actions.playerInventory = inv;
            actions.itemDatabase = itemDatabase;
            actions.startItemCount = 4;

            powerActions.playerInventory = inv;
            powerActions.itemDatabase = itemDatabase;
            powerActions.turnManager = turnManager;
            powerActions.shieldSprite = shieldSprite;

            players.Add(player);
        }
    }
}