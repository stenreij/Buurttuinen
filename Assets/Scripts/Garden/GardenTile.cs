using UnityEngine;

public class GardenTile : MonoBehaviour
{
    public bool occupied = false;
    public int xPosition;
    public int yPosition;
    public GameObject placedItem;

    private void OnMouseDown()
    {
        Debug.Log($"🖱️ Tile clicked: {gameObject.name} ({xPosition}, {yPosition})");

        // Find the TurnManager
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

        // CHECK: Is this tile in the current player's own garden?
        Garden parentGarden = GetComponentInParent<Garden>();
        if (parentGarden == null)
        {
            Debug.Log("⚠️ Tile has no Garden parent!");
            return;
        }

        if (currentPlayer.assignedGarden != parentGarden)
        {
            Debug.Log($"⚠️ {currentPlayer.gameObject.name} cannot place in {parentGarden.name}! This is not your garden.");
            return;
        }

        // Get the ItemActions from the current player
        ItemActions actions = currentPlayer.GetComponent<ItemActions>();
        if (actions == null)
        {
            Debug.Log("⚠️ No ItemActions found on current player!");
            return;
        }

        // Place the item on this tile
        actions.PlaceItem(this);
    }
}