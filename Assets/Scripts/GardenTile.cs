using UnityEngine;

public class GardenTile : MonoBehaviour
{
    public bool occupied = false;
    public int xPosition;
    public int yPosition;

    private void OnMouseDown()
    {
        Debug.Log("Tile clicked: " + gameObject.name);
        //Debug.Log("Tile clicked: " + gameObject.name + " at position (" + xPosition + ", " + yPosition + ")");
    }
}
