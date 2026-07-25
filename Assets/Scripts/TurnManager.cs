using UnityEngine;
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

        Debug.Log($"🎮 {currentPlayer.gameObject.name} is now taking their turn.");
    }

    public void EndTurn()
    {
        // Reset placed item flag
        hasPlacedItemThisTurn = false;

        // Move to next player
        currentPlayerIndex++;
        if (currentPlayerIndex >= players.Count)
        {
            currentPlayerIndex = 0;
        }

        StartTurn();
    }

    public void PassTurn()
    {
        // Give current player a random item as a reward for passing
        ItemActions actions = currentPlayer.GetComponent<ItemActions>();
        if (actions != null)
        {
            actions.GiveRandomItem();
            Debug.Log($"⏭️ {currentPlayer.gameObject.name} passed and received a random item!");
        }

        // Update UI
        if (inventoryUI != null)
        {
            inventoryUI.RefreshUI();
        }

        // End the turn
        EndTurn();
    }

    public Player GetCurrentPlayer()
    {
        return currentPlayer;
    }

    public void ResetPlacedItemFlag()
    {
        hasPlacedItemThisTurn = false;
    }
}