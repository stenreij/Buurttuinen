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
        Debug.Log("📋 EndScene Loaded!");

        if (backToMenuButton != null)
        {
            backToMenuButton.onClick.RemoveAllListeners();
            backToMenuButton.onClick.AddListener(BackToMenu);
            //Debug.Log("🔘 Back to Menu button ready!");
        }
        else
        {
            Debug.LogError("❌ backToMenuButton is NULL!");
            return;
        }

        GameSetup setup = FindFirstObjectByType<GameSetup>();
        if (setup == null || setup.finalScores == null || setup.finalScores.Count == 0)
        {
            Debug.Log("⚠️ No scores found!");
            if (scoreText != null)
                scoreText.text = "Geen scores beschikbaar!";
            return;
        }

        DisplayScores(setup.finalScores);
        DisplayNeighborhoodScore(setup.totalScore);
    }

    void DisplayScores(Dictionary<string, int> scores)
    {
        var sortedScores = scores.OrderByDescending(x => x.Value).ToList();

        string result = "";
        int rank = 1;

        Debug.Log("🏆 EINDSTAND:");
        foreach (var entry in sortedScores)
        {
            string medal = GetMedal(rank);
            Debug.Log($"{medal} {entry.Key} → {entry.Value} punten");
            result += $"{entry.Key} | {entry.Value}\n";
            rank++;
        }

        if (scoreText != null)
        {
            scoreText.text = result;
        }
    }

    void DisplayNeighborhoodScore(int totalScore)
    {
        if (neighborhoodScoreText != null)
        {
            neighborhoodScoreText.text = $"Buurtscore | {totalScore}";
            Debug.Log($"🌍 Buurtscore | {totalScore} punten");
        }
    }

    string GetMedal(int rank)
    {
        switch (rank)
        {
            case 1: return "🥇";
            case 2: return "🥈";
            case 3: return "🥉";
            default: return $"  {rank}.";
        }
    }

    void BackToMenu()
    {
        Debug.Log("🔙 Terug naar StartMenu...");
        GameSetup setup = FindFirstObjectByType<GameSetup>();
        if (setup != null)
        {
            Destroy(setup.gameObject);
        }
        SceneManager.LoadScene("StartMenu");
    }
}