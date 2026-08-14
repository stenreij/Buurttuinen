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
    public float announcementDuration = 5f;
    public float goalDisplayDuration = 8f;
    public int maxRounds = 10;

    private List<CommunityGoal> possibleGoals = new List<CommunityGoal>();
    private CommunityGoal currentGoal;
    private bool isCommunityActive = false;
    private bool isExecutingCommunity = false;
    private bool isWaitingForResult = false;
    private int currentRound = 0;
    private int goalStartRound = 0;
    private int goalEndRound = 0;
    private int goalScoreAtStart = 0;

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

        InitializeGoals();
    }

    void InitializeGoals()
    {
        possibleGoals.Clear();

        // Goal 1: Biodiversity
        possibleGoals.Add(new CommunityGoal(
            "Biodiversity",
            "Reach {0} biodiversity points in the neighborhood!",
            (players) => {
                int total = 0;
                foreach (Player player in players)
                {
                    total += GetBiodiversityScore(player);
                }
                return total;
            },
            15,
            ""
        ));

        // Goal 2: Water Storage
        possibleGoals.Add(new CommunityGoal(
            "Water Storage",
            "Reach {0} water storage points in the neighborhood!",
            (players) => {
                int total = 0;
                foreach (Player player in players)
                {
                    total += GetWaterScore(player);
                }
                return total;
            },
            12,
            ""
        ));

        // Goal 3: Soil Health
        possibleGoals.Add(new CommunityGoal(
            "Soil Health",
            "Reach {0} soil health points in the neighborhood!",
            (players) => {
                int total = 0;
                foreach (Player player in players)
                {
                    total += GetSoilScore(player);
                }
                return total;
            },
            10,
            ""
        ));

        // Goal 4: Aesthetics
        possibleGoals.Add(new CommunityGoal(
            "Aesthetics",
            "Reach {0} aesthetic points in the neighborhood!",
            (players) => {
                int total = 0;
                foreach (Player player in players)
                {
                    total += GetAestheticScore(player);
                }
                return total;
            },
            8,
            ""
        ));
    }

    public void OnRoundStarted(int roundNumber)
    {
        currentRound = roundNumber;

        // Check if weather is active - if so, delay community
        if (weatherManager != null && weatherManager.IsWeatherEventActive())
        {
            Debug.Log("🏛️ Weather is active, community will wait...");
            return;
        }

        // If we're waiting for a result, check on the end round
        if (isWaitingForResult && currentRound >= goalEndRound)
        {
            CheckGoalResult();
            return;
        }

        // Start a new goal if it's time
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

    public void StartNewGoal()
    {
        isCommunityActive = true;
        isExecutingCommunity = true;

        // Pause the game briefly for announcement
        if (turnManager != null)
        {
            turnManager.SetGamePaused(true);
        }

        // Select a random goal (different from previous if possible)
        CommunityGoal newGoal = possibleGoals[Random.Range(0, possibleGoals.Count)];
        if (possibleGoals.Count > 1 && currentGoal != null)
        {
            while (newGoal == currentGoal)
            {
                newGoal = possibleGoals[Random.Range(0, possibleGoals.Count)];
            }
        }
        currentGoal = newGoal;

        // Store the start round and score
        goalStartRound = currentRound;
        goalEndRound = currentRound + communityInterval;
        
        List<Player> players = turnManager.players;
        goalScoreAtStart = currentGoal.CalculateScore(players);

        // Show announcement
        string announcement = $"COMMUNITY GOAL!";
        ShowCommunityAnnouncement(announcement, announcementDuration);

        // Show the goal text (stays visible)
        ShowGoalText(currentGoal, goalScoreAtStart);

        StartCoroutine(ExecuteNewGoal());
    }

    IEnumerator ExecuteNewGoal()
    {
        yield return new WaitForSeconds(2f);

        // Show the goal details
        string goalDisplay = $"{currentGoal.icon} {currentGoal.name}\n{string.Format(currentGoal.goalText, currentGoal.goalValue)}\nCurrent: {goalScoreAtStart} / {currentGoal.goalValue}";
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

        Debug.Log($"Community goal active until round {goalEndRound}: {currentGoal.name} ({goalScoreAtStart}/{currentGoal.goalValue})");
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

        // Show result
        string resultText;
        if (isAchieved)
        {
            resultText = $"GOAL ACHIEVED! {currentGoal.icon}\nScore: {currentScore} / {currentGoal.goalValue} (+{scoreGained})";
            ShowCommunityAnnouncement(resultText, announcementDuration + 1f);
            StartCoroutine(DelayedReward(players));
        }
        else
        {
            resultText = $"GOAL FAILED! {currentGoal.icon}\nScore: {currentScore} / {currentGoal.goalValue} (+{scoreGained})";
            ShowCommunityAnnouncement(resultText, announcementDuration + 1f);
            StartCoroutine(DelayedPenalty(players));
        }

        // Clear the goal text
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
        Debug.Log($"Community goal check completed!");
    }

    // ============================================
    // REWARDS
    // ============================================

    void GiveReward(List<Player> players)
    {
        int rewardType = Random.Range(0, 3);

        switch (rewardType)
        {
            case 0:
                GivePowerUpReward(players);
                break;
            case 1:
                PlaceGoodItemReward(players);
                break;
            case 2:
                GiveScoreBonusReward(players);
                break;
        }
    }

    void GivePowerUpReward(List<Player> players)
    {
        Player targetPlayer = players[Random.Range(0, players.Count)];
        ItemData powerUpItem = FindItemByType(ItemType.PowerUp);
        
        if (powerUpItem != null && targetPlayer != null)
        {
            Inventory inv = targetPlayer.GetComponent<Inventory>();
            if (inv != null)
            {
                inv.AddItem(powerUpItem);
                ShowCommunityAnnouncement($"{targetPlayer.playerName} receives a POWER-UP: {powerUpItem.itemName}!", announcementDuration);
                Debug.Log($"🏛️ Community reward: {targetPlayer.playerName} receives {powerUpItem.itemName}");
            }
        }
        else
        {
            ShowCommunityAnnouncement($"No power-up available! Score bonus instead.", announcementDuration);
            GiveScoreBonusReward(players);
        }
    }

    void PlaceGoodItemReward(List<Player> players)
    {
        Player targetPlayer = players[Random.Range(0, players.Count)];
        Garden garden = targetPlayer.assignedGarden;
        
        if (garden != null)
        {
            ItemData goodItem = FindGoodItem();
            
            if (goodItem != null)
            {
                GardenTile emptyTile = GetRandomEmptyTile(targetPlayer);
                if (emptyTile != null)
                {
                    GameObject placed = Instantiate(goodItem.prefab, emptyTile.transform.position, Quaternion.identity);
                    placed.transform.parent = emptyTile.transform;
                    placed.transform.localPosition = Vector3.zero;
                    
                    SpriteRenderer sr = placed.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.enabled = true;
                        sr.sortingOrder = 10;
                    }
                    
                    emptyTile.placedItem = placed;
                    emptyTile.placedItemData = goodItem;
                    emptyTile.occupied = true;
                    
                    ShowCommunityAnnouncement($"{targetPlayer.playerName} receives a {goodItem.itemName} in their garden!", announcementDuration);
                    Debug.Log($"Community reward: {goodItem.itemName} placed in {targetPlayer.playerName}'s garden");
                }
                else
                {
                    ShowCommunityAnnouncement($"No empty space! Score bonus instead.", announcementDuration);
                    GiveScoreBonusReward(players);
                }
            }
            else
            {
                ShowCommunityAnnouncement($"No good item available! Score bonus.", announcementDuration);
                GiveScoreBonusReward(players);
            }
        }
        else
        {
            ShowCommunityAnnouncement($"No garden available! Score bonus.", announcementDuration);
            GiveScoreBonusReward(players);
        }
    }

    void GiveScoreBonusReward(List<Player> players)
    {
        foreach (Player player in players)
        {
            player.score += 3;
        }
        ShowCommunityAnnouncement($"All players receive +3 score bonus!", announcementDuration);
        Debug.Log($"Community reward: All players +3 score");
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
        
        ShowCommunityAnnouncement($"Community removes {totalRemoved} item(s) from the neighborhood!", announcementDuration);
        Debug.Log($"🏛️ Community penalty: {totalRemoved} items removed");
    }

    void BlockItemsPenalty(List<Player> players)
    {
        ItemType[] types = { ItemType.Plant, ItemType.Tree_Big, ItemType.Tree_Small, ItemType.Hedge, ItemType.Water, ItemType.Decoration };
        ItemType blockedType = types[Random.Range(0, types.Length)];
        
        ShowCommunityAnnouncement($"Community blocks {blockedType} for 1 round!", announcementDuration);
        Debug.Log($"Community penalty: {blockedType} is blocked for 1 round");
    }

    void ReduceScorePenalty(List<Player> players)
    {
        foreach (Player player in players)
        {
            player.score = Mathf.Max(0, player.score - 2);
        }
        ShowCommunityAnnouncement($"All players lose 2 score points!", announcementDuration);
        Debug.Log($"Community penalty: All players -2 score");
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
            Debug.Log($"{message}");
        }
    }

    void ShowGoalText(CommunityGoal goal, int currentScore)
    {
        if (communityGoalText != null)
        {
            string goalDisplay = $"{goal.icon} COMMUNITY GOAL: {goal.name}\n" +
                                 $"{string.Format(goal.goalText, goal.goalValue)}\n" +
                                 $"Current: {currentScore} / {goal.goalValue}\n" +
                                 $"Complete by round {goalEndRound}";
            communityGoalText.text = goalDisplay;
            communityGoalText.gameObject.SetActive(true);
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
}

// ============================================
// COMMUNITY GOAL CLASS
// ============================================

[System.Serializable]
public class CommunityGoal
{
    public string name;
    public string goalText;
    public System.Func<List<Player>, int> calculateScore;
    public int goalValue;
    public string icon;

    public CommunityGoal(string name, string goalText, System.Func<List<Player>, int> calculateScore, int goalValue, string icon)
    {
        this.name = name;
        this.goalText = goalText;
        this.calculateScore = calculateScore;
        this.goalValue = goalValue;
        this.icon = icon;
    }

    public int CalculateScore(List<Player> players)
    {
        return calculateScore(players);
    }
}