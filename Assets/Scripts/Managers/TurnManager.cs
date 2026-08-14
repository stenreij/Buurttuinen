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
            Debug.LogError("❌ No players found in TurnManager!");
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
            Debug.Log("⏸️ Game is paused (weather/community)");
            UpdateActionButtons();
            return;
        }

        isPowerUpSelected = false;
        ResetPowerUpButtonVisuals();

        hasPlacedItemThisTurn = false;
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
                    ItemData selectedPowerUp = powerActions.GetSelectedPowerUp();
                    bool hasPowerUp = selectedPowerUp != null;
                    bool canUsePowerUp = canAct && hasPowerUp;

                    powerUpButton.interactable = canUsePowerUp;

                    if (hasPowerUp && canAct)
                    {
                        isPowerUpSelected = true;

                        if (powerUpButtonText != null)
                        {
                            powerUpButtonText.text = $"⚡ {selectedPowerUp.itemName}";
                        }
                    }
                    else
                    {
                        isPowerUpSelected = false;
                        ResetPowerUpButtonVisuals();

                        if (powerUpButtonText != null)
                        {
                            powerUpButtonText.text = "⚡ POWERUP";
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
                        powerUpButtonText.text = "⚡ POWERUP";
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
                    powerUpButtonText.text = "⚡ POWERUP";
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
            powerUpButtonText.text = "⚡ POWERUP";
        }
    }

    public void SetGamePaused(bool paused)
    {
        isGamePaused = paused;
        UpdateActionButtons();

        if (paused)
        {
            Debug.Log("⏸️ Game paused");
        }
        else
        {
            Debug.Log("▶️ Game resumed");
        }
    }

    public bool IsGamePaused()
    {
        return isGamePaused;
    }

    public void OnPowerUpClicked()
    {
        if (isGamePaused)
        {
            Debug.Log("⏸️ Game is paused, cannot use powerup!");
            return;
        }

        if (hasPlacedItemThisTurn)
        {
            Debug.Log("⚠️ You have already performed an action this turn!");
            return;
        }

        Debug.Log($"⚡ {currentPlayer.gameObject.name} clicked POWER UP!");

        if (powerUpButtonImage != null)
        {
            StartCoroutine(FlashPowerUpButton());
        }

        PowerUpActions powerActions = currentPlayer.GetComponent<PowerUpActions>();
        if (powerActions == null)
        {
            Debug.Log("⚠️ No PowerUpActions found!");
            return;
        }

        ItemData selectedPowerUp = powerActions.GetSelectedPowerUp();
        if (selectedPowerUp == null)
        {
            Debug.Log("⚠️ Select a power-up from your inventory first!");
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
        if (isGamePaused)
        {
            Debug.Log("⏸️ Game is paused, cannot pass!");
            return;
        }

        Debug.Log($"⏭️ {currentPlayer.gameObject.name} clicked PASS");

        isPowerUpSelected = false;
        ResetPowerUpButtonVisuals();

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
            Debug.Log("⏸️ Game is paused, cannot trade!");
            return;
        }

        Debug.Log($"🔄 {currentPlayer.gameObject.name} clicked TRADE (not implemented yet)");
    }

    public void OnEndTurnClicked()
    {
        if (isGamePaused)
        {
            Debug.Log("⏸️ Game is paused, cannot end turn!");
            return;
        }

        Debug.Log($"⏹️ {currentPlayer.gameObject.name} clicked END TURN");

        isPowerUpSelected = false;
        ResetPowerUpButtonVisuals();

        EndTurn();
    }

    public void EndTurn()
    {
        if (isGamePaused)
        {
            Debug.Log("⏸️ Game is paused, end turn delayed!");
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
                Debug.Log($"🏁 GAME HAS ENDED! Round {currentRound} completed!");

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
        Debug.Log($"📋 Max rounds set to: {maxRounds}");
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
    // RESUME TURN AFTER WEATHER/COMMUNITY
    // ============================================

    public void ResumeTurnAfterWeather()
    {
        // Reset powerup selection after weather/community event
        isPowerUpSelected = false;
        ResetPowerUpButtonVisuals();

        // Update UI to reflect current state
        if (inventoryUI != null)
        {
            inventoryUI.RefreshUI();
        }

        if (scoreManager != null)
        {
            scoreManager.UpdateScores();
        }

        StartTurn();
        Debug.Log($"🔄 Turn resumed for {currentPlayer.gameObject.name} after event");
    }
}