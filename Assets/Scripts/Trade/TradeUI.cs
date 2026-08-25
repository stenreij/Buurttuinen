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
    [SerializeField] private Transform content; // ← DIRECT NAAR CONTENT
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
        {
            confirmButton.onClick.AddListener(ConfirmTargetItem);
            confirmButton.interactable = false;
        }
        if (cancelButton != null)
            cancelButton.onClick.AddListener(CancelTargetSelection);
    }

    public void ShowTradeRequest(Player initiator, Player targetPlayer, ItemData offeredItem, System.Action<bool, ItemData> callback)
    {
        onTargetResponse = callback;
        selectedTargetItem = null;

        if (confirmButton != null)
            confirmButton.interactable = false;

        if (tradeRequestPanel != null)
        {
            tradeRequestPanel.SetActive(true);
            if (requestText != null)
                requestText.text = $"{initiator.playerName} offers {offeredItem.itemName}. Select an item to trade back.";
        }

        if (targetItemSelectionPanel != null)
            targetItemSelectionPanel.SetActive(false);

        // Clear de content (niet de targetItemContainer)
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
                        // Instantieer in content (de container van de ScrollView)
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

                // Forceer layout update
                if (content != null)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(content as RectTransform);
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

        if (confirmButton != null)
            confirmButton.interactable = false;
    }
}