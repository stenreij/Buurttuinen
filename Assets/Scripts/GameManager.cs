using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public InventoryUI inventoryUI;
    public Transform playersParent;
    public ItemDatabase itemDatabase;
    public ScoreManager scoreManager;
    public Sprite shieldSprite;

    [Header("Weer System")]
    public TextMeshProUGUI weatherAnnouncementText;
    public GameObject tornadoEffectPrefab;
    public GameObject stormEffectPrefab;
    public GameObject heatEffectPrefab;
    public GameObject rainEffectPrefab;
    public GameObject frostEffectPrefab;
    public int maxWeatherEvents = 3;
    public int minWeatherEvents = 2;
    public float weatherAnnouncementDuration = 9f;

    public List<Player> players = new List<Player>();

    // Weer variabelen
    private int weatherEventsTriggered = 0;
    private List<int> roundsWithWeather = new List<int>();
    private bool weatherEventActive = false;
    private int currentRound = 0;
    private TurnManager turnManager;

    void Start()
    {
        itemDatabase = FindFirstObjectByType<ItemDatabase>();
        if (itemDatabase != null)
        {
            itemDatabase.ResetPool();
            Debug.Log("ItemDatabase pool gereset!");
        }

        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.ResetScores();
            Debug.Log("Scores gereset!");
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
        Debug.Log("GAME STARTING...");

        // 1. Add players to the game
        CreatePlayers(playerNames);

        // 2. CREATE THE BOARD
        BoardManager board = FindFirstObjectByType<BoardManager>();
        if (board != null)
        {
            board.numberOfGardens = playerNames.Count;
            board.CreateBoard();
            Debug.Log("Board created!");
        }
        else
        {
            Debug.LogError("BoardManager not found!");
            return;
        }

        // 3. Link players to their gardens
        if (board != null)
        {
            for (int i = 0; i < players.Count && i < board.gardens.Count; i++)
            {
                Player player = players[i];
                Garden garden = board.gardens[i];
                player.assignedGarden = garden;
            }
        }

        // 4. Give each player their starting items
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
                    Debug.Log($"Speler {player.gameObject.name} ontving: {itemList}");
                }
            }
            else
            {
                Debug.LogError($"Geen ItemActions gevonden op {player.gameObject.name}!");
            }
        }

        // 5. Update UI
        if (inventoryUI != null && players.Count > 0)
        {
            inventoryUI.playerInventory = players[0].GetComponent<Inventory>();
            inventoryUI.RefreshUI();
        }

        // 6. Set player names in PlayerTagManager
        PlayerTagManager tagManager = FindFirstObjectByType<PlayerTagManager>();
        if (tagManager != null)
        {
            tagManager.SetPlayerTags(playerNames);
        }

        Debug.Log("GAME READY!");

        // 7. Start TurnManager
        turnManager = FindFirstObjectByType<TurnManager>();
        if (turnManager != null)
        {
            turnManager.Initialize(players);
            turnManager.OnRoundStarted += OnRoundStarted;
            Debug.Log("TurnManager gestart!");
        }

        // 8. Initialize Weather System
        InitializeWeatherSystem();
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

    // ============================================
    // WEER SYSTEM
    // ============================================

    void InitializeWeatherSystem()
    {
        int totalRounds = 10;
        int numberOfEvents = Random.Range(minWeatherEvents, maxWeatherEvents + 1);
        roundsWithWeather.Clear();
        
        List<int> availableRounds = new List<int>();
        for (int i = 2; i < totalRounds - 1; i++)
        {
            availableRounds.Add(i);
        }
        
        for (int i = 0; i < numberOfEvents && availableRounds.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availableRounds.Count);
            roundsWithWeather.Add(availableRounds[randomIndex]);
            availableRounds.RemoveAt(randomIndex);
        }
        
        roundsWithWeather.Sort();
        
        Debug.Log($"Weer events gepland op rondes: {string.Join(", ", roundsWithWeather)}");
    }

    void OnRoundStarted(int roundNumber)
    {
        currentRound = roundNumber;
        
        if (roundsWithWeather.Contains(roundNumber) && weatherEventsTriggered < maxWeatherEvents && !weatherEventActive)
        {
            TriggerWeatherEvent();
        }
    }

    void TriggerWeatherEvent()
    {
        weatherEventsTriggered++;
        weatherEventActive = true;
        
        if (turnManager != null)
        {
            turnManager.SetWeatherEventActive(true);
        }
        
        WeatherType weatherType = GetRandomWeatherType();
        string announcement = GetWeatherAnnouncement(weatherType);
        ShowWeatherAnnouncement(announcement);
        
        StartCoroutine(ExecuteWeatherEvent(weatherType));
    }

    WeatherType GetRandomWeatherType()
    {
        WeatherType[] allTypes = System.Enum.GetValues(typeof(WeatherType)) as WeatherType[];
        return allTypes[Random.Range(0, allTypes.Length)];
    }

    string GetWeatherAnnouncement(WeatherType weatherType)
    {
        switch (weatherType)
        {
            case WeatherType.Tornado:
                return "TORNADO WAARSCHUWING!";
            case WeatherType.Hitte:
                return "HITTEGOLF! Water verdampt!";
            case WeatherType.Regen:
                return "HEVIGE REGENVAL! Planten groeien!";
            case WeatherType.Vorst:
                return "VORST! Planten bevriezen!";
            case WeatherType.Storm:
                return "ZWARE STORM! Items waaien weg!";
            default:
                return "WEERSWAARSCHUWING!";
        }
    }

    IEnumerator ExecuteWeatherEvent(WeatherType weatherType)
    {
        yield return new WaitForSeconds(3f);
        
        switch (weatherType)
        {
            case WeatherType.Tornado:
                ExecuteTornado();
                break;
            case WeatherType.Hitte:
                ExecuteHitte();
                break;
            case WeatherType.Regen:
                ExecuteRegen();
                break;
            case WeatherType.Vorst:
                ExecuteVorst();
                break;
            case WeatherType.Storm:
                ExecuteStorm();
                break;
        }
        
        yield return new WaitForSeconds(2.5f);
        
        if (turnManager != null)
        {
            turnManager.SetWeatherEventActive(false);
        }
        
        weatherEventActive = false;
    }

    // ============================================
    // TORNADO IMPLEMENTATIE
    // ============================================

    void ExecuteTornado()
    {
        int gardensAffected = Random.Range(0, players.Count + 1);
        
        if (gardensAffected == 0)
        {
            ShowWeatherAnnouncement("Tornado mist de buurt! Geluk gehad!");
            return;
        }
        
        List<int> affectedGardenIndices = GetRandomGardenIndices(gardensAffected);
        
        ShowWeatherAnnouncement($"Tornado raast door {gardensAffected} tuinen!");
        
        if (tornadoEffectPrefab != null)
        {
            StartCoroutine(PlayWeatherEffect(tornadoEffectPrefab));
        }
        
        int totalDestroyed = 0;
        
        foreach (int playerIndex in affectedGardenIndices)
        {
            Player player = players[playerIndex];
            int itemsToDestroy = Random.Range(1, 3);
            
            List<GardenTile> occupiedTiles = GetOccupiedTiles(player);
            int actualDestroyCount = Mathf.Min(itemsToDestroy, occupiedTiles.Count);
            
            for (int i = 0; i < actualDestroyCount && occupiedTiles.Count > 0; i++)
            {
                int randomTileIndex = Random.Range(0, occupiedTiles.Count);
                GardenTile targetTile = occupiedTiles[randomTileIndex];
                occupiedTiles.RemoveAt(randomTileIndex);
                
                DestroyItemInGarden(targetTile, player);
                totalDestroyed++;
            }
            
            Debug.Log($"Tornado vernietigde {actualDestroyCount} item(s) in tuin van {player.playerName}");
        }
        
        ShowWeatherAnnouncement($"Tornado vernietigde {totalDestroyed} item(s)!");
        UpdateAllUI();
    }

    // ============================================
    // HITTE IMPLEMENTATIE
    // ============================================

    void ExecuteHitte()
    {
        ShowWeatherAnnouncement("Hittegolf! Water verdampt!");
        
        if (heatEffectPrefab != null)
        {
            StartCoroutine(PlayWeatherEffect(heatEffectPrefab));
        }
        
        int totalDestroyed = 0;
        
        foreach (Player player in players)
        {
            List<GardenTile> waterTiles = new List<GardenTile>();
            Garden garden = player.assignedGarden;
            
            if (garden != null)
            {
                GardenTile[] allTiles = garden.GetComponentsInChildren<GardenTile>();
                foreach (GardenTile tile in allTiles)
                {
                    if (tile.occupied && tile.placedItemData != null)
                    {
                        // Check of het item type Water is
                        if (tile.placedItemData.type == ItemType.Water)
                        {
                            waterTiles.Add(tile);
                        }
                    }
                }
            }
            
            int itemsToDestroy = Mathf.Min(Random.Range(1, 3), waterTiles.Count);
            for (int i = 0; i < itemsToDestroy && waterTiles.Count > 0; i++)
            {
                int randomIndex = Random.Range(0, waterTiles.Count);
                GardenTile targetTile = waterTiles[randomIndex];
                waterTiles.RemoveAt(randomIndex);
                
                DestroyItemInGarden(targetTile, player);
                totalDestroyed++;
            }
        }
        
        ShowWeatherAnnouncement($"Hitte verdampt {totalDestroyed} water item(s)!");
        UpdateAllUI();
    }

    // ============================================
    // REGEN IMPLEMENTATIE
    // ============================================

    void ExecuteRegen()
    {
        ShowWeatherAnnouncement("Hevige regenval! Planten groeien!");
        
        if (rainEffectPrefab != null)
        {
            StartCoroutine(PlayWeatherEffect(rainEffectPrefab));
        }
        
        int boostedPlants = 0;
        int destroyedDecor = 0;
        
        foreach (Player player in players)
        {
            List<GardenTile> plantTiles = new List<GardenTile>();
            List<GardenTile> decoratieTiles = new List<GardenTile>();
            Garden garden = player.assignedGarden;
            
            if (garden != null)
            {
                GardenTile[] allTiles = garden.GetComponentsInChildren<GardenTile>();
                foreach (GardenTile tile in allTiles)
                {
                    if (tile.occupied && tile.placedItemData != null)
                    {
                        // Check of het item een plant-type is
                        if (IsPlantType(tile.placedItemData.type))
                        {
                            plantTiles.Add(tile);
                        }
                        // Check of het item een decoratie-type is
                        else if (tile.placedItemData.type == ItemType.Decoration)
                        {
                            decoratieTiles.Add(tile);
                        }
                    }
                }
            }
            
            // Boost planten (extra score)
            foreach (GardenTile tile in plantTiles)
            {
                if (tile.placedItemData != null)
                {
                    tile.placedItemData.score += 1;
                    boostedPlants++;
                }
            }
            
            // Beschadig decoraties (verwijder 0-1 decoratie per tuin)
            int decorToDestroy = Mathf.Min(Random.Range(0, 2), decoratieTiles.Count);
            for (int i = 0; i < decorToDestroy && decoratieTiles.Count > 0; i++)
            {
                int randomIndex = Random.Range(0, decoratieTiles.Count);
                GardenTile targetTile = decoratieTiles[randomIndex];
                decoratieTiles.RemoveAt(randomIndex);
                
                DestroyItemInGarden(targetTile, player);
                destroyedDecor++;
            }
        }
        
        ShowWeatherAnnouncement($"Regen boost {boostedPlants} planten en beschadigt {destroyedDecor} decoratie(s)!");
        UpdateAllUI();
    }

    bool IsPlantType(ItemType type)
    {
        // Alle types die als plant worden beschouwd
        return type == ItemType.Plant || 
               type == ItemType.Tree_Big || 
               type == ItemType.Tree_Small || 
               type == ItemType.Hedge;
    }

    // ============================================
    // VORST IMPLEMENTATIE
    // ============================================

    void ExecuteVorst()
    {
        ShowWeatherAnnouncement("Vorst! Planten bevriezen!");
        
        if (frostEffectPrefab != null)
        {
            StartCoroutine(PlayWeatherEffect(frostEffectPrefab));
        }
        
        int totalDestroyed = 0;
        
        foreach (Player player in players)
        {
            List<GardenTile> plantTiles = new List<GardenTile>();
            Garden garden = player.assignedGarden;
            
            if (garden != null)
            {
                GardenTile[] allTiles = garden.GetComponentsInChildren<GardenTile>();
                foreach (GardenTile tile in allTiles)
                {
                    if (tile.occupied && tile.placedItemData != null)
                    {
                        // Check of het item een plant-type is
                        if (IsPlantType(tile.placedItemData.type))
                        {
                            plantTiles.Add(tile);
                        }
                    }
                }
            }
            
            int itemsToDestroy = Mathf.Min(Random.Range(1, 4), plantTiles.Count);
            for (int i = 0; i < itemsToDestroy && plantTiles.Count > 0; i++)
            {
                int randomIndex = Random.Range(0, plantTiles.Count);
                GardenTile targetTile = plantTiles[randomIndex];
                plantTiles.RemoveAt(randomIndex);
                
                DestroyItemInGarden(targetTile, player);
                totalDestroyed++;
            }
        }
        
        ShowWeatherAnnouncement($"Vorst vernietigt {totalDestroyed} plant(en)!");
        UpdateAllUI();
    }

    // ============================================
    // STORM IMPLEMENTATIE
    // ============================================

    void ExecuteStorm()
    {
        ShowWeatherAnnouncement("Zware storm! Items waaien weg!");
        
        if (stormEffectPrefab != null)
        {
            StartCoroutine(PlayWeatherEffect(stormEffectPrefab));
        }
        
        List<GardenTile> allOccupiedTiles = new List<GardenTile>();
        foreach (Player player in players)
        {
            List<GardenTile> tiles = GetOccupiedTiles(player);
            allOccupiedTiles.AddRange(tiles);
        }
        
        int itemsToMove = Mathf.Min(Random.Range(1, 4), allOccupiedTiles.Count);
        int movedCount = 0;
        
        for (int i = 0; i < itemsToMove && allOccupiedTiles.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, allOccupiedTiles.Count);
            GardenTile sourceTile = allOccupiedTiles[randomIndex];
            allOccupiedTiles.RemoveAt(randomIndex);
            
            // Vind de speler die eigenaar is van deze tile
            Player sourcePlayer = null;
            foreach (Player player in players)
            {
                if (player.assignedGarden != null)
                {
                    GardenTile[] tiles = player.assignedGarden.GetComponentsInChildren<GardenTile>();
                    if (tiles.Contains(sourceTile))
                    {
                        sourcePlayer = player;
                        break;
                    }
                }
            }
            
            if (sourcePlayer == null) continue;
            
            // Vind een nieuwe willekeurige tuin en lege plek
            Player targetPlayer = players[Random.Range(0, players.Count)];
            GardenTile targetTile = GetRandomEmptyTile(targetPlayer);
            
            if (targetTile != null && sourceTile != null && sourceTile.placedItem != null)
            {
                // Verplaats item
                targetTile.occupied = true;
                targetTile.placedItem = sourceTile.placedItem;
                targetTile.placedItemData = sourceTile.placedItemData;
                
                if (sourceTile.placedItem != null)
                {
                    sourceTile.placedItem.transform.position = targetTile.transform.position;
                    sourceTile.placedItem.transform.parent = targetTile.transform;
                }
                
                sourceTile.occupied = false;
                sourceTile.placedItem = null;
                sourceTile.placedItemData = null;
                
                movedCount++;
                Debug.Log($"Storm verplaatst item van {sourcePlayer.playerName} naar {targetPlayer.playerName}");
            }
        }
        
        ShowWeatherAnnouncement($"Storm verplaatst {movedCount} item(s) naar andere tuinen!");
        UpdateAllUI();
    }

    // ============================================
    // HELPER METHODES
    // ============================================

    List<int> GetRandomGardenIndices(int count)
    {
        List<int> indices = new List<int>();
        List<int> available = new List<int>();
        for (int i = 0; i < players.Count; i++)
        {
            available.Add(i);
        }
        
        for (int i = 0; i < count && available.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, available.Count);
            indices.Add(available[randomIndex]);
            available.RemoveAt(randomIndex);
        }
        
        return indices;
    }

    List<GardenTile> GetOccupiedTiles(Player player)
    {
        List<GardenTile> tiles = new List<GardenTile>();
        Garden garden = player.assignedGarden;
        
        if (garden != null)
        {
            GardenTile[] allTiles = garden.GetComponentsInChildren<GardenTile>();
            foreach (GardenTile tile in allTiles)
            {
                if (tile.occupied && tile.placedItem != null)
                {
                    tiles.Add(tile);
                }
            }
        }
        
        return tiles;
    }

    GardenTile GetRandomEmptyTile(Player player)
    {
        Garden garden = player.assignedGarden;
        if (garden == null) return null;
        
        List<GardenTile> emptyTiles = new List<GardenTile>();
        GardenTile[] allTiles = garden.GetComponentsInChildren<GardenTile>();
        foreach (GardenTile tile in allTiles)
        {
            if (!tile.occupied)
            {
                emptyTiles.Add(tile);
            }
        }
        
        if (emptyTiles.Count == 0) return null;
        return emptyTiles[Random.Range(0, emptyTiles.Count)];
    }

    void DestroyItemInGarden(GardenTile tile, Player player)
    {
        if (tile == null || !tile.occupied || tile.placedItem == null) return;
        
        if (tile.placedItem != null)
        {
            Destroy(tile.placedItem);
        }
        
        tile.occupied = false;
        tile.placedItem = null;
        tile.placedItemData = null;
        
        Debug.Log($"Item verwijderd uit tuin van {player.playerName}");
    }

    IEnumerator PlayWeatherEffect(GameObject effectPrefab)
    {
        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, Vector3.zero, Quaternion.identity);
            yield return new WaitForSeconds(2f);
            Destroy(effect);
        }
        yield return null;
    }

    // ============================================
    // UI EN FEEDBACK
    // ============================================

    void ShowWeatherAnnouncement(string message)
    {
        if (weatherAnnouncementText != null)
        {
            weatherAnnouncementText.text = message;
            weatherAnnouncementText.gameObject.SetActive(true);
            StartCoroutine(HideAnnouncementAfterDelay(weatherAnnouncementDuration));
        }
        else
        {
            Debug.Log($"Weer aankondiging: {message}");
        }
    }

    IEnumerator HideAnnouncementAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (weatherAnnouncementText != null)
        {
            weatherAnnouncementText.gameObject.SetActive(false);
        }
    }

    void UpdateAllUI()
    {
        if (scoreManager != null)
        {
            scoreManager.UpdateScores();
        }
        
        if (inventoryUI != null)
        {
            inventoryUI.RefreshUI();
        }
    }

    // ============================================
    // PUBLIC METHODES
    // ============================================

    public bool IsWeatherEventActive()
    {
        return weatherEventActive;
    }

    public int GetCurrentRound()
    {
        return currentRound;
    }

    public List<int> GetWeatherRounds()
    {
        return roundsWithWeather;
    }
}

public enum WeatherType
{
    Tornado,
    Hitte,
    Regen,
    Vorst,
    Storm
}