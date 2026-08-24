using UnityEngine;
using UnityEngine.EventSystems;

public class GardenTile : MonoBehaviour
{
    public bool occupied = false;
    public bool isProtected = false;
    public int xPosition;
    public int yPosition;
    public GameObject placedItem;
    public ItemData placedItemData;

    [Header("Tooltip")]
    public TooltipManager tooltipManager;

    [Header("Hover Effects")]
    public GameObject tileVisual;
    public float hoverScale = 1.05f;
    public float animationSpeed = 8f;

    private Vector3 originalScale;
    private bool isHovering = false;
    private float currentScale = 1f;

    void Start()
    {
        tooltipManager = FindFirstObjectByType<TooltipManager>();

        if (tileVisual == null)
        {
            SpriteRenderer[] children = GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sr in children)
            {
                if (sr.gameObject != gameObject && sr.gameObject != placedItem)
                {
                    tileVisual = sr.gameObject;
                    break;
                }
            }

            if (tileVisual == null)
            {
                tileVisual = gameObject;
            }
        }

        originalScale = tileVisual.transform.localScale;
    }

    void Update()
    {
        if (isHovering)
        {
            currentScale = Mathf.Lerp(currentScale, hoverScale, Time.deltaTime * animationSpeed);
        }
        else
        {
            currentScale = Mathf.Lerp(currentScale, 1f, Time.deltaTime * animationSpeed);
        }
        tileVisual.transform.localScale = originalScale * currentScale;
    }

    private void OnMouseDown()
    {
        TurnManager turnManager = FindFirstObjectByType<TurnManager>();
        if (turnManager == null) return;

        Player currentPlayer = turnManager.GetCurrentPlayer();
        if (currentPlayer == null) return;

        Garden parentGarden = GetComponentInParent<Garden>();
        if (parentGarden == null) return;

        ItemActions actions = currentPlayer.GetComponent<ItemActions>();
        if (actions == null) return;

        if (turnManager.IsGamePaused()) return;

        PowerUpActions powerActions = currentPlayer.GetComponent<PowerUpActions>();
        if (powerActions != null && powerActions.IsWaitingForTile())
        {
            powerActions.ExecutePendingPowerUp(this);
            return;
        }

        ItemData selectedItem = actions.GetSelectedItem();
        if (selectedItem != null && selectedItem.type == ItemType.Sabotage)
        {
            actions.RemoveItem(this);
            return;
        }

        InventoryUI inventoryUI = FindFirstObjectByType<InventoryUI>();
        if (inventoryUI != null)
        {
            ItemData uiSelectedItem = inventoryUI.GetSelectedItem();
            if (uiSelectedItem != null && uiSelectedItem.type == ItemType.PowerUp)
            {
                return;
            }
        }

        if (currentPlayer.assignedGarden != parentGarden)
        {
            Debug.Log($"{currentPlayer.gameObject.name} cannot place in {parentGarden.name} - not their garden.");
            return;
        }

        actions.PlaceItem(this);
    }

    void OnMouseOver()
    {
        isHovering = true;

        if (occupied && placedItemData != null && tooltipManager != null)
        {
            if (placedItem != null)
            {
                SpriteRenderer sr = placedItem.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingOrder = 20;
                }
            }

            tooltipManager.ShowTooltip(
                placedItemData.itemName,
                placedItemData.type.ToString(),
                placedItemData.score,
                placedItemData.water,
                placedItemData.soilHealth,
                placedItemData.biodiversity,
                placedItemData.aesthetic,
                GetBonusScore()
            );
        }
    }

    void OnMouseExit()
    {
        isHovering = false;

        if (placedItem != null)
        {
            SpriteRenderer sr = placedItem.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = 10;
            }
        }

        if (tooltipManager != null)
        {
            tooltipManager.HideTooltip();
        }
    }

    int GetBonusScore()
    {
        int bonus = 0;

        Garden garden = GetComponentInParent<Garden>();
        if (garden != null)
        {
            Player[] allPlayers = FindObjectsByType<Player>(FindObjectsSortMode.None);
            foreach (Player player in allPlayers)
            {
                if (player.assignedGarden == garden)
                {
                    bonus += player.soilBoost;
                    bonus += player.communityPenalty;
                    bonus += player.communityBoost;

                    if (placedItemData != null && IsPlantType(placedItemData.type))
                    {
                        bonus += player.weatherBoost;
                    }

                    break;
                }
            }
        }

        return bonus;
    }

    bool IsPlantType(ItemType type)
    {
        return type == ItemType.Plant;
    }

    int GetBonusWater() => 0;
    int GetBonusSoil() => 0;
    int GetBonusBiodiversity() => 0;
    int GetBonusAesthetic() => 0;
}