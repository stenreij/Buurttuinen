using UnityEngine;

public class Garden : MonoBehaviour
{
    public int width = 7;
    public int height = 3;
    public GameObject tilePrefab;
    public float tileSize = 2f;


    public void CreateGarden()
    {
        Debug.Log($"🌱 CreateGarden() called for {gameObject.name}");

        Vector3 startPos = transform.position;
        Debug.Log($"📍 Start position: {startPos}");

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = startPos + new Vector3(x * tileSize, y * tileSize, 0);
                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                tile.name = "Tile_" + x + "_" + y;
            }
        }

        Debug.Log($"✅ Garden {gameObject.name} created with {width * height} tiles");
    }
}