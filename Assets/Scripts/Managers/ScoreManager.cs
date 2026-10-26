using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    [Header("References")]
    public TurnManager turnManager;
    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI neighborhoodScoreText;

    private readonly List<ScoreRow> scoreRows = new List<ScoreRow>();

    private class ScoreRow
    {
        public GameObject gameObject;
        public Image background;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI scoreText;
    }

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
            playerScoreText.text = "PLAYERS";
            foreach (ScoreRow row in scoreRows)
            {
                row.gameObject.SetActive(false);
            }
        }

        if (neighborhoodScoreText != null)
        {
            neighborhoodScoreText.text = "Neighbourhood Score: 0";
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

        if (turnManager.players == null) return;

        Player currentPlayer = turnManager.GetCurrentPlayer();
        int totalScore = 0;
        int rowIndex = 0;

        foreach (Player player in turnManager.players)
        {
            if (player == null) continue;

            player.score = ScoreCalculator.CalculatePlayerScore(player);
            totalScore += player.score;

            if (playerScoreText != null)
            {
                if (rowIndex == scoreRows.Count)
                    scoreRows.Add(CreateScoreRow(rowIndex));

                ScoreRow row = scoreRows[rowIndex];
                bool isActive = player == currentPlayer;
                row.gameObject.SetActive(true);
                row.background.color = isActive
                    ? new Color(1f, 0.88f, 0.49f, 0.9f)
                    : new Color(1f, 1f, 1f, 0.3f);
                row.nameText.text = player.playerName;
                row.nameText.fontStyle = isActive ? FontStyles.Bold : FontStyles.Normal;
                row.scoreText.text = player.score.ToString();
                row.scoreText.fontStyle = isActive ? FontStyles.Bold : FontStyles.Normal;
                rowIndex++;
            }
        }

        if (playerScoreText != null)
        {
            playerScoreText.text = "PLAYERS";
            for (int i = rowIndex; i < scoreRows.Count; i++)
                scoreRows[i].gameObject.SetActive(false);

            ResizeScorePanel(rowIndex);
        }

        if (neighborhoodScoreText != null)
        {
            neighborhoodScoreText.text = $"Total score: {totalScore}";
        }
    }

    private ScoreRow CreateScoreRow(int index)
    {
        GameObject rowObject = new GameObject("Player Score Row", typeof(RectTransform), typeof(Image));
        RectTransform rowRect = rowObject.GetComponent<RectTransform>();
        rowRect.SetParent(playerScoreText.rectTransform, false);
        rowRect.anchorMin = new Vector2(0f, 1f);
        rowRect.anchorMax = new Vector2(0f, 1f);
        rowRect.pivot = new Vector2(0f, 1f);
        rowRect.anchoredPosition = new Vector2(0f, -29f - index * 31f);
        rowRect.sizeDelta = new Vector2(205f, 28f);

        Image background = rowObject.GetComponent<Image>();
        background.raycastTarget = false;

        return new ScoreRow
        {
            gameObject = rowObject,
            background = background,
            nameText = CreateRowText(rowRect, "Name", 8f, -46f, TextAlignmentOptions.MidlineLeft),
            scoreText = CreateRowText(rowRect, "Score", 161f, -8f, TextAlignmentOptions.MidlineRight)
        };
    }

    private TextMeshProUGUI CreateRowText(RectTransform rowRect, string objectName,
        float left, float right, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.SetParent(rowRect, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(left, 0f);
        textRect.offsetMax = new Vector2(right, 0f);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = playerScoreText.font;
        text.fontSize = 20f;
        text.enableAutoSizing = true;
        text.fontSizeMin = 14f;
        text.fontSizeMax = 20f;
        text.alignment = alignment;
        text.color = new Color(0.08f, 0.16f, 0.18f);
        text.richText = false;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.raycastTarget = false;
        return text;
    }

    private void ResizeScorePanel(int playerCount)
    {
        RectTransform panel = playerScoreText.rectTransform.parent as RectTransform;
        if (panel == null) return;

        float height = 195f + playerCount * 31f;
        panel.sizeDelta = new Vector2(panel.sizeDelta.x, height);
        panel.anchoredPosition = new Vector2(panel.anchoredPosition.x, -5f - height / 2f);
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
