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

    // 🔥 NIEUW: PowerUp button visuals
    [Header("PowerUp Button Visuals")]
    public Color powerUpDefaultColor = Color.white;
    public Color powerUpSelectedColor = new Color(1f, 0.8f, 0.2f, 1f);
    public Color powerUpDisabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    public float powerUpPulseSpeed = 1f;

    private int currentRound = 1;
    public int maxRounds = 10;

    private bool isGamePaused = false;
    
    // 🔥 NIEUW: PowerUp button state
    private bool isPowerUpSelected = false;
    private Image powerUpButtonImage;
    private Text powerUpButtonText;
    private float pulseTimer = 0f;

    void Update()
    {
        // 🔥 NIEUW: Pulse animatie voor geselecteerde powerup knop
        if (isPowerUpSelected && powerUpButtonImage != null && powerUpButton != null)
        {
            pulseTimer += Time.deltaTime * powerUpPulseSpeed;
            float pulse = Mathf.Sin(pulseTimer) * 0.15f + 0.85f;
            
            // Kleur laten pulseren
            Color pulsedColor = powerUpSelectedColor;
            pulsedColor.a = pulse;
            powerUpButtonImage.color = pulsedColor;
            
            // Schaal laten pulseren
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

        // 🔥 NIEUW: Sla de Image en Text references op
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
            Debug.Log("⏸️ Game is gepauzeerd (weer)");
            UpdateActionButtons();
            return;
        }

        // 🔥 NIEUW: Reset powerup selectie aan het begin van elke beurt
        isPowerUpSelected = false;
        ResetPowerUpButtonVisuals();

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

        // 🔥 AANGEPAST: PowerUp button met visuele feedback
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
                    
                    // 🔥 NIEUW: Update powerup button visuals
                    if (hasPowerUp && canAct)
                    {
                        // Er is een powerup geselecteerd en we kunnen hem gebruiken
                        isPowerUpSelected = true;
                        
                        // Update de tekst
                        if (powerUpButtonText != null)
                        {
                            powerUpButtonText.text = $"⚡ {selectedPowerUp.itemName}";
                        }
                    }
                    else
                    {
                        // Geen powerup geselecteerd of niet bruikbaar
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

    // 🔥 NIEUW: Reset de powerup button visuals
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
            Debug.Log("⏸️ Game gepauzeerd (weer)");
        }
        else
        {
            Debug.Log("▶️ Game hervat na weer");
        }
    }

    public bool IsGamePaused()
    {
        return isGamePaused;
    }

    // 🔥 AANGEPAST: PowerUp clicked met feedback
    public void OnPowerUpClicked()
    {
        if (isGamePaused)
        {
            Debug.Log("⏸️ Even wachten, game is gepauzeerd!");
            return;
        }

        if (hasPlacedItemThisTurn)
        {
            Debug.Log("⚠️ Je hebt deze beurt al een actie gedaan!");
            return;
        }

        Debug.Log($"⚡ {currentPlayer.gameObject.name} clicked POWER UP!");

        // 🔥 NIEUW: Flash feedback op de knop
        if (powerUpButtonImage != null)
        {
            StartCoroutine(FlashPowerUpButton());
        }

        PowerUpActions powerActions = currentPlayer.GetComponent<PowerUpActions>();
        if (powerActions == null)
        {
            Debug.Log("⚠️ Geen PowerUpActions gevonden!");
            return;
        }

        // Check of er een powerup is geselecteerd
        ItemData selectedPowerUp = powerActions.GetSelectedPowerUp();
        if (selectedPowerUp == null)
        {
            Debug.Log("⚠️ Selecteer eerst een power-up uit je inventory!");
            // Toon een visuele waarschuwing
            StartCoroutine(ShowPowerUpWarning());
            return;
        }

        // Voer de powerup uit
        powerActions.ExecuteSelectedPowerUp();
        
        // 🔥 NIEUW: Reset de powerup selectie status na gebruik
        isPowerUpSelected = false;
        ResetPowerUpButtonVisuals();
        
        // Update de UI
        UpdateActionButtons();
        
        // Refresh inventory UI
        if (inventoryUI != null)
        {
            inventoryUI.ClearSelectedItem();
            inventoryUI.RefreshUI();
        }
    }

    // 🔥 NIEUW: Flash animatie voor powerup knop
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

    // 🔥 NIEUW: Waarschuwing als er geen powerup is geselecteerd
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
            Debug.Log("⏸️ Even wachten, game is gepauzeerd!");
            return;
        }

        Debug.Log($"⏭️ {currentPlayer.gameObject.name} clicked PASS");

        // 🔥 NIEUW: Reset powerup selectie bij passen
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
        
        // 🔥 NIEUW: Reset powerup selectie bij einde beurt
        isPowerUpSelected = false;
        ResetPowerUpButtonVisuals();
        
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
        // 🔥 NIEUW: Reset powerup selectie na weer
        isPowerUpSelected = false;
        ResetPowerUpButtonVisuals();
        
        StartTurn();
        Debug.Log($"🔄 Beurt hervat voor {currentPlayer.gameObject.name} na weerevent");
    }
}