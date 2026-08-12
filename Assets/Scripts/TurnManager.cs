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
    public System.Action<int> OnRoundStarted;
    private int currentPlayerIndex = 0;
    private Player currentPlayer;
    private Player startingPlayer;
    public bool hasPlacedItemThisTurn = false;
    public bool hasUsedExtraAction = false;

    [Header("UI Buttons")]
    public Button passButton;
    public Button tradeButton;
    public Button endTurnButton;
    public Button powerUpButton;

    [Header("UI Text")]
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI turnText;

    private int currentRound = 1;
    public int maxRounds = 10;

    private bool isGamePaused = false;

    public void Initialize(List<Player> playerList)
    {
        players = playerList;
        if (players.Count == 0)
        {
            Debug.LogError("❌ No players found in TurnManager!");
            return;
        }

        RandomizeStartingPlayer();

        OnRoundStarted?.Invoke(currentRound);

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

        if (isGamePaused)
        {
            Debug.Log("⏸️ Game is gepauzeerd (weer)");
            UpdateActionButtons();
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

        if (powerUpButton != null)
        {
            powerUpButton.onClick.RemoveAllListeners();
            powerUpButton.onClick.AddListener(OnPowerUpClicked);
        }

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
        bool canAct = !hasPlacedItemThisTurn && !isGamePaused;

        if (passButton != null)
            passButton.interactable = canAct;

        if (tradeButton != null)
            tradeButton.interactable = canAct;

        if (endTurnButton != null)
            endTurnButton.interactable = !isGamePaused;

        if (powerUpButton != null)
        {
            Player currentPlayer = GetCurrentPlayer();
            if (currentPlayer != null)
            {
                PowerUpActions powerActions = currentPlayer.GetComponent<PowerUpActions>();
                if (powerActions != null)
                {
                    powerUpButton.interactable = powerActions.GetSelectedPowerUp() != null && canAct;
                }
                else
                {
                    powerUpButton.interactable = false;
                }
            }
            else
            {
                powerUpButton.interactable = false;
            }
        }
    }

    public void SetGamePaused(bool paused)
    {
        isGamePaused = paused;
        UpdateActionButtons();

        if (paused)
        {
            Debug.Log("⏸️ Game gepauzeerd (weer)");
        }
        else
        {
            Debug.Log("▶️ Game hervat na weer");
        }
    }

    public void OnPowerUpClicked()
    {
        if (isGamePaused)
        {
            Debug.Log("⏸️ Even wachten, game is gepauzeerd!");
            return;
        }

        Debug.Log($"⚡ {currentPlayer.gameObject.name} clicked POWER UP!");

        PowerUpActions powerActions = currentPlayer.GetComponent<PowerUpActions>();
        if (powerActions == null)
        {
            Debug.Log("⚠️ Geen PowerUpActions gevonden!");
            return;
        }

        powerActions.ExecuteSelectedPowerUp();
    }

    public void OnPassClicked()
    {
        if (isGamePaused)
        {
            Debug.Log("⏸️ Even wachten, game is gepauzeerd!");
            return;
        }

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
        if (isGamePaused)
        {
            Debug.Log("⏸️ Even wachten, game is gepauzeerd!");
            return;
        }

        Debug.Log($"🔄 {currentPlayer.gameObject.name} clicked TRADE (not implemented yet)");
    }

    public void OnEndTurnClicked()
    {
        if (isGamePaused)
        {
            Debug.Log("⏸️ Even wachten, game is gepauzeerd!");
            return;
        }

        Debug.Log($"⏹️ {currentPlayer.gameObject.name} clicked END TURN");
        EndTurn();
    }

    public void EndTurn()
    {
        if (isGamePaused)
        {
            Debug.Log("⏸️ Game is gepauzeerd, einde beurt uitgesteld!");
            return;
        }

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
                }

                SceneManager.LoadScene("EndScene");
                return;
            }

            currentRound++;
            Debug.Log($"🔄 New round: {currentRound}/{maxRounds}");

            OnRoundStarted?.Invoke(currentRound);
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

    public void ResumeTurnAfterWeather()
    {
        StartTurn();
        Debug.Log($"🔄 Beurt hervat voor {currentPlayer.gameObject.name} na weerevent");
    }
}