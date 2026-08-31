using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class WeatherManager : MonoBehaviour
{
    [Header("References")]
    public TurnManager turnManager;
    public ScoreManager scoreManager;
    public InventoryUI inventoryUI;
    public TextMeshProUGUI weatherAnnouncementText;

    [Header("Effect Prefabs")]
    public GameObject tornadoEffectPrefab;
    public GameObject stormEffectPrefab;
    public GameObject heatEffectPrefab;
    public GameObject rainEffectPrefab;
    public GameObject frostEffectPrefab;

    [Header("Settings")]
    public int maxWeatherEvents = 3;
    public int minWeatherEvents = 2;

    public float weatherAnnouncementDuration = 3f;
    public float resultAnnouncementDuration = 5f;

    private List<int> roundsWithWeather = new List<int>();
    private int weatherEventsTriggered = 0;
    private bool weatherEventActive = false;
    private bool isExecutingWeather = false;
    private int currentRound = 0;

    private List<int> processedRounds = new List<int>();
    private Queue<int> pendingWeatherRounds = new Queue<int>();

    private Coroutine currentAnnouncementCoroutine;

    void Start()
    {
        if (turnManager == null)
            turnManager = FindFirstObjectByType<TurnManager>();

        if (scoreManager == null)
            scoreManager = FindFirstObjectByType<ScoreManager>();

        if (inventoryUI == null)
            inventoryUI = FindFirstObjectByType<InventoryUI>();
    }

    public void InitializeWeatherSystem(int totalRounds)
    {
        int numberOfEvents = Random.Range(minWeatherEvents, maxWeatherEvents + 1);
        roundsWithWeather.Clear();
        processedRounds.Clear();
        pendingWeatherRounds.Clear();

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

        Debug.Log($"☁️ Weather events planned on rounds: {string.Join(", ", roundsWithWeather)}");
    }

    public void OnRoundStarted(int roundNumber)
    {
        currentRound = roundNumber;

        if (processedRounds.Contains(roundNumber) || weatherEventActive || isExecutingWeather) return;

        if (!roundsWithWeather.Contains(roundNumber)) return;

        CommunityManager community = FindFirstObjectByType<CommunityManager>();
        if (community != null && community.IsExecutingCommunity())
        {
            pendingWeatherRounds.Enqueue(roundNumber);
            return;
        }

        if (weatherEventsTriggered < maxWeatherEvents)
        {
            processedRounds.Add(roundNumber);
            TriggerWeatherEvent();
        }
    }

    public void OnCommunityReady()
    {
        CommunityManager community = FindFirstObjectByType<CommunityManager>();
        if (community != null && community.IsExecutingCommunity())
        {
            StartCoroutine(RetryCommunityReady());
            return;
        }

        if (pendingWeatherRounds.Count > 0)
        {
            int nextRound = pendingWeatherRounds.Peek();

            if (!roundsWithWeather.Contains(nextRound))
            {
                pendingWeatherRounds.Dequeue();
                OnCommunityReady();
                return;
            }

            if (weatherEventsTriggered >= maxWeatherEvents)
            {
                pendingWeatherRounds.Clear();
                return;
            }

            nextRound = pendingWeatherRounds.Dequeue();

            if (!processedRounds.Contains(nextRound))
            {
                processedRounds.Add(nextRound);
                TriggerWeatherEvent();
            }
        }
    }

    private IEnumerator RetryCommunityReady()
    {
        yield return new WaitForSeconds(0.5f);
        OnCommunityReady();
    }

    public bool CanWeatherTrigger()
    {
        return !weatherEventActive && !isExecutingWeather;
    }

    public void TriggerWeatherEvent()
    {
        if (weatherEventActive || isExecutingWeather) return;

        weatherEventsTriggered++;
        weatherEventActive = true;
        isExecutingWeather = true;

        if (turnManager != null)
        {
            turnManager.SetGamePaused(true);
        }

        WeatherType weatherType = GetRandomWeatherType();
        string announcement = GetWeatherAnnouncement(weatherType);
        ShowWeatherAnnouncement(announcement, weatherAnnouncementDuration);

        StartCoroutine(ExecuteWeatherEvent(weatherType));
    }

    WeatherType GetRandomWeatherType()
    {
        WeatherType[] allTypes = System.Enum.GetValues(typeof(WeatherType)) as WeatherType[];
        WeatherType selectedType = allTypes[Random.Range(0, allTypes.Length)];
        Debug.Log($"☁️Weather selected: {selectedType}");
        return selectedType;
    }

    string GetWeatherAnnouncement(WeatherType weatherType)
    {
        switch (weatherType)
        {
            case WeatherType.Tornado: return "Tornado! Items fly away!";
            case WeatherType.Storm: return "Heavy storm! Destroys items!";
            case WeatherType.Heatwave: return "Heatwave! Water evaporates!";
            case WeatherType.Rain: return "Heavy rain! Plants grow!";
            case WeatherType.Frost: return "Frost! Plants freeze!";
            default: return "Weather warning!";
        }
    }

    IEnumerator ExecuteWeatherEvent(WeatherType weatherType)
    {
        yield return new WaitForSeconds(1f);

        switch (weatherType)
        {
            case WeatherType.Tornado: ExecuteTornado_MoveItems(); break;
            case WeatherType.Storm: ExecuteStorm_DestroyItems(); break;
            case WeatherType.Heatwave: ExecuteHeatwave(); break;
            case WeatherType.Rain: ExecuteRain(); break;
            case WeatherType.Frost: ExecuteFrost(); break;
        }

        yield return new WaitForSeconds(1f);

        weatherEventActive = false;
        isExecutingWeather = false;

        if (turnManager != null)
        {
            turnManager.SetGamePaused(false);
            turnManager.UpdateActionButtons();
        }

        if (pendingWeatherRounds.Count > 0)
        {
            int nextRound = pendingWeatherRounds.Dequeue();
            if (!processedRounds.Contains(nextRound))
            {
                processedRounds.Add(nextRound);
                TriggerWeatherEvent();
            }
        }

        Debug.Log($"☁️ Weather event {weatherType} completed");
    }

    // ============================================
    // TORNADO - MOVE ITEMS
    // ============================================

    void ExecuteTornado_MoveItems()
    {
        ShowWeatherAnnouncement("Tornado! Items fly away!", weatherAnnouncementDuration);
        PlayEffect(tornadoEffectPrefab);

        List<Player> players = turnManager.players;
        List<GardenTile> allOccupiedTiles = new List<GardenTile>();

        foreach (Player player in players)
        {
            allOccupiedTiles.AddRange(GetOccupiedTiles(player));
        }

        int itemsToMove = Mathf.Min(Random.Range(1, 4), allOccupiedTiles.Count);
        int movedCount = 0;

        for (int i = 0; i < itemsToMove && allOccupiedTiles.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, allOccupiedTiles.Count);
            GardenTile sourceTile = allOccupiedTiles[randomIndex];
            allOccupiedTiles.RemoveAt(randomIndex);

            if (sourceTile.isProtected) continue;

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

            Player targetPlayer = players[Random.Range(0, players.Count)];
            GardenTile targetTile = GetRandomEmptyTile(targetPlayer);

            if (targetTile != null && sourceTile != null && sourceTile.placedItem != null)
            {
                if (ItemPlacer.MoveItem(sourceTile, targetTile))
                {
                    movedCount++;
                    Debug.Log($"☁️🌪️ Tornado moved item from {sourcePlayer.playerName} to {targetPlayer.playerName}");
                }
            }

            if (movedCount > 0)
            {
                ShowWeatherAnnouncement($"Tornado moved {movedCount} item(s)!", resultAnnouncementDuration);
            }
            else
            {
                ShowWeatherAnnouncement($"Tornado! No items could be moved!", resultAnnouncementDuration);
            }
            UpdateAllUI();
        }
    }

    // ============================================
    // STORM - DESTROY ITEMS
    // ============================================

    private void ExecuteStorm_DestroyItems()
    {
        List<Player> players = turnManager.players;
        int gardensAffected = Random.Range(0, players.Count + 1);

        if (gardensAffected == 0)
        {
            ShowWeatherAnnouncement("Storm misses the neighborhood! Lucky!", resultAnnouncementDuration);
            return;
        }

        List<int> affectedGardenIndices = GetRandomGardenIndices(gardensAffected);
        ShowWeatherAnnouncement($"Storm hits {gardensAffected} gardens!", weatherAnnouncementDuration);
        PlayEffect(stormEffectPrefab);

        int totalDestroyed = 0;

        foreach (int playerIndex in affectedGardenIndices)
        {
            Player player = players[playerIndex];
            int itemsToDestroy = Random.Range(1, 3);

            List<GardenTile> tilesToDestroy = GetRandomUnprotectedTiles(player, itemsToDestroy);
            totalDestroyed += DestroyTiles(tilesToDestroy, player);
        }

        ShowWeatherAnnouncement($"Storm destroyed {totalDestroyed} item(s)!", resultAnnouncementDuration);
        UpdateAllUI();
    }

    // ============================================
    // HEATWAVE
    // ============================================

    private void ExecuteHeatwave()
    {
        ShowWeatherAnnouncement("Heatwave! Water evaporates!", weatherAnnouncementDuration);
        PlayEffect(heatEffectPrefab);

        int totalDestroyed = 0;
        List<Player> players = turnManager.players;

        foreach (Player player in players)
        {
            List<GardenTile> waterTiles = GardenHelper.GetTilesByType(player, ItemType.Water);

            // Filter out protected tiles
            waterTiles.RemoveAll(t => t.isProtected);

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

        ShowWeatherAnnouncement($"Heat evaporated {totalDestroyed} water item(s)!", resultAnnouncementDuration);
        UpdateAllUI();
    }

    // ============================================
    // RAIN
    // ============================================

    void ExecuteRain()
    {
        ShowWeatherAnnouncement("Heavy rain! Plants grow!", weatherAnnouncementDuration);
        PlayEffect(rainEffectPrefab);

        int destroyedDecor = 0;
        List<Player> players = turnManager.players;

        foreach (Player player in players)
        {
            player.weatherBoost = 1;

            Garden garden = player.assignedGarden;
            if (garden != null)
            {
                GardenTile[] allTiles = garden.GetComponentsInChildren<GardenTile>();
                foreach (GardenTile tile in allTiles)
                {
                    if (tile.occupied && tile.placedItemData != null)
                    {
                        if (tile.placedItemData.type == ItemType.Decoration && !tile.isProtected)
                        {
                            DestroyItemInGarden(tile, player);
                            destroyedDecor++;
                        }
                    }
                }
            }
        }

        ShowWeatherAnnouncement($"Rain boost active! Plants get +1 each! {destroyedDecor} decorations damaged.", resultAnnouncementDuration);
        UpdateAllUI();
    }

    // ============================================
    // FROST
    // ============================================

    void ExecuteFrost()
    {
        ShowWeatherAnnouncement("Frost! Plants freeze!", weatherAnnouncementDuration);
        PlayEffect(frostEffectPrefab);

        int totalDestroyed = 0;
        List<Player> players = turnManager.players;

        foreach (Player player in players)
        {
            List<GardenTile> plantTiles = new List<GardenTile>();
            Garden garden = player.assignedGarden;

            if (garden != null)
            {
                GardenTile[] allTiles = garden.GetComponentsInChildren<GardenTile>();
                foreach (GardenTile tile in allTiles)
                {
                    if (tile.occupied && tile.placedItemData != null &&
                        IsPlantType(tile.placedItemData.type) &&
                        !tile.isProtected)
                    {
                        plantTiles.Add(tile);
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

        ShowWeatherAnnouncement($"Frost destroyed {totalDestroyed} plant(s)!", resultAnnouncementDuration);
        UpdateAllUI();
    }

    // ============================================
    // HELPER METHODS
    // ============================================

    List<int> GetRandomGardenIndices(int count)
    {
        List<int> indices = new List<int>();
        List<int> available = new List<int>();
        for (int i = 0; i < turnManager.players.Count; i++)
            available.Add(i);

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
        if (garden == null) return tiles;

        GardenTile[] allTiles = garden.GetComponentsInChildren<GardenTile>();
        foreach (GardenTile tile in allTiles)
        {
            if (tile.occupied && tile.placedItem != null)
                tiles.Add(tile);
        }
        return tiles;
    }

    List<GardenTile> GetUnprotectedTiles(Player player)
    {
        List<GardenTile> allTiles = GetOccupiedTiles(player);
        List<GardenTile> unprotectedTiles = new List<GardenTile>();

        foreach (GardenTile tile in allTiles)
        {
            if (!tile.isProtected)
            {
                unprotectedTiles.Add(tile);
            }
        }

        return unprotectedTiles;
    }

    List<GardenTile> GetRandomUnprotectedTiles(Player player, int count)
    {
        List<GardenTile> unprotectedTiles = GetUnprotectedTiles(player);
        List<GardenTile> selectedTiles = new List<GardenTile>();

        int actualCount = Mathf.Min(count, unprotectedTiles.Count);

        for (int i = 0; i < actualCount && unprotectedTiles.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, unprotectedTiles.Count);
            selectedTiles.Add(unprotectedTiles[randomIndex]);
            unprotectedTiles.RemoveAt(randomIndex);
        }

        return selectedTiles;
    }

    int DestroyTiles(List<GardenTile> tiles, Player player)
    {
        int destroyed = 0;
        foreach (GardenTile tile in tiles)
        {
            if (tile != null && tile.occupied && tile.placedItem != null && !tile.isProtected)
            {
                DestroyItemInGarden(tile, player);
                destroyed++;
            }
        }
        return destroyed;
    }

    GardenTile GetRandomEmptyTile(Player player)
    {
        Garden garden = player.assignedGarden;
        if (garden == null) return null;

        List<GardenTile> emptyTiles = new List<GardenTile>();
        GardenTile[] allTiles = garden.GetComponentsInChildren<GardenTile>();
        foreach (GardenTile tile in allTiles)
        {
            if (!tile.occupied) emptyTiles.Add(tile);
        }

        if (emptyTiles.Count == 0) return null;
        return emptyTiles[Random.Range(0, emptyTiles.Count)];
    }

    void DestroyItemInGarden(GardenTile tile, Player player)
    {
        if (tile == null || !tile.occupied || tile.placedItem == null) return;
        if (tile.isProtected) return;

        ItemPlacer.RemoveItemFromTile(tile);
        Debug.Log($"☁️ Item removed from {player?.playerName ?? "unknown"}'s garden by weather");
    }

    bool IsPlantType(ItemType type)
    {
        return type == ItemType.Plant ||
               type == ItemType.Tree_Big ||
               type == ItemType.Tree_Small;
    }

    void PlayEffect(GameObject effectPrefab)
    {
        if (effectPrefab != null)
            StartCoroutine(PlayEffectCoroutine(effectPrefab));
    }

    IEnumerator PlayEffectCoroutine(GameObject effectPrefab)
    {
        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, Vector3.zero, Quaternion.identity);
            yield return new WaitForSeconds(2f);
            Destroy(effect);
        }
    }

    void ShowWeatherAnnouncement(string message, float duration = -1)
    {
        if (weatherAnnouncementText != null)
        {
            weatherAnnouncementText.text = message;
            weatherAnnouncementText.gameObject.SetActive(true);

            float actualDuration = duration > 0 ? duration : weatherAnnouncementDuration;
            StartCoroutine(HideAnnouncementAfterDelay(actualDuration));
        }
        else
        {
            Debug.Log($"☁️ Weather: {message}");
        }
    }

    IEnumerator HideAnnouncementAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (weatherAnnouncementText != null)
            weatherAnnouncementText.gameObject.SetActive(false);
    }

    void UpdateAllUI()
    {
        if (scoreManager != null) scoreManager.UpdateScores();
        if (inventoryUI != null) inventoryUI.RefreshUI();

        CommunityManager communityManager = FindFirstObjectByType<CommunityManager>();
        if (communityManager != null)
        {
            communityManager.UpdateCommunityGoalScore();
        }
    }


    // ============================================
    // PUBLIC METHODS
    // ============================================

    public bool IsWeatherEventActive() => weatherEventActive;
    public List<int> GetWeatherRounds() => roundsWithWeather;

    public void ResetWeatherManager()
    {
        weatherEventActive = false;
        isExecutingWeather = false;
        weatherEventsTriggered = 0;
        currentRound = 0;
        roundsWithWeather.Clear();
        processedRounds.Clear();
        pendingWeatherRounds.Clear();

        if (currentAnnouncementCoroutine != null)
        {
            StopCoroutine(currentAnnouncementCoroutine);
            currentAnnouncementCoroutine = null;
        }

        if (weatherAnnouncementText != null)
        {
            weatherAnnouncementText.gameObject.SetActive(false);
        }
    }
}