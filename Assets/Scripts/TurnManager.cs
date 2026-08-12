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

    private bool isWeatherEventActive = false;
    private bool isWaitingForWeatherToFinish = false; // Nieuwe vlag

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

        // Als er een weer event actief is, wachten we
        if (isWeatherEventActive)
        {
            Debug.Log("⏳ Weer event actief, wachten tot het afgelopen is...");
            isWaitingForWeatherToFinish = true;
            return;
        }

        // Reset de wacht-vlag als we hier komen
        isWaitingForWeatherToFinish = false;

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
        bool canAct = !hasPlacedItemThisTurn && !isWeatherEventActive;

        if (passButton != null)
            passButton.interactable = canAct;

        if (tradeButton != null)
            tradeButton.interactable = canAct;

        if (endTurnButton != null)
            endTurnButton.interactable = true;

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

    public void OnPowerUpClicked()
    {
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

    // ============================================
    // WEER GERELATEERDE METHODES
    // ============================================

    public void SetWeatherEventActive(bool active)
    {
        isWeatherEventActive = active;
        UpdateActionButtons();
        
        if (active)
        {
            Debug.Log("🌪️ Weer event actief! Spelers kunnen geen acties uitvoeren.");
        }
        else
        {
            Debug.Log("☀️ Weer event voorbij! Spelers kunnen weer acties uitvoeren.");
            
            // Als we aan het wachten waren op het weer om te eindigen,
            // herstart dan de beurt voor dezelfde speler
            if (isWaitingForWeatherToFinish)
            {
                Debug.Log($"🔄 Weer is voorbij, hervat beurt voor {currentPlayer.gameObject.name}");
                StartTurn();
            }
        }
    }

    public bool IsWeatherEventActive()
    {
        return isWeatherEventActive;
    }
}