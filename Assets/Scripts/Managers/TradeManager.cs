using UnityEngine;
using System.Collections.Generic;

public class TradeManager : MonoBehaviour
{
    public static TradeManager Instance;

    public enum TradeState { Idle, SelectingItem, SelectingTarget, AwaitingResponse, Completed, Cancelled }

    private TradeState currentState = TradeState.Idle;
    private Player initiator;
    private Player target;
    private ItemData offeredItem;
    private ItemData requestedItem;

    public System.Action<TradeState> OnStateChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool IsTradeActive()
    {
        return currentState != TradeState.Idle && currentState != TradeState.Completed && currentState != TradeState.Cancelled;
    }

    public void StartTrade(Player initiatorPlayer)
    {
        if (IsTradeActive())
        {
            Debug.Log("Trade already in progress.");
            return;
        }

        this.initiator = initiatorPlayer;
        currentState = TradeState.SelectingItem;
        OnStateChanged?.Invoke(currentState);
        Debug.Log($"{initiator.playerName} started a trade.");
    }

    public void SelectItem(ItemData item)
    {
        if (currentState != TradeState.SelectingItem) return;
        if (item == null) return;

        offeredItem = item;
        currentState = TradeState.SelectingTarget;
        OnStateChanged?.Invoke(currentState);
        Debug.Log($"{initiator.playerName} offered {item.itemName}.");

        // Get all players
        TurnManager turnManager = FindFirstObjectByType<TurnManager>();
        if (turnManager == null)
        {
            Debug.LogWarning("TurnManager not found!");
            CancelTrade("TurnManager missing.");
            return;
        }

        List<Player> allPlayers = turnManager.players;

        // If only 2 players, auto-select the other player
        if (allPlayers.Count == 2)
        {
            Player targetPlayer = allPlayers.Find(p => p != initiator);
            if (targetPlayer != null)
            {
                SelectTarget(targetPlayer);
                return;
            }
        }

        // With 3 or 4 players, show player selection UI
        TradeUI tradeUI = FindFirstObjectByType<TradeUI>();
        if (tradeUI != null)
        {
            tradeUI.ShowPlayerSelection(initiator, allPlayers, (selectedPlayer) =>
            {
                SelectTarget(selectedPlayer);
            });
        }
        else
        {
            Debug.LogWarning("TradeUI not found.");
            CancelTrade("TradeUI missing.");
        }
    }

    public void SelectTarget(Player targetPlayer)
    {
        if (currentState != TradeState.SelectingTarget) return;
        if (targetPlayer == initiator) return;
        if (targetPlayer == null) return;

        target = targetPlayer;
        currentState = TradeState.AwaitingResponse;
        OnStateChanged?.Invoke(currentState);
        Debug.Log($"{initiator.playerName} offered trade to {targetPlayer.playerName}.");

        // Hide player selection panel if still visible
        TradeUI tradeUI = FindFirstObjectByType<TradeUI>();
        if (tradeUI != null)
        {
            tradeUI.HideAllPanels();
            tradeUI.ShowTradeRequest(initiator, targetPlayer, offeredItem, OnTargetResponse);
        }
        else
        {
            Debug.LogWarning("TradeUI not found.");
            CancelTrade("TradeUI missing.");
        }
    }

    private void OnTargetResponse(bool accepted, ItemData targetItem)
    {
        if (!accepted)
        {
            CancelTrade("Target declined.");
            return;
        }

        if (targetItem == null)
        {
            CancelTrade("Target did not select an item.");
            return;
        }

        Inventory invInitiator = initiator.GetComponent<Inventory>();
        Inventory invTarget = target.GetComponent<Inventory>();

        if (!invInitiator.HasItem(offeredItem) || !invTarget.HasItem(targetItem))
        {
            CancelTrade("One of the items is no longer available.");
            return;
        }

        requestedItem = targetItem;
        CompleteTrade();
    }

    private void CompleteTrade()
    {
        if (offeredItem == null || requestedItem == null)
        {
            CancelTrade("Invalid items.");
            return;
        }

        Inventory invInitiator = initiator.GetComponent<Inventory>();
        Inventory invTarget = target.GetComponent<Inventory>();

        if (!invInitiator.HasItem(offeredItem) || !invTarget.HasItem(requestedItem))
        {
            CancelTrade("One of the items is no longer available.");
            return;
        }

        // Remove items
        invInitiator.RemoveItem(offeredItem);
        invTarget.RemoveItem(requestedItem);

        // Add swapped items
        invInitiator.AddItem(requestedItem);
        invTarget.AddItem(offeredItem);

        currentState = TradeState.Completed;
        OnStateChanged?.Invoke(currentState);

        Debug.Log($"Trade completed: {initiator.playerName} received {requestedItem.itemName}, {target.playerName} received {offeredItem.itemName}.");

        // Refresh UI
        InventoryUI ui = FindFirstObjectByType<InventoryUI>();
        if (ui != null) ui.RefreshUI();

        // Notify TurnManager that trade is complete (success)
        TurnManager turnManager = FindFirstObjectByType<TurnManager>();
        if (turnManager != null)
        {
            turnManager.OnTradeCompleted(true); // true = trade success
        }

        ResetTrade();
    }

    public void CancelTrade(string reason = "")
    {
        if (currentState == TradeState.Idle) return;

        currentState = TradeState.Cancelled;
        OnStateChanged?.Invoke(currentState);

        if (!string.IsNullOrEmpty(reason))
            Debug.Log($"Trade cancelled: {reason}");
        else
            Debug.Log("Trade cancelled.");

        // Notify TurnManager that trade is cancelled
        TurnManager turnManager = FindFirstObjectByType<TurnManager>();
        if (turnManager != null)
        {
            turnManager.OnTradeCompleted(false);
        }

        // Hide all UI panels
        TradeUI tradeUI = FindFirstObjectByType<TradeUI>();
        if (tradeUI != null)
        {
            tradeUI.HideAllPanels();
        }

        ResetTrade();
    }

    private void ResetTrade()
    {
        initiator = null;
        target = null;
        offeredItem = null;
        requestedItem = null;
        currentState = TradeState.Idle;
        OnStateChanged?.Invoke(currentState);
    }

    public Player GetInitiator() => initiator;
    public Player GetTarget() => target;
    public ItemData GetOfferedItem() => offeredItem;
    public TradeState GetState() => currentState;
}