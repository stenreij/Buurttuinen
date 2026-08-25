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

    [Header("PowerUp Button Visuals")]
    public Color powerUpDefaultColor = Color.white;
    public Color powerUpSelectedColor = new Color(1f, 0.8f, 0.2f, 1f);
    public Color powerUpDisabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    public float powerUpPulseSpeed = 1f;

    private int currentRound = 1;
    public int maxRounds = 10;

    private bool isGamePaused = false;

    private bool isPowerUpSelected = false;
    private Image powerUpButtonImage;
    private Text powerUpButtonText;
    private float pulseTimer = 0f;
    private int lastProcessedRound = 0;

    private bool isTradeActive = false;

    void Update()
    {
        if (isPowerUpSelected && powerUpButtonImage != null && powerUpButton != null)
        {
            pulseTimer += Time.deltaTime * powerUpPulseSpeed;
            float pulse = Mathf.Sin(pulseTimer) * 0.15f + 0.85f;

            Color pulsedColor = powerUpSelectedColor;
            pulsedColor.a = pulse;
            powerUpButtonImage.color = pulsedColor;

            float scale = 1f + Mathf.Sin(pulseTimer) * 0.05f;
            powerUpButton.transform.localScale = new Vector3(scale, scale, 1f);
        }
    }

    public void Initialize(List<Player> playerList)
    {
        players = playerList;
        if (players.Count == 0)
        {
            Debug.LogError("No players found in TurnManager!");
            return;
        }

        if (powerUpButton != null)
        {
            powerUpButtonImage = powerUpButton.GetComponent<Image>();
            powerUpButtonText = powerUpButton.GetComponentInChildren<Text>();
        }

        RandomizeStartingPlayer();

        OnRoundStarted?.Invoke(currentRound);

        StartTurn();
    }

    public void StartTurn()
    {
        if (players == null || players.Count == 0)
        {
            Debug.LogError("No players in TurnManager!");
            return;
        }

        if (currentRound > maxRounds)
        {
            return;
        }

        if (isGamePaused)
        {
            UpdateActionButtons();
            return;
        }

        isPowerUpSelected = false;
        ResetPowerUpButtonVisuals();

        hasPlacedItemThisTurn = false;
        isTradeActive = false;
        currentPlayer = players[currentPlayerIndex];

        if (roundText != null)
        {
            roundText.text = $"Round {currentRound} / {maxRounds}";
        }

        if (turnText != null)
        {
            turnText.text = $"Turn: {currentPlayer.gameObject.name}";
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

        if (currentRound != lastProcessedRound)
        {
            lastProcessedRound = currentRound;

            CommunityManager communityManager = FindFirstObjectByType<CommunityManager>();
            if (communityManager != null)
            {
                communityManager.OnRoundStarted(currentRound);
            }

            WeatherManager weatherManager = FindFirstObjectByType<WeatherManager>();
            if (weatherManager != null)
            {
                weatherManager.OnRoundStarted(currentRound);
            }
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

        Debug.Log($"{currentPlayer.gameObject.name} taking their turn");
    }

    public void UpdateActionButtons()
    {
        bool canAct = !hasPlacedItemThisTurn && !isGamePaused && !isTradeActive;

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
                    ItemData selectedPowerUp = powerActions.GetSelectedPowerUp();
                    bool hasPowerUp = selectedPowerUp != null;
                    bool canUsePowerUp = canAct && hasPowerUp;

                    powerUpButton.interactable = canUsePowerUp;

                    if (hasPowerUp && canAct)
                    {
                        isPowerUpSelected = true;

                        if (powerUpButtonText != null)
                        {
                            powerUpButtonText.text = $"{selectedPowerUp.itemName}";
                        }
                    }
                    else
                    {
                        isPowerUpSelected = false;
                        ResetPowerUpButtonVisuals();

                        if (powerUpButtonText != null)
                        {
                            powerUpButtonText.text = "POWERUP";
                        }
                    }
                }
                else
                {
                    powerUpButton.interactable = false;
                    isPowerUpSelected = false;
                    ResetPowerUpButtonVisuals();

                    if (powerUpButtonText != null)
                    {
                        powerUpButtonText.text = "POWERUP";
                    }
                }
            }
            else
            {
                powerUpButton.interactable = false;
                isPowerUpSelected = false;
                ResetPowerUpButtonVisuals();

                if (powerUpButtonText != null)
                {
                    powerUpButtonText.text = "POWERUP";
                }
            }
        }
    }

    void ResetPowerUpButtonVisuals()
    {
        if (powerUpButtonImage != null)
        {
            powerUpButtonImage.color = powerUpDefaultColor;
        }
        if (powerUpButton != null)
        {
            powerUpButton.transform.localScale = Vector3.one;
        }

        if (powerUpButtonText != null)
        {
            powerUpButtonText.text = "POWERUP";
        }
    }

    public void SetGamePaused(bool paused)
    {
        isGamePaused = paused;
        UpdateActionButtons();
    }

    public bool IsGamePaused()
    {
        return isGamePaused;
    }

    public void OnPowerUpClicked()
    {
        if (isGamePaused) return;
        if (hasPlacedItemThisTurn) return;
        if (isTradeActive) return;

        if (powerUpButtonImage != null)
        {
            StartCoroutine(FlashPowerUpButton());
        }

        PowerUpActions powerActions = currentPlayer.GetComponent<PowerUpActions>();
        if (powerActions == null) return;

        ItemData selectedPowerUp = powerActions.GetSelectedPowerUp();
        if (selectedPowerUp == null)
        {
            StartCoroutine(ShowPowerUpWarning());
            return;
        }

        powerActions.ExecuteSelectedPowerUp();

        isPowerUpSelected = false;
        ResetPowerUpButtonVisuals();

        UpdateActionButtons();

        if (inventoryUI != null)
        {
            inventoryUI.ClearSelectedItem();
            inventoryUI.RefreshUI();
        }
    }

    System.Collections.IEnumerator FlashPowerUpButton()
    {
        if (powerUpButtonImage == null) yield break;

        Color originalColor = powerUpButtonImage.color;
        powerUpButtonImage.color = Color.green;
        yield return new WaitForSeconds(0.1f);
        powerUpButtonImage.color = originalColor;
        yield return new WaitForSeconds(0.1f);
        powerUpButtonImage.color = Color.green;
        yield return new WaitForSeconds(0.1f);
        powerUpButtonImage.color = originalColor;
    }

    System.Collections.IEnumerator ShowPowerUpWarning()
    {
        if (powerUpButtonImage == null) yield break;

        Color originalColor = powerUpButtonImage.color;
        for (int i = 0; i < 3; i++)
        {
            powerUpButtonImage.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            powerUpButtonImage.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void OnPassClicked()
    {
        if (isGamePaused) return;
        if (isTradeActive) return;

        isPowerUpSelected = false;
        ResetPowerUpButtonVisuals();

        ItemActions actions = currentPlayer.GetComponent<ItemActions>();
        if (actions != null)
        {
            actions.PassTurn();
        }
        else
        {
            EndTurn();
        }
    }

    public void OnTradeClicked()
    {
        if (isGamePaused) return;
        if (hasPlacedItemThisTurn)
        {
            Debug.Log("Already performed an action this turn.");
            return;
        }
        if (isTradeActive) return;

        if (TradeManager.Instance == null)
        {
            Debug.LogWarning("TradeManager not found.");
            return;
        }

        if (TradeManager.Instance.GetState() != TradeManager.TradeState.Idle)
        {
            TradeManager.Instance.CancelTrade("Reset trade state.");
        }

        // Start trade - item selection will happen via inventory click
        TradeManager.Instance.StartTrade(currentPlayer);

        Debug.Log($"{currentPlayer.gameObject.name} initiated a trade. Select an item from your inventory.");

        isTradeActive = true;
        UpdateActionButtons();
    }

    public void OnEndTurnClicked()
    {
        if (isGamePaused) return;

        if (isTradeActive)
        {
            Debug.Log("Cannot end turn while a trade is in progress.");
            return;
        }

        isPowerUpSelected = false;
        ResetPowerUpButtonVisuals();

        EndTurn();
    }

    public void EndTurn()
    {
        if (isGamePaused) return;

        hasPlacedItemThisTurn = false;
        isTradeActive = false;

        ItemActions actions = currentPlayer.GetComponent<ItemActions>();
        if (actions != null)
        {
            actions.GiveRandomItem();
            Debug.Log($"{currentPlayer.gameObject.name} received a random item");
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
                Debug.Log($"Game ended - Round {currentRound} completed");

                ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
                if (scoreManager != null)
                {
                    scoreManager.SaveFinalScores();
                }

                SceneManager.LoadScene("EndScene");
                return;
            }

            currentRound++;
            Debug.Log($"Round {currentRound} started");

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
    }

    private void RandomizeStartingPlayer()
    {
        if (players.Count == 0) return;

        int randomStartIndex = Random.Range(0, players.Count);
        currentPlayerIndex = randomStartIndex;
        startingPlayer = players[randomStartIndex];

        Debug.Log($"{startingPlayer.gameObject.name} starts the game");
    }

    public void ResumeTurnAfterWeather()
    {
        isPowerUpSelected = false;
        ResetPowerUpButtonVisuals();

        if (inventoryUI != null)
        {
            inventoryUI.RefreshUI();
        }

        if (scoreManager != null)
        {
            scoreManager.UpdateScores();
        }

        StartTurn();
        Debug.Log($"Turn resumed for {currentPlayer.gameObject.name}");
    }

    public void OnTradeCompleted(bool success)
    {
        isTradeActive = false;

        if (success)
        {
            hasPlacedItemThisTurn = true;
        }
        else
        {
            hasPlacedItemThisTurn = false;
        }

        UpdateActionButtons();

        if (inventoryUI != null)
        {
            inventoryUI.RefreshUI();
        }

        Debug.Log($"Trade completed (success: {success}) - turn action marked");
    }
}