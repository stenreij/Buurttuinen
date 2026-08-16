using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class CommunityManager : MonoBehaviour
{
    [Header("References")]
    public TurnManager turnManager;
    public ScoreManager scoreManager;
    public InventoryUI inventoryUI;
    public WeatherManager weatherManager;
    public TextMeshProUGUI communityAnnouncementText;
    public TextMeshProUGUI communityGoalText;

    [Header("Settings")]
    public int firstCommunityRound = 2;
    public int communityInterval = 2;
    public float announcementDuration = 4f;
    public float goalDisplayDuration = 6f;
    public int maxRounds = 10;

    [Header("Goal Value Ranges (scaled by player count)")]
    public int biodiversityMin = 20;
    public int biodiversityMax = 60;
    public int waterStorageMin = 12;
    public int waterStorageMax = 42;
    public int soilHealthMin = 10;
    public int soilHealthMax = 45;
    public int aestheticsMin = 10;
    public int aestheticsMax = 45;
    public int totalScoreMin = 20;
    public int totalScoreMax = 60;

    private List<CommunityGoal> possibleGoals = new List<CommunityGoal>();
    private CommunityGoal currentGoal;
    private List<CommunityGoalType> usedGoalTypes = new List<CommunityGoalType>();
    private bool isCommunityActive = false;
    private bool isExecutingCommunity = false;
    private bool isWaitingForResult = false;
    private int currentRound = 0;
    private int goalStartRound = 0;
    private int goalEndRound = 0;
    private int goalScoreAtStart = 0;
    private int playerCount = 0;

    void Start()
    {
        if (turnManager == null)
            turnManager = FindFirstObjectByType<TurnManager>();

        if (scoreManager == null)
            scoreManager = FindFirstObjectByType<ScoreManager>();

        if (inventoryUI == null)
            inventoryUI = FindFirstObjectByType<InventoryUI>();

        if (weatherManager == null)
            weatherManager = FindFirstObjectByType<WeatherManager>();
    }

    public void InitializeGoals()
    {
        if (turnManager != null && turnManager.players != null && turnManager.players.Count > 0)
        {
            playerCount = turnManager.players.Count;
        }
        else
        {
            playerCount = 2;
        }

        possibleGoals.Clear();

        possibleGoals.Add(new CommunityGoal(
            CommunityGoalType.Biodiversity,
            "Biodiversity",
            "Reach {0} biodiversity points in the neighborhood!",
            (players) =>
            {
                int total = 0;
                foreach (Player player in players)
                {
                    total += GetBiodiversityScore(player);
                }
                return total;
            },
            () => GetRandomScaledGoalValue(biodiversityMin, biodiversityMax)
        ));

        possibleGoals.Add(new CommunityGoal(
            CommunityGoalType.WaterStorage,
            "Water Storage",
            "Reach {0} water storage points in the neighborhood!",
            (players) =>
            {
                int total = 0;
                foreach (Player player in players)
                {
                    total += GetWaterScore(player);
                }
                return total;
            },
            () => GetRandomScaledGoalValue(waterStorageMin, waterStorageMax)
        ));

        possibleGoals.Add(new CommunityGoal(
            CommunityGoalType.SoilHealth,
            "Soil Health",
            "Reach {0} soil health points in the neighborhood!",
            (players) =>
            {
                int total = 0;
                foreach (Player player in players)
                {
                    total += GetSoilScore(player);
                }
                return total;
            },
            () => GetRandomScaledGoalValue(soilHealthMin, soilHealthMax)
        ));

        possibleGoals.Add(new CommunityGoal(
            CommunityGoalType.Aesthetics,
            "Aesthetics",
            "Reach {0} aesthetic points in the neighborhood!",
            (players) =>
            {
                int total = 0;
                foreach (Player player in players)
                {
                    total += GetAestheticScore(player);
                }
                return total;
            },
            () => GetRandomScaledGoalValue(aestheticsMin, aestheticsMax)
        ));

        possibleGoals.Add(new CommunityGoal(
            CommunityGoalType.TotalScore,
            "Total Score",
            "Reach {0} total neighborhood score!",
            (players) =>
            {
                int total = 0;
                foreach (Player player in players)
                {
                    total += GetTotalScore(player);
                }
                return total;
            },
            () => GetRandomScaledGoalValue(totalScoreMin, totalScoreMax)
        ));

        Debug.Log($"Initialized {possibleGoals.Count} community goals for {playerCount} players");
    }

    int GetRandomScaledGoalValue(int min, int max)
    {
        int baseValue = Random.Range(min, max + 1);

        float scale = 0.5f + ((playerCount -1) * 0.5f);
        int scaledValue = Mathf.RoundToInt(baseValue * scale);

        return Mathf.Max(3, scaledValue);
    }

    public void OnRoundStarted(int roundNumber)
    {
        currentRound = roundNumber;

        if (weatherManager != null && weatherManager.IsWeatherEventActive())
        {
            Debug.Log("🏛️ Weather is active, community will wait...");
            return;
        }

        if (isWaitingForResult && currentRound >= goalEndRound)
        {
            CheckGoalResult();
            return;
        }

        if (roundNumber >= firstCommunityRound &&
            (roundNumber - firstCommunityRound) % communityInterval == 0 &&
            !isCommunityActive &&
            !isExecutingCommunity &&
            !isWaitingForResult &&
            roundNumber <= maxRounds)
        {
            StartNewGoal();
        }
    }

    public bool IsExecutingCommunity() => isExecutingCommunity || isCommunityActive;

    public void StartNewGoal()
    {
        isCommunityActive = true;
        isExecutingCommunity = true;

        if (turnManager != null)
        {
            turnManager.SetGamePaused(true);
        }

        CommunityGoal newGoal = SelectUnusedGoal();

        if (newGoal == null)
        {
            Debug.Log("All goals have been used! Resetting goal list...");
            usedGoalTypes.Clear();
            newGoal = SelectUnusedGoal();
        }

        currentGoal = newGoal;
        currentGoal.GenerateValue();

        goalStartRound = currentRound;
        goalEndRound = currentRound + communityInterval;

        if (goalEndRound > maxRounds)
        {
            Debug.Log($"🏛️ Goal would end after game ends ({goalEndRound} > {maxRounds}), skipping...");
            isCommunityActive = false;
            isExecutingCommunity = false;

            if (turnManager != null)
            {
                turnManager.SetGamePaused(false);
                turnManager.ResumeTurnAfterWeather();
            }
            return;
        }

        List<Player> players = turnManager.players;
        goalScoreAtStart = currentGoal.CalculateScore(players);

        string announcement = "COMMUNITY GOAL!";
        ShowCommunityAnnouncement(announcement, announcementDuration);

        ShowGoalText(currentGoal, goalScoreAtStart);

        StartCoroutine(ExecuteNewGoal());
    }

    CommunityGoal SelectUnusedGoal()
    {
        List<CommunityGoal> availableGoals = possibleGoals
            .Where(g => !usedGoalTypes.Contains(g.goalType))
            .ToList();

        if (availableGoals.Count == 0)
        {
            Debug.Log("No unused goals available!");
            return null;
        }

        CommunityGoal selected = availableGoals[Random.Range(0, availableGoals.Count)];
        usedGoalTypes.Add(selected.goalType);

        Debug.Log($"Selected community goal: {selected.goalType} (Used: {usedGoalTypes.Count}/{possibleGoals.Count})");

        return selected;
    }

    IEnumerator ExecuteNewGoal()
    {
        yield return new WaitForSeconds(2f);

        string goalDisplay = "Reach " + currentGoal.goalValue + " " + currentGoal.name.ToLower() + " points!";
        ShowCommunityAnnouncement(goalDisplay, goalDisplayDuration);

        yield return new WaitForSeconds(2f);

        isCommunityActive = false;
        isExecutingCommunity = false;
        isWaitingForResult = true;

        if (turnManager != null)
        {
            turnManager.SetGamePaused(false);
            turnManager.ResumeTurnAfterWeather();
        }

        Debug.Log($"🏛️ Community goal active until round {goalEndRound}: {currentGoal.name} ({goalScoreAtStart}/{currentGoal.goalValue})");

        if (weatherManager != null)
        {
            weatherManager.OnCommunityReady();
        }
    }

    void CheckGoalResult()
    {
        isWaitingForResult = false;
        isExecutingCommunity = true;

        if (turnManager != null)
        {
            turnManager.SetGamePaused(true);
        }

        List<Player> players = turnManager.players;
        int currentScore = currentGoal.CalculateScore(players);
        int scoreGained = currentScore - goalScoreAtStart;
        bool isAchieved = currentScore >= currentGoal.goalValue;

        string resultText;
        if (isAchieved)
        {
            resultText = "GOAL ACHIEVED! Score: " + currentScore + " / " + currentGoal.goalValue + " (+" + scoreGained + ")";
            ShowCommunityAnnouncement(resultText, announcementDuration + 1f);
            StartCoroutine(DelayedReward(players));
        }
        else
        {
            resultText = "GOAL FAILED! Score: " + currentScore + " / " + currentGoal.goalValue + " (+" + scoreGained + ")";
            ShowCommunityAnnouncement(resultText, announcementDuration + 1f);
            StartCoroutine(DelayedPenalty(players));
        }

        if (communityGoalText != null)
        {
            communityGoalText.text = "";
        }

        StartCoroutine(FinishGoalCheck());
    }

    IEnumerator DelayedReward(List<Player> players)
    {
        yield return new WaitForSeconds(2f);
        GiveReward(players);
    }

    IEnumerator DelayedPenalty(List<Player> players)
    {
        yield return new WaitForSeconds(2f);
        GivePenalty(players);
    }

    IEnumerator FinishGoalCheck()
    {
        yield return new WaitForSeconds(3f);

        isExecutingCommunity = false;
        currentGoal = null;

        if (turnManager != null)
        {
            turnManager.SetGamePaused(false);
            turnManager.ResumeTurnAfterWeather();
        }

        UpdateAllUI();
        Debug.Log("Community goal check completed!");

        if (currentRound >= firstCommunityRound &&
            (currentRound - firstCommunityRound) % communityInterval == 0 &&
            !isCommunityActive &&
            !isExecutingCommunity &&
            !isWaitingForResult &&
            currentRound <= maxRounds)
        {
            Debug.Log("🏛️ Starting new goal immediately after finishing previous");
            StartNewGoal();
        }

        if (weatherManager != null)
        {
            weatherManager.OnCommunityReady();
        }
    }

    // ============================================
    // REWARDS
    // ============================================

    void GiveReward(List<Player> players)
    {
        int rewardType = Random.Range(0, 2);

        switch (rewardType)
        {
            case 0:
                GiveRandomItemToAllPlayers(players);
                break;
            case 1:
                GiveScoreBoostToAllItems(players);
                break;
        }
    }

    void GiveRandomItemToAllPlayers(List<Player> players)
    {
        foreach (Player player in players)
        {
            Garden garden = player.assignedGarden;
            if (garden == null) continue;

            ItemData randomItem = FindRandomPlantOrDecoration();
            if (randomItem == null)
            {
                Debug.Log($"No item available for {player.playerName}");
                continue;
            }

            GardenTile emptyTile = GetRandomEmptyTile(player);
            if (emptyTile == null)
            {
                Debug.Log($"No empty space in {player.playerName}'s garden");
                continue;
            }

            GameObject placed = Instantiate(randomItem.prefab, emptyTile.transform.position, Quaternion.identity);
            placed.transform.parent = emptyTile.transform;
            placed.transform.localPosition = Vector3.zero;

            SpriteRenderer sr = placed.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = true;
                sr.sortingOrder = 10;
            }

            emptyTile.placedItem = placed;
            emptyTile.placedItemData = randomItem;
            emptyTile.occupied = true;

            Debug.Log($"Community reward: {player.playerName} received {randomItem.itemName} in their garden");
        }

        ShowCommunityAnnouncement("All players received a random item in their garden!", announcementDuration);
    }

    void GiveScoreBoostToAllItems(List<Player> players)
    {
        int boostAmount = 2;

        foreach (Player player in players)
        {
            Garden garden = player.assignedGarden;
            if (garden == null) continue;

            GardenTile[] allTiles = garden.GetComponentsInChildren<GardenTile>();
            foreach (GardenTile tile in allTiles)
            {
                if (tile.occupied && tile.placedItemData != null)
                {
                    tile.placedItemData.score += boostAmount;
                    Debug.Log($"🏛️ {tile.placedItemData.itemName} in {player.playerName}'s garden +{boostAmount} score (now: {tile.placedItemData.score})");
                }
            }
        }

        ShowCommunityAnnouncement($"All items in every garden get +{boostAmount} score!", announcementDuration);
        Debug.Log($"Community reward: All items +{boostAmount} score");
    }

    ItemData FindRandomPlantOrDecoration()
    {
        ItemDatabase database = FindFirstObjectByType<ItemDatabase>();
        if (database != null && database.allItems != null)
        {
            List<ItemData> goodItems = new List<ItemData>();
            foreach (ItemData item in database.allItems)
            {
                if ((item.type == ItemType.Plant ||
                     item.type == ItemType.Tree_Big ||
                     item.type == ItemType.Tree_Small ||
                     item.type == ItemType.Hedge ||
                     item.type == ItemType.Water ||
                     item.type == ItemType.Decoration) &&
                    database.IsAvailable(item))
                {
                    goodItems.Add(item);
                }
            }
            if (goodItems.Count > 0)
            {
                return goodItems[Random.Range(0, goodItems.Count)];
            }
        }
        return null;
    }

    // ============================================
    // PENALTIES
    // ============================================

    void GivePenalty(List<Player> players)
    {
        int penaltyType = Random.Range(0, 3);

        switch (penaltyType)
        {
            case 0:
                RemoveItemsPenalty(players);
                break;
            case 1:
                BlockItemsPenalty(players);
                break;
            case 2:
                ReduceScorePenalty(players);
                break;
        }
    }

    void RemoveItemsPenalty(List<Player> players)
    {
        int totalRemoved = 0;

        foreach (Player player in players)
        {
            Garden garden = player.assignedGarden;
            if (garden == null) continue;

            List<GardenTile> occupiedTiles = new List<GardenTile>();
            GardenTile[] allTiles = garden.GetComponentsInChildren<GardenTile>();
            foreach (GardenTile tile in allTiles)
            {
                if (tile.occupied && tile.placedItem != null && !tile.isProtected)
                {
                    occupiedTiles.Add(tile);
                }
            }

            int itemsToRemove = Mathf.Min(Random.Range(1, 3), occupiedTiles.Count);
            for (int i = 0; i < itemsToRemove && occupiedTiles.Count > 0; i++)
            {
                int randomIndex = Random.Range(0, occupiedTiles.Count);
                GardenTile targetTile = occupiedTiles[randomIndex];
                occupiedTiles.RemoveAt(randomIndex);

                if (targetTile.placedItem != null)
                    Destroy(targetTile.placedItem);

                targetTile.occupied = false;
                targetTile.placedItem = null;
                targetTile.placedItemData = null;
                totalRemoved++;
            }
        }

        ShowCommunityAnnouncement("Community removes " + totalRemoved + " item(s) from the neighborhood!", announcementDuration);
        Debug.Log("Community penalty: " + totalRemoved + " items removed");
    }

    void BlockItemsPenalty(List<Player> players)
    {
        ItemType[] types = { ItemType.Plant, ItemType.Tree_Big, ItemType.Tree_Small, ItemType.Hedge, ItemType.Water, ItemType.Decoration };
        ItemType blockedType = types[Random.Range(0, types.Length)];

        ShowCommunityAnnouncement("Community blocks " + blockedType + " for 1 round!", announcementDuration);
        Debug.Log("Community penalty: " + blockedType + " is blocked for 1 round");
    }

    void ReduceScorePenalty(List<Player> players)
    {
        foreach (Player player in players)
        {
            player.score = Mathf.Max(0, player.score - 2);
        }
        ShowCommunityAnnouncement("All players lose 2 score points!", announcementDuration);
        Debug.Log("Community penalty: All players -2 score");
    }

    // ============================================
    // SCORE CALCULATION
    // ============================================

    int GetBiodiversityScore(Player player)
    {
        int total = 0;
        Garden garden = player.assignedGarden;
        if (garden == null) return 0;

        GardenTile[] tiles = garden.GetComponentsInChildren<GardenTile>();
        foreach (GardenTile tile in tiles)
        {
            if (tile.occupied && tile.placedItemData != null)
            {
                total += tile.placedItemData.biodiversity;
            }
        }
        return total + player.soilBoost;
    }

    int GetWaterScore(Player player)
    {
        int total = 0;
        Garden garden = player.assignedGarden;
        if (garden == null) return 0;

        GardenTile[] tiles = garden.GetComponentsInChildren<GardenTile>();
        foreach (GardenTile tile in tiles)
        {
            if (tile.occupied && tile.placedItemData != null)
            {
                total += tile.placedItemData.water;
            }
        }
        return total;
    }

    int GetSoilScore(Player player)
    {
        int total = 0;
        Garden garden = player.assignedGarden;
        if (garden == null) return 0;

        GardenTile[] tiles = garden.GetComponentsInChildren<GardenTile>();
        foreach (GardenTile tile in tiles)
        {
            if (tile.occupied && tile.placedItemData != null)
            {
                total += tile.placedItemData.soilHealth;
            }
        }
        return total + player.soilBoost;
    }

    int GetAestheticScore(Player player)
    {
        int total = 0;
        Garden garden = player.assignedGarden;
        if (garden == null) return 0;

        GardenTile[] tiles = garden.GetComponentsInChildren<GardenTile>();
        foreach (GardenTile tile in tiles)
        {
            if (tile.occupied && tile.placedItemData != null)
            {
                total += tile.placedItemData.esthetic;
            }
        }
        return total;
    }

    int GetTotalScore(Player player)
    {
        int total = 0;
        Garden garden = player.assignedGarden;
        if (garden == null) return 0;

        GardenTile[] tiles = garden.GetComponentsInChildren<GardenTile>();
        foreach (GardenTile tile in tiles)
        {
            if (tile.occupied && tile.placedItemData != null)
            {
                total += tile.placedItemData.score;
            }
        }
        return total + player.soilBoost;
    }

    // ============================================
    // HELPER METHODS
    // ============================================

    ItemData FindItemByType(ItemType type)
    {
        ItemDatabase database = FindFirstObjectByType<ItemDatabase>();
        if (database != null && database.allItems != null)
        {
            foreach (ItemData item in database.allItems)
            {
                if (item.type == type && database.IsAvailable(item))
                {
                    return item;
                }
            }
        }
        return null;
    }

    ItemData FindGoodItem()
    {
        ItemDatabase database = FindFirstObjectByType<ItemDatabase>();
        if (database != null && database.allItems != null)
        {
            List<ItemData> goodItems = new List<ItemData>();
            foreach (ItemData item in database.allItems)
            {
                if (item.type != ItemType.Sabotage && item.type != ItemType.PowerUp && database.IsAvailable(item))
                {
                    goodItems.Add(item);
                }
            }
            if (goodItems.Count > 0)
            {
                return goodItems[Random.Range(0, goodItems.Count)];
            }
        }
        return null;
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

    void ShowCommunityAnnouncement(string message, float duration = -1)
    {
        if (communityAnnouncementText != null)
        {
            communityAnnouncementText.text = message;
            communityAnnouncementText.gameObject.SetActive(true);

            float actualDuration = duration > 0 ? duration : announcementDuration;
            StartCoroutine(HideAnnouncementAfterDelay(actualDuration));
        }
        else
        {
            Debug.Log("Community: " + message);
        }
    }

    void ShowGoalText(CommunityGoal goal, int currentScore)
    {
        if (communityGoalText != null)
        {
            string goalDisplay = goal.name + "\n" +
                                 "Reach " + goal.goalValue + " pts" + "\n" +
                                 "Progress: " + currentScore + "/" + goal.goalValue + "\n" +
                                 "Ends turn " + goalEndRound;
            communityGoalText.text = goalDisplay;
            communityGoalText.gameObject.SetActive(true);
        }
    }

    public void UpdateCommunityGoalScore()
    {
        if (currentGoal == null || turnManager == null) return;

        List<Player> players = turnManager.players;
        int currentScore = currentGoal.CalculateScore(players);

        if (communityGoalText != null && communityGoalText.gameObject.activeSelf)
        {
            ShowGoalText(currentGoal, currentScore);
            Debug.Log($"📊 Community goal score updated: {currentScore} / {currentGoal.goalValue}");
        }
    }

    IEnumerator HideAnnouncementAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (communityAnnouncementText != null)
            communityAnnouncementText.gameObject.SetActive(false);
    }

    void UpdateAllUI()
    {
        if (scoreManager != null) scoreManager.UpdateScores();
        if (inventoryUI != null) inventoryUI.RefreshUI();
    }

    public bool IsCommunityActive() => isCommunityActive || isExecutingCommunity || isWaitingForResult;
    public CommunityGoal GetCurrentGoal() => currentGoal;
    public int GetGoalEndRound() => goalEndRound;
    public List<CommunityGoalType> GetUsedGoalTypes() => usedGoalTypes;
}