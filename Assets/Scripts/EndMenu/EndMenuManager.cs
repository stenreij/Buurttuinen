using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class EndMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI neighborhoodScoreText;
    public Button backToMenuButton;

    void Start()
    {
        if (backToMenuButton != null)
        {
            backToMenuButton.onClick.RemoveAllListeners();
            backToMenuButton.onClick.AddListener(BackToMenu);
        }
        else
        {
            Debug.LogError("BackToMenuButton is not assigned!");
            return;
        }

        GameSetup setup = FindFirstObjectByType<GameSetup>();
        if (setup == null || setup.finalScores == null || setup.finalScores.Count == 0)
        {
            Debug.LogWarning("No scores found to display.");
            if (scoreText != null)
                scoreText.text = "No scores available!";
            return;
        }

        DisplayScores(setup.finalScores);
        DisplayNeighborhoodScore(setup.totalScore);
    }

    private void DisplayScores(Dictionary<string, int> scores)
    {
        var sortedScores = scores.OrderByDescending(x => x.Value).ToList();

        string result = "";
        int rank = 1;

        foreach (var entry in sortedScores)
        {
            string medal = GetMedal(rank);
            result += $"{medal} {entry.Key}: {entry.Value}\n";
            rank++;
        }

        if (scoreText != null)
        {
            scoreText.text = result;
        }
    }

    private void DisplayNeighborhoodScore(int totalScore)
    {
        if (neighborhoodScoreText != null)
        {
            neighborhoodScoreText.text = $"Neighborhood Score: {totalScore}";
        }
    }

    private string GetMedal(int rank)
    {
        switch (rank)
        {
            case 1: return "1st";
            case 2: return "2nd";
            case 3: return "3rd";
            default: return $"{rank}.";
        }
    }

    private void BackToMenu()
    {
        GameSetup setup = FindFirstObjectByType<GameSetup>();
        if (setup != null)
        {
            Destroy(setup.gameObject);
        }
        SceneManager.LoadScene("StartMenu");
    }
}