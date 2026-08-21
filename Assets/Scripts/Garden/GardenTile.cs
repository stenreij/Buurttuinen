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
                Debug.LogWarning($"⚠️ No tile visual found for {gameObject.name}, using self (item will scale too!)");
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
        Debug.Log($"🖱️ Tile clicked: {gameObject.name} ({xPosition}, {yPosition})");

        TurnManager turnManager = FindFirstObjectByType<TurnManager>();
        if (turnManager == null)
        {
            Debug.Log("⚠️ TurnManager not found!");
            return;
        }

        Player currentPlayer = turnManager.GetCurrentPlayer();
        if (currentPlayer == null)
        {
            Debug.Log("⚠️ No current player found!");
            return;
        }

        Garden parentGarden = GetComponentInParent<Garden>();
        if (parentGarden == null)
        {
            Debug.Log("⚠️ Tile has no Garden parent!");
            return;
        }

        ItemActions actions = currentPlayer.GetComponent<ItemActions>();
        if (actions == null)
        {
            Debug.Log("⚠️ No ItemActions found on current player!");
            return;
        }

        if (turnManager.IsGamePaused())
        {
            Debug.Log("⏸️ Game is gepauzeerd (weer), je kunt geen acties uitvoeren!");
            return;
        }

        PowerUpActions powerActions = currentPlayer.GetComponent<PowerUpActions>();
        if (powerActions != null && powerActions.IsWaitingForTile())
        {
            powerActions.ExecutePendingPowerUp(this);
            return;
        }

        ItemData selectedItem = actions.GetSelectedItem();
        if (selectedItem != null && selectedItem.type == ItemType.Sabotage)
        {
            Debug.Log($"🔧 Using sabotage on tile!");
            actions.RemoveItem(this);
            return;
        }

        InventoryUI inventoryUI = FindFirstObjectByType<InventoryUI>();
        if (inventoryUI != null)
        {
            ItemData uiSelectedItem = inventoryUI.GetSelectedItem();
            if (uiSelectedItem != null && uiSelectedItem.type == ItemType.PowerUp)
            {
                Debug.Log($"⚡ PowerUp selected, but waiting for PowerUpActions to handle it");
                return;
            }
        }

        if (currentPlayer.assignedGarden != parentGarden)
        {
            Debug.Log($"⚠️ {currentPlayer.gameObject.name} cannot place in {parentGarden.name}! This is not your garden.");
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
            string itemName = placedItemData.itemName;
            string itemType = placedItemData.type.ToString();
            int score = placedItemData.score;
            int water = placedItemData.water;
            int soil = placedItemData.soilHealth;
            int biodiversity = placedItemData.biodiversity;
            int aesthetic = placedItemData.aesthetic;
            bool isProtected = this.isProtected;

            int bonusScore = GetBonusScore();

            tooltipManager.ShowTooltip(
                itemName,
                itemType,
                score,
                water,
                soil,
                biodiversity,
                aesthetic,
                bonusScore
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
                    if (player.soilBoost > 0)
                    {
                        bonus += player.soilBoost;
                    }
                    break;
                }
            }
        }

        return bonus;
    }

    int GetBonusWater()
    {
        return 0;
    }

    int GetBonusSoil()
    {
        return 0;
    }

    int GetBonusBiodiversity()
    {
        return 0;
    }

    int GetBonusAesthetic()
    {
        return 0;
    }
}