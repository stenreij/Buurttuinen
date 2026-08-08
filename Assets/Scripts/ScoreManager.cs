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

    public static void RefreshScores()
    {
        ScoreManager instance = FindFirstObjectByType<ScoreManager>();
        if (instance != null)
        {
            instance.UpdateScores();
        }
        else
        {
            Debug.LogWarning("⚠️ ScoreManager niet gevonden!");
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

        int totalScore = CalculateTotalScore();

        if (neighborhoodScoreText != null)
        {
            neighborhoodScoreText.text = $"Buurt score: {totalScore}";
        }
    }

    public int CalculatePlayerScore(Player player)
    {
        int total = 0;

        if (player == null || player.assignedGarden == null) return 0;

        GardenTile[] tiles = player.assignedGarden.GetComponentsInChildren<GardenTile>();

        foreach (GardenTile tile in tiles)
        {
            if (tile.occupied && tile.placedItemData != null)
            {
                total += tile.placedItemData.score + player.soilBoost;
            }
        }

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

    public void SaveFinalScores()
    {
        GameSetup setup = FindFirstObjectByType<GameSetup>();
        if (setup == null)
        {
            GameObject setupGO = new GameObject("GameSetup");
            setup = setupGO.AddComponent<GameSetup>();
            DontDestroyOnLoad(setupGO);
        }

        setup.finalScores.Clear();

        if (turnManager == null)
        {
            turnManager = FindFirstObjectByType<TurnManager>();
        }

        if (turnManager == null || turnManager.players == null)
        {
            Debug.LogWarning("⚠️ No players found to save scores for!");
            return;
        }

        int totalScore = CalculateTotalScore();

        foreach (Player player in turnManager.players)
        {
            int score = CalculatePlayerScore(player);
            setup.finalScores[player.gameObject.name] = score;
        }

        setup.totalScore = totalScore;
    }
}