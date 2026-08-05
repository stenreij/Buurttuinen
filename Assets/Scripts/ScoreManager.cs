using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    [Header("References")]
    public TurnManager turnManager;
    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI neighborhoodScoreText;

    void Start()
    {
        if (turnManager == null)
        {
            turnManager = FindFirstObjectByType<TurnManager>();
        }
    }

    public void ResetScores()
    {
        if (playerScoreText != null)
        {
            playerScoreText.text = "Score: 0";
        }

        if (neighborhoodScoreText != null)
        {
            neighborhoodScoreText.text = "Buurt score: 0";
        }

        if (turnManager != null && turnManager.players != null)
        {
            foreach (Player player in turnManager.players)
            {
                player.score = 0;
            }
        }

        Debug.Log("🔄 Alle scores gereset naar 0!");
    }

    public void UpdateScores()
    {
        if (turnManager == null)
        {
            turnManager = FindFirstObjectByType<TurnManager>();
            if (turnManager == null) return;
        }

        Player currentPlayer = turnManager.GetCurrentPlayer();
        if (currentPlayer == null) return;

        int playerScore = CalculatePlayerScore(currentPlayer);

        currentPlayer.score = playerScore;

        if (playerScoreText != null)
        {
            playerScoreText.text = $"Score: {playerScore}";
        }

        int totalScore = 0;
        foreach (Player player in turnManager.players)
        {
            totalScore += CalculatePlayerScore(player);
        }

        if (neighborhoodScoreText != null)
        {
            neighborhoodScoreText.text = $"Buurt score: {totalScore}";
        }
    }

    public int CalculatePlayerScore(Player player)
{
    int total = 0;

    if (player == null || player.assignedGarden == null)
    {
        Debug.Log($"⚠️ {player?.gameObject.name} heeft geen tuin!");
        return 0;
    }

    GardenTile[] tiles = player.assignedGarden.GetComponentsInChildren<GardenTile>();
    
    Debug.Log($"🔍 ===== SCORE BEREKENING VOOR {player.gameObject.name} =====");
    Debug.Log($"🔍 Aantal tiles in tuin: {tiles.Length}");

    int occupiedCount = 0;
    int itemCount = 0;

    foreach (GardenTile tile in tiles)
    {
        if (tile.occupied)
        {
            occupiedCount++;
            if (tile.placedItemData != null)
            {
                total += tile.placedItemData.score;
                itemCount++;
                Debug.Log($"   ✅ {tile.placedItemData.itemName} (+{tile.placedItemData.score}) op {tile.name}");
            }
            else
            {
                Debug.LogWarning($"   ⚠️ Tile {tile.name} is occupied maar placedItemData is NULL!");
            }
        }
    }

    Debug.Log($"📊 {player.gameObject.name}: {occupiedCount} bezette tiles, {itemCount} items, totaal {total} punten");
    Debug.Log($"🔍 ===== EINDE SCORE BEREKENING =====");
    
    return total;
}

    public int CalculateTotalScore()
    {
        int total = 0;
        foreach (Player player in turnManager.players)
        {
            total += CalculatePlayerScore(player);
        }
        return total;
    }
}