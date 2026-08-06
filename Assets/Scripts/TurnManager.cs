using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement; 

public class TurnManager : MonoBehaviour
{
    [Header("References")]
    public List<Player> players;
    public InventoryUI inventoryUI;
    public ScoreManager scoreManager;

    private int currentPlayerIndex = 0;
    private Player currentPlayer;
    private Player startingPlayer;
    public bool hasPlacedItemThisTurn = false;

    [Header("UI Buttons")]
    public Button passButton;
    public Button tradeButton;
    public Button endTurnButton;

    [Header("UI Text")]
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI turnText;

    private int currentRound = 1;
    public int maxRounds = 10;

    public void Initialize(List<Player> playerList)
    {
        players = playerList;
        if (players.Count == 0)
        {
            Debug.LogError("❌ No players found in TurnManager!");
            return;
        }

        RandomizeStartingPlayer();
        StartTurn();
    }

    public void StartTurn()
    {
        if (players == null || players.Count == 0)
        {
            Debug.LogError("❌ No players in TurnManager!");
            return;
        }

        if (currentRound > maxRounds)
        {
            Debug.Log("🏁 Game is already over!");
            return;
        }

        hasPlacedItemThisTurn = false;
        currentPlayer = players[currentPlayerIndex];

        if (roundText != null)
        {
            roundText.text = $"Ronde {currentRound} / {maxRounds}";
        }

        if (turnText != null)
        {
            turnText.text = $"Beurt: {currentPlayer.gameObject.name}";
        }


        if (scoreManager != null)
        {
            scoreManager.UpdateScores();
        }


        if (inventoryUI != null)
        {
            inventoryUI.playerInventory = currentPlayer.GetComponent<Inventory>();
            inventoryUI.RefreshUI();
        }

        UpdateActionButtons();

        if (passButton != null)
        {
            passButton.onClick.RemoveAllListeners();
            passButton.onClick.AddListener(OnPassClicked);
        }
        if (tradeButton != null)
        {
            tradeButton.onClick.RemoveAllListeners();
            tradeButton.onClick.AddListener(OnTradeClicked);
        }
        if (endTurnButton != null)
        {
            endTurnButton.onClick.RemoveAllListeners();
            endTurnButton.onClick.AddListener(OnEndTurnClicked);
        }

        Debug.Log($"🎮 {currentPlayer.gameObject.name} is now taking their turn.");
    }

    public void UpdateActionButtons()
    {
        bool canAct = !hasPlacedItemThisTurn;

        if (passButton != null)
            passButton.interactable = canAct;

        if (tradeButton != null)
            tradeButton.interactable = canAct;

        if (endTurnButton != null)
            endTurnButton.interactable = true;
    }

    public void OnPassClicked()
    {
        Debug.Log($"⏭️ {currentPlayer.gameObject.name} clicked PASS");

        ItemActions actions = currentPlayer.GetComponent<ItemActions>();
        if (actions != null)
        {
            actions.PassTurn();
        }
        else
        {
            Debug.LogWarning("⚠️ No ItemActions found!");
            EndTurn();
        }
    }

    public void OnTradeClicked()
    {
        Debug.Log($"🔄 {currentPlayer.gameObject.name} clicked TRADE (not implemented yet)");
    }

    public void OnEndTurnClicked()
    {
        Debug.Log($"⏹️ {currentPlayer.gameObject.name} clicked END TURN");
        EndTurn();
    }

    public void EndTurn()
    {
        hasPlacedItemThisTurn = false;

        ItemActions actions = currentPlayer.GetComponent<ItemActions>();
        if (actions != null)
        {
            actions.GiveRandomItem();
            Debug.Log($"🎁 {currentPlayer.gameObject.name} received a random item at end of turn!");
        }

        currentPlayerIndex++;
        if (currentPlayerIndex >= players.Count)
        {
            currentPlayerIndex = 0;
        }

        if (players[currentPlayerIndex] == startingPlayer)
        {
            if (currentRound >= maxRounds)
            {
                Debug.Log($"🏁 GAME HAS ENDED! Ronde {currentRound} is voltooid!");

                ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
                if (scoreManager != null)
                {
                    scoreManager.SaveFinalScores();
                    //Debug.Log("📊 Scores opgeslagen!");
                }

                SceneManager.LoadScene("EndScene");
                return;
            }

            currentRound++;
            Debug.Log($"🔄 New round: {currentRound}/{maxRounds}");
        }

        if (inventoryUI != null)
        {
            inventoryUI.RefreshUI();
        }

        StartTurn();
    }

    public Player GetCurrentPlayer()
    {
        return currentPlayer;
    }

    public void ResetPlacedItemFlag()
    {
        hasPlacedItemThisTurn = false;
        UpdateActionButtons();
    }

    public void SetMaxRounds(int rounds)
    {
        maxRounds = rounds;
        Debug.Log($"📋 Max rounds set on: {maxRounds}");
    }

    private void RandomizeStartingPlayer()
    {
        if (players.Count == 0) return;

        int randomStartIndex = Random.Range(0, players.Count);
        currentPlayerIndex = randomStartIndex;
        startingPlayer = players[randomStartIndex];

        Debug.Log($"🎲 {startingPlayer.gameObject.name} starts the game!");
    }
}