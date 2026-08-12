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
    public float weatherAnnouncementDuration = 6f;

    private List<int> roundsWithWeather = new List<int>();
    private int weatherEventsTriggered = 0;
    private bool weatherEventActive = false;
    private bool isExecutingWeather = false;
    private int currentRound = 0;

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

        if (roundsWithWeather.Contains(roundNumber) &&
            weatherEventsTriggered < maxWeatherEvents &&
            !weatherEventActive &&
            !isExecutingWeather)
        {
            TriggerWeatherEvent();
        }
    }

    public void TriggerWeatherEvent()
    {
        weatherEventsTriggered++;
        weatherEventActive = true;
        isExecutingWeather = true;

        if (turnManager != null)
        {
            turnManager.SetGamePaused(true);
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
            case WeatherType.Tornado: return " Tornado waarschuwing!";
            case WeatherType.Hitte: return " Hittegolf! Water verdampt!";
            case WeatherType.Regen: return " Hevige regenval! Planten verzuipen!";
            case WeatherType.Vorst: return " Vorst! Planten bevriezen!";
            case WeatherType.Storm: return " Zware storm! Items waaien weg!";
            default: return " Weerswaarschuwing!";
        }
    }

    IEnumerator ExecuteWeatherEvent(WeatherType weatherType)
    {
        yield return new WaitForSeconds(3f);

        switch (weatherType)
        {
            case WeatherType.Tornado: ExecuteTornado(); break;
            case WeatherType.Hitte: ExecuteHitte(); break;
            case WeatherType.Regen: ExecuteRegen(); break;
            case WeatherType.Vorst: ExecuteVorst(); break;
            case WeatherType.Storm: ExecuteStorm(); break;
        }

        yield return new WaitForSeconds(2.5f);

        weatherEventActive = false;
        isExecutingWeather = false;

        if (turnManager != null)
        {
            turnManager.SetGamePaused(false);
            turnManager.ResumeTurnAfterWeather();
        }
    }

    // ============================================
    // WEER EFFECTEN
    // ============================================

    void ExecuteTornado()
    {
        List<Player> players = turnManager.players;
        int gardensAffected = Random.Range(0, players.Count + 1);

        if (gardensAffected == 0)
        {
            ShowWeatherAnnouncement("Tornado mist de buurt! Geluk gehad!");
            return;
        }

        List<int> affectedGardenIndices = GetRandomGardenIndices(gardensAffected);
        ShowWeatherAnnouncement($"Tornado raast door {gardensAffected} tuinen!");
        PlayEffect(tornadoEffectPrefab);

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
        }

        ShowWeatherAnnouncement($"Tornado vernietigde {totalDestroyed} item(s)!");
        UpdateAllUI();
    }

    void ExecuteHitte()
    {
        ShowWeatherAnnouncement("Hittegolf! Water verdampt!");
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
                        tile.placedItemData.type == ItemType.Water)
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

        ShowWeatherAnnouncement($"Hitte verdampt {totalDestroyed} water item(s)!");
        UpdateAllUI();
    }

    void ExecuteRegen()
    {
        ShowWeatherAnnouncement("Hevige regenval! Planten verzuipen!");
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

        ShowWeatherAnnouncement($"Regen boost {boostedPlants} planten, beschadigt {destroyedDecor} decoratie(s)!");
        UpdateAllUI();
    }

    void ExecuteVorst()
    {
        ShowWeatherAnnouncement("Vorst! Planten bevriezen!");
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
                        IsPlantType(tile.placedItemData.type))
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

        ShowWeatherAnnouncement($"Vorst vernietigt {totalDestroyed} plant(en)!");
        UpdateAllUI();
    }

    void ExecuteStorm()
    {
        ShowWeatherAnnouncement("Zware storm! Items waaien weg!");
        PlayEffect(stormEffectPrefab);

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
            }
        }

        ShowWeatherAnnouncement($"Storm verplaatst {movedCount} item(s)!");
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

        if (tile.placedItem != null) Destroy(tile.placedItem);
        tile.occupied = false;
        tile.placedItem = null;
        tile.placedItemData = null;
        Debug.Log($"Item verwijderd uit tuin van {player.playerName}");
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
    }

    // ============================================
    // PUBLIC METHODES
    // ============================================

    public bool IsWeatherEventActive() => weatherEventActive;
    public List<int> GetWeatherRounds() => roundsWithWeather;
}

public enum WeatherType
{
    Tornado,
    Hitte,
    Regen,
    Vorst,
    Storm
}