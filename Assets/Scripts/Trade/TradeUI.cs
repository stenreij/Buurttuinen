using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TradeUI : MonoBehaviour
{
    public static TradeUI Instance;

    [Header("Trade Request Panel")]
    [SerializeField] private GameObject tradeRequestPanel;
    [SerializeField] private TextMeshProUGUI requestText;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button declineButton;

    [Header("Target Item Selection")]
    [SerializeField] private GameObject targetItemSelectionPanel;
    [SerializeField] private Transform content;
    [SerializeField] private GameObject targetItemButtonPrefab;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    [Header("Player Selection")]
    [SerializeField] private GameObject playerSelectionPanel;
    [SerializeField] private Transform playerContainer;
    [SerializeField] private GameObject playerButtonPrefab;

    private Inventory targetInventory;
    private System.Action<bool, ItemData> onTargetResponse;
    private ItemData selectedTargetItem;
    private Player selectedTargetPlayer;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (tradeRequestPanel != null)
            tradeRequestPanel.SetActive(false);
        if (targetItemSelectionPanel != null)
            targetItemSelectionPanel.SetActive(false);
        if (playerSelectionPanel != null)
            playerSelectionPanel.SetActive(false);

        if (acceptButton != null)
            acceptButton.onClick.AddListener(OnAcceptClicked);
        if (declineButton != null)
            declineButton.onClick.AddListener(OnDeclineClicked);
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(ConfirmTargetItem);
            confirmButton.interactable = false;
        }
        if (cancelButton != null)
            cancelButton.onClick.AddListener(CancelTargetSelection);
    }

    public void ShowPlayerSelection(Player initiator, List<Player> allPlayers, System.Action<Player> onPlayerSelected)
    {
        if (playerSelectionPanel == null || playerContainer == null || playerButtonPrefab == null)
        {
            Debug.LogWarning("Player selection UI not fully configured in TradeUI Inspector!");
            return;
        }

        playerSelectionPanel.SetActive(true);

        foreach (Transform child in playerContainer)
            Destroy(child.gameObject);

        Debug.Log($"Showing player selection. Total players: {allPlayers.Count}. Initiator: {initiator.playerName}");

        int playerCount = 0;
        foreach (Player player in allPlayers)
        {
            if (player == initiator) continue;

            playerCount++;
            Debug.Log($"Creating button for player: {player.playerName}");

            GameObject btn = Instantiate(playerButtonPrefab, playerContainer);
            
            TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
                btnText.text = player.playerName;

            Player captured = player;
            Button btnComponent = btn.GetComponent<Button>();
            if (btnComponent != null)
            {
                btnComponent.onClick.AddListener(() => {
                    Debug.Log($"Player selected: {captured.playerName}");
                    selectedTargetPlayer = captured;
                    if (playerSelectionPanel != null)
                        playerSelectionPanel.SetActive(false);
                    onPlayerSelected?.Invoke(captured);
                });
            }
        }

        Debug.Log($"Created {playerCount} player buttons.");
    }

    public void ShowTradeRequest(Player initiator, Player targetPlayer, ItemData offeredItem, System.Action<bool, ItemData> callback)
    {
        onTargetResponse = callback;
        selectedTargetItem = null;

        if (confirmButton != null)
            confirmButton.interactable = false;

        selectedTargetPlayer = targetPlayer;

        Debug.Log($"Showing trade request to: {targetPlayer.playerName} from {initiator.playerName}");

        if (tradeRequestPanel != null)
        {
            tradeRequestPanel.SetActive(true);
            if (requestText != null)
                requestText.text = $"{initiator.playerName} offers {offeredItem.itemName}. Select an item to trade back.";
        }

        if (targetItemSelectionPanel != null)
            targetItemSelectionPanel.SetActive(false);

        if (content != null)
        {
            foreach (Transform child in content)
                Destroy(child.gameObject);
        }

        if (targetPlayer != null)
        {
            targetInventory = targetPlayer.GetComponent<Inventory>();
            if (targetInventory != null && targetInventory.items.Count > 0)
            {
                HashSet<ItemData> uniqueItems = new HashSet<ItemData>();

                foreach (var item in targetInventory.items)
                {
                    if (uniqueItems.Contains(item))
                        continue;
                    uniqueItems.Add(item);

                    if (targetItemButtonPrefab != null && content != null)
                    {
                        GameObject btn = Instantiate(targetItemButtonPrefab, content);
                        btn.transform.SetParent(content, false);

                        TextMeshProUGUI nameText = btn.GetComponentInChildren<TextMeshProUGUI>();
                        if (nameText != null)
                            nameText.text = item.itemName;

                        Image iconImage = btn.GetComponent<Image>();
                        if (iconImage != null && item.icon != null)
                        {
                            iconImage.sprite = item.icon;
                            iconImage.preserveAspect = true;
                        }

                        ItemData captured = item;
                        btn.GetComponent<Button>().onClick.AddListener(() => SelectTargetItem(captured));
                    }
                }

                if (content != null)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(content as RectTransform);
            }
        }

        if (targetInventory == null || targetInventory.items.Count == 0)
        {
            Debug.Log($"Target {targetPlayer.playerName} has no items to trade.");
            DeclineTrade();
        }
    }

    private void SelectTargetItem(ItemData item)
    {
        selectedTargetItem = item;

        if (content != null)
        {
            foreach (Transform child in content)
            {
                Image img = child.GetComponent<Image>();
                if (img != null)
                {
                    TextMeshProUGUI text = child.GetComponentInChildren<TextMeshProUGUI>();
                    if (text != null && text.text == item.itemName)
                    {
                        img.color = Color.yellow;
                    }
                    else
                    {
                        img.color = Color.white;
                    }
                }
            }
        }

        if (confirmButton != null)
            confirmButton.interactable = true;
    }

    public void OnAcceptClicked()
    {
        if (targetInventory == null || targetInventory.items.Count == 0)
        {
            DeclineTrade();
            return;
        }

        if (tradeRequestPanel != null)
            tradeRequestPanel.SetActive(false);
        if (targetItemSelectionPanel != null)
            targetItemSelectionPanel.SetActive(true);
    }

    public void OnDeclineClicked()
    {
        DeclineTrade();
    }

    private void DeclineTrade()
    {
        if (tradeRequestPanel != null)
            tradeRequestPanel.SetActive(false);
        if (targetItemSelectionPanel != null)
            targetItemSelectionPanel.SetActive(false);

        if (confirmButton != null)
            confirmButton.interactable = false;

        onTargetResponse?.Invoke(false, null);
    }

    public void ConfirmTargetItem()
    {
        if (selectedTargetItem == null)
        {
            Debug.Log("Please select an item to trade.");
            return;
        }

        if (targetItemSelectionPanel != null)
            targetItemSelectionPanel.SetActive(false);

        if (confirmButton != null)
            confirmButton.interactable = false;

        onTargetResponse?.Invoke(true, selectedTargetItem);
    }

    public void CancelTargetSelection()
    {
        if (targetItemSelectionPanel != null)
            targetItemSelectionPanel.SetActive(false);

        if (confirmButton != null)
            confirmButton.interactable = false;

        DeclineTrade();
    }

    public void HideAllPanels()
    {
        if (tradeRequestPanel != null)
            tradeRequestPanel.SetActive(false);
        if (targetItemSelectionPanel != null)
            targetItemSelectionPanel.SetActive(false);
        if (playerSelectionPanel != null)
            playerSelectionPanel.SetActive(false);

        if (confirmButton != null)
            confirmButton.interactable = false;
    }

    public Player GetSelectedTargetPlayer()
    {
        return selectedTargetPlayer;
    }
}