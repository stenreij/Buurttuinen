using UnityEngine;

public class GardenTile : MonoBehaviour
{
    public bool occupied = false;
    public bool isProtected = false;
    public int xPosition;
    public int yPosition;
    public GameObject placedItem;
    public ItemData placedItemData;

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
            actions.RemoveItem(this);
            return;
        }

        if (selectedItem != null && selectedItem.type == ItemType.PowerUp)
        {
            actions.PlaceItem(this);
            return;
        }

        if (currentPlayer.assignedGarden != parentGarden)
        {
            Debug.Log($"⚠️ {currentPlayer.gameObject.name} cannot place in {parentGarden.name}! This is not your garden.");
            return;
        }

        actions.PlaceItem(this);
    }
}