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

        Debug.Log($"🌤️ Weer events gepland op rondes: {string.Join(", ", roundsWithWeather)}");
    }

    public void OnRoundStarted(int roundNumber)
    {
        currentRound = roundNumber;

        if (processedRounds.Contains(roundNumber))
        {
            Debug.Log($"🌤️ Ronde {roundNumber} is al afgehandeld, skip...");
            return;
        }

        if (weatherEventActive || isExecutingWeather)
        {
            Debug.Log($"🌤️ Weather event al actief in ronde {roundNumber}, skip...");
            return;
        }

        if (roundsWithWeather.Contains(roundNumber) && weatherEventsTriggered < maxWeatherEvents)
        {
            processedRounds.Add(roundNumber);
            TriggerWeatherEvent();
        }
    }

    public bool CanWeatherTrigger()
    {
        return !weatherEventActive && !isExecutingWeather;
    }

    public void TriggerWeatherEvent()
    {
        if (weatherEventActive || isExecutingWeather)
        {
            Debug.Log("🌤️ Weather event is al actief, trigger geweigerd!");
            return;
        }

        weatherEventsTriggered++;
        weatherEventActive = true;
        isExecutingWeather = true;

        if (turnManager != null)
        {
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

        Debug.Log($"🌤️ Willekeurig weer gekozen: {selectedType}");
        return selectedType;
    }

    string GetWeatherAnnouncement(WeatherType weatherType)
    {
        switch (weatherType)
        {
            case WeatherType.Tornado: return "Tornado! Items vliegen weg!";
            case WeatherType.Storm: return "Zware storm! Vernietigt items!";
            case WeatherType.Hitte: return "Hittegolf! Water verdampt!";
            case WeatherType.Regen: return "Hevige regenval! Planten groeien!";
            case WeatherType.Vorst: return "Vorst! Planten bevriezen!";
            default: return "Weerswaarschuwing!";
        }
    }

    IEnumerator ExecuteWeatherEvent(WeatherType weatherType)
    {
        yield return new WaitForSeconds(1f);

        switch (weatherType)
        {
            case WeatherType.Tornado: ExecuteTornado_MoveItems(); break;
            case WeatherType.Storm: ExecuteStorm_DestroyItems(); break;
            case WeatherType.Hitte: ExecuteHitte(); break;
            case WeatherType.Regen: ExecuteRegen(); break;
            case WeatherType.Vorst: ExecuteVorst(); break;
        }

        yield return new WaitForSeconds(1f);

        weatherEventActive = false;
        isExecutingWeather = false;

        if (turnManager != null)
        {
            turnManager.UpdateActionButtons();
        }

        Debug.Log($"🌤️ Weather event {weatherType} voltooid!");
    }

    // ============================================
    // TORNADO - VERPLAATST ITEMS
    // ============================================

    void ExecuteTornado_MoveItems()
    {
        ShowWeatherAnnouncement("Tornado! Items waaien weg!", weatherAnnouncementDuration);
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

            if (sourceTile.isProtected)
            {
                Debug.Log($"Beschermd item overgeslagen door tornado!");
                continue;
            }

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
                Debug.Log($"Tornado verplaatst item van {sourcePlayer.playerName} naar {targetPlayer.playerName}");
            }
        }

        if (movedCount > 0)
        {
            ShowWeatherAnnouncement($"Tornado verplaatst {movedCount} item(s)!", resultAnnouncementDuration);
        }
        else
        {
            ShowWeatherAnnouncement($"Tornado! Geen items konden worden verplaatst!", resultAnnouncementDuration);
        }
        UpdateAllUI();
    }

    // ============================================
    // STORM - VERNIETIGT ITEMS
    // ============================================

    void ExecuteStorm_DestroyItems()
    {
        List<Player> players = turnManager.players;
        int gardensAffected = Random.Range(0, players.Count + 1);

        if (gardensAffected == 0)
        {
            ShowWeatherAnnouncement("Storm mist de buurt! Geluk gehad!", resultAnnouncementDuration);
            return;
        }

        List<int> affectedGardenIndices = GetRandomGardenIndices(gardensAffected);
        ShowWeatherAnnouncement($"Storm raast door {gardensAffected} tuinen!", weatherAnnouncementDuration);
        PlayEffect(stormEffectPrefab);

        int totalDestroyed = 0;

        foreach (int playerIndex in affectedGardenIndices)
        {
            Player player = players[playerIndex];
            int itemsToDestroy = Random.Range(1, 3);

            List<GardenTile> tilesToDestroy = GetRandomUnprotectedTiles(player, itemsToDestroy);
            totalDestroyed += DestroyTiles(tilesToDestroy, player);
        }

        ShowWeatherAnnouncement($"Storm vernietigde {totalDestroyed} item(s)!", resultAnnouncementDuration);
        UpdateAllUI();
    }

    // ============================================
    // HITTE IMPLEMENTATIE
    // ============================================

    void ExecuteHitte()
    {
        ShowWeatherAnnouncement("Hittegolf! Water verdampt!", weatherAnnouncementDuration);
        PlayEffect(heatEffectPrefab);

        int totalDestroyed = 0;
        List<Player> players = turnManager.players;

        foreach (Player player in players)
        {
            List<GardenTile> waterTiles = new List<GardenTile>();
            Garden garden = player.assignedGarden;

            if (garden != null)
            {
                GardenTile[] allTiles = garden.GetComponentsInChildren<GardenTile>();
                foreach (GardenTile tile in allTiles)
                {
                    if (tile.occupied && tile.placedItemData != null &&
                        tile.placedItemData.type == ItemType.Water &&
                        !tile.isProtected)
                    {
                        waterTiles.Add(tile);
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

        ShowWeatherAnnouncement($"Hitte verdampt {totalDestroyed} water item(s)!", resultAnnouncementDuration);
        UpdateAllUI();
    }

    // ============================================
    // REGEN IMPLEMENTATIE
    // ============================================

    void ExecuteRegen()
    {
        ShowWeatherAnnouncement("Hevige regenval! Planten groeien!", weatherAnnouncementDuration);
        PlayEffect(rainEffectPrefab);

        int boostedPlants = 0;
        int destroyedDecor = 0;
        List<Player> players = turnManager.players;

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
                        if (IsPlantType(tile.placedItemData.type))
                            plantTiles.Add(tile);
                        else if (tile.placedItemData.type == ItemType.Decoration)
                            decoratieTiles.Add(tile);
                    }
                }
            }

            foreach (GardenTile tile in plantTiles)
            {
                if (tile.placedItemData != null)
                {
                    tile.placedItemData.score += 1;
                    boostedPlants++;
                }
            }

            List<GardenTile> unprotectedDecor = new List<GardenTile>();
            foreach (GardenTile tile in decoratieTiles)
            {
                if (!tile.isProtected)
                    unprotectedDecor.Add(tile);
            }

            int decorToDestroy = Mathf.Min(Random.Range(0, 2), unprotectedDecor.Count);
            for (int i = 0; i < decorToDestroy && unprotectedDecor.Count > 0; i++)
            {
                int randomIndex = Random.Range(0, unprotectedDecor.Count);
                GardenTile targetTile = unprotectedDecor[randomIndex];
                unprotectedDecor.RemoveAt(randomIndex);
                DestroyItemInGarden(targetTile, player);
                destroyedDecor++;
            }
        }

        ShowWeatherAnnouncement($"Regen boost {boostedPlants} planten, beschadigt {destroyedDecor} decoratie(s)!", resultAnnouncementDuration);
        UpdateAllUI();
    }

    // ============================================
    // VORST IMPLEMENTATIE
    // ============================================

    void ExecuteVorst()
    {
        ShowWeatherAnnouncement("Vorst! Planten bevriezen!", weatherAnnouncementDuration);
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

        ShowWeatherAnnouncement($"Vorst vernietigt {totalDestroyed} plant(en)!", resultAnnouncementDuration);
        UpdateAllUI();
    }

    // ============================================
    // HELPER METHODES
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

        if (tile.isProtected)
        {
            Debug.Log($"Beschermd item in tuin van {player.playerName} overgeslagen!");
            return;
        }

        if (tile.placedItem != null) Destroy(tile.placedItem);
        tile.occupied = false;
        tile.placedItem = null;
        tile.placedItemData = null;
        Debug.Log($"Item verwijderd uit tuin van {player.playerName} door weer!");
    }

    bool IsPlantType(ItemType type)
    {
        return type == ItemType.Plant ||
               type == ItemType.Tree_Big ||
               type == ItemType.Tree_Small ||
               type == ItemType.Hedge;
    }

    void PlayEffect(GameObject effectPrefab)
    {
        if (effectPrefab != null)
            StartCoroutine(PlayEffectCoroutine(effectPrefab));
        else
            Debug.Log("Geen effect prefab toegevoegd voor dit weertype!");
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
            Debug.Log($"🌤️ {message}");
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
    // PUBLIC METHODES
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

        if (currentAnnouncementCoroutine != null)
        {
            StopCoroutine(currentAnnouncementCoroutine);
            currentAnnouncementCoroutine = null;
        }

        if (weatherAnnouncementText != null)
        {
            weatherAnnouncementText.gameObject.SetActive(false);
        }

        Debug.Log("🌤️ WeatherManager gereset!");
    }
}