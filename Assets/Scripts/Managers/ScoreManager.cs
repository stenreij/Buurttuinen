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
    }

    public void ResetScores()
    {
        if (playerScoreText != null)
        {
            playerScoreText.text = "Score: 0";
        }

        if (neighborhoodScoreText != null)
        {
            neighborhoodScoreText.text = "Neighborhood score: 0";
        }

        if (turnManager != null && turnManager.players != null)
        {
            foreach (Player player in turnManager.players)
            {
                player.score = 0;
            }
        }

        Debug.Log("🗑️ All scores reset to 0");
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

        int playerScore = ScoreCalculator.CalculatePlayerScore(currentPlayer);
        currentPlayer.score = playerScore;

        if (playerScoreText != null)
        {
            playerScoreText.text = $"Score: {playerScore}";
        }

        int totalScore = ScoreCalculator.CalculateTotalScore(turnManager.players);

        if (neighborhoodScoreText != null)
        {
            neighborhoodScoreText.text = $"Neighborhood score: {totalScore}";
        }
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
            Debug.LogWarning("No players found to save scores for!");
            return;
        }

        int totalScore = ScoreCalculator.CalculateTotalScore(turnManager.players);

        foreach (Player player in turnManager.players)
        {
            int score = ScoreCalculator.CalculatePlayerScore(player);
            setup.finalScores[player.gameObject.name] = score;
        }

        setup.totalScore = totalScore;
    }
}