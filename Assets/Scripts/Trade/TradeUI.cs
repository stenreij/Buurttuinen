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
    [SerializeField] private Transform targetItemContainer;
    [SerializeField] private GameObject targetItemButtonPrefab;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private Inventory targetInventory;
    private System.Action<bool, ItemData> onTargetResponse;
    private ItemData selectedTargetItem;

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

        if (acceptButton != null)
            acceptButton.onClick.AddListener(OnAcceptClicked);
        if (declineButton != null)
            declineButton.onClick.AddListener(OnDeclineClicked);
        if (confirmButton != null)
            confirmButton.onClick.AddListener(ConfirmTargetItem);
        if (cancelButton != null)
            cancelButton.onClick.AddListener(CancelTargetSelection);
    }

    public void ShowTradeRequest(Player initiator, Player targetPlayer, ItemData offeredItem, System.Action<bool, ItemData> callback)
    {
        onTargetResponse = callback;
        selectedTargetItem = null;

        if (tradeRequestPanel != null)
        {
            tradeRequestPanel.SetActive(true);
            if (requestText != null)
                requestText.text = $"{initiator.playerName} offers {offeredItem.itemName}. Select an item to trade back.";
        }

        if (targetItemSelectionPanel != null)
            targetItemSelectionPanel.SetActive(false);

        if (targetItemContainer != null)
        {
            foreach (Transform child in targetItemContainer)
                Destroy(child.gameObject);
        }

        if (targetPlayer != null)
        {
            targetInventory = targetPlayer.GetComponent<Inventory>();
            if (targetInventory != null && targetInventory.items.Count > 0)
            {
                foreach (var item in targetInventory.items)
                {
                    if (targetItemButtonPrefab != null && targetItemContainer != null)
                    {
                        GameObject btn = Instantiate(targetItemButtonPrefab, targetItemContainer);
                        btn.GetComponentInChildren<TextMeshProUGUI>().text = item.itemName;
                        ItemData captured = item;
                        btn.GetComponent<Button>().onClick.AddListener(() => SelectTargetItem(captured));
                    }
                }
            }
        }

        if (targetInventory == null || targetInventory.items.Count == 0)
        {
            DeclineTrade();
        }
    }

    private void SelectTargetItem(ItemData item)
    {
        selectedTargetItem = item;
        if (targetItemContainer != null)
        {
            foreach (Transform child in targetItemContainer)
            {
                Image img = child.GetComponent<Image>();
                if (img != null)
                {
                    bool isSelected = child.GetComponentInChildren<TextMeshProUGUI>().text == item.itemName;
                    img.color = isSelected ? Color.yellow : Color.white;
                }
            }
        }
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
        onTargetResponse?.Invoke(true, selectedTargetItem);
    }

    public void CancelTargetSelection()
    {
        if (targetItemSelectionPanel != null)
            targetItemSelectionPanel.SetActive(false);
        DeclineTrade();
    }

    public void HideAllPanels()
    {
        if (tradeRequestPanel != null)
            tradeRequestPanel.SetActive(false);
        if (targetItemSelectionPanel != null)
            targetItemSelectionPanel.SetActive(false);
    }
}