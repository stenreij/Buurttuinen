using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class TurnManager : MonoBehaviour
{
    [Header("References")]
    public List<Player> players;
    public TextMeshProUGUI turnIndicator;
    public InventoryUI inventoryUI;

    private int currentPlayerIndex = 0;
    private Player currentPlayer;
    public bool hasPlacedItemThisTurn = false;

    [Header("UI Buttons")]
    public Button passButton;
    public Button tradeButton;
    public Button endTurnButton;

    public void Initialize(List<Player> playerList)
    {
        players = playerList;
        if (players.Count == 0)
        {
            Debug.LogError("❌ No players found in TurnManager!");
            return;
        }
        StartTurn();
    }

    public void StartTurn()
    {
        if (players == null || players.Count == 0)
        {
            Debug.LogError("❌ No players in TurnManager!");
            return;
        }

        // Reset the placed item flag for the new turn
        hasPlacedItemThisTurn = false;

        currentPlayer = players[currentPlayerIndex];

        // Update turn indicator UI
        if (turnIndicator != null)
        {
            turnIndicator.text = "Turn: " + currentPlayer.gameObject.name;
        }

        // Show inventory of the current player
        if (inventoryUI != null)
        {
            inventoryUI.playerInventory = currentPlayer.GetComponent<Inventory>();
            inventoryUI.RefreshUI();
        }

        // Update button states
        UpdateActionButtons();


        // Link buttons to methods (remove old listeners first to avoid duplicates)
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

    // UPDATE BUTTON STATES
    public void UpdateActionButtons()
    {
        bool canAct = !hasPlacedItemThisTurn;

        if (passButton != null)
            passButton.interactable = canAct;

        if (tradeButton != null)
            tradeButton.interactable = canAct;

        // End turn button is always active
        if (endTurnButton != null)
            endTurnButton.interactable = true;

        Debug.Log($"🔘 Buttons updated: Pass={canAct}, Trade={canAct}, EndTurn=true");
    }

    // PASS BUTTON
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

    // TRADE BUTTON
    public void OnTradeClicked()
    {
        Debug.Log($"🔄 {currentPlayer.gameObject.name} clicked TRADE (not implemented yet)");
        // Later: open trade UI
    }

    // END TURN BUTTON
    public void OnEndTurnClicked()
    {
        Debug.Log($"⏹️ {currentPlayer.gameObject.name} clicked END TURN");
        EndTurn();
    }

    public void EndTurn()
    {
        // Reset placed item flag
        hasPlacedItemThisTurn = false;

        // Give current player a random item at the end of their turn
        ItemActions actions = currentPlayer.GetComponent<ItemActions>();
        if (actions != null)
        {
            actions.GiveRandomItem();
            Debug.Log($"🎁 {currentPlayer.gameObject.name} received a random item at end of turn!");
        }

        // Move to next player
        currentPlayerIndex++;
        if (currentPlayerIndex >= players.Count)
        {
            currentPlayerIndex = 0;
        }

        // Update UI
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
}