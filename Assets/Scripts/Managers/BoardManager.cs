using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Garden Settings")]
    public GameObject gardenPrefab;
    public int numberOfGardens = 4;
    public List<Garden> gardens = new List<Garden>();

    [Header("Spacing (automatic)")]
    public float spacing = 2f;

    [Header("Fixed Positions (override automatic)")]
    public bool useFixedPositions = false;
    public Vector3[] fixedPositions = new Vector3[4];

    [Header("Start Items")]
    public int minStartItemsPerGarden = 1;
    public int maxStartItemsPerGarden = 3;

    public void CreateBoard()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.name.StartsWith("Garden_"))
            {
                Destroy(child.gameObject);
            }
        }
        gardens.Clear();

        ItemDatabase itemDatabase = FindFirstObjectByType<ItemDatabase>();

        if (useFixedPositions && fixedPositions.Length >= numberOfGardens)
        {
            for (int i = 0; i < numberOfGardens; i++)
            {
                Vector3 position = fixedPositions[i];
                CreateGarden(position, i + 1, itemDatabase);
            }
        }
        else
        {
            float gardenWidth = 7 * 2f;
            float totalWidth = (numberOfGardens - 1) * (gardenWidth + spacing);
            float startX = -totalWidth / 2f;

            for (int i = 0; i < numberOfGardens; i++)
            {
                Vector3 position = new Vector3(startX + i * (gardenWidth + spacing), 0, 0);
                CreateGarden(position, i + 1, itemDatabase);
            }
        }
    }

    void CreateGarden(Vector3 position, int index, ItemDatabase itemDatabase)
    {
        GameObject garden = Instantiate(gardenPrefab, position, Quaternion.identity, transform);
        garden.name = "Garden_" + index;

        Garden gardenScript = garden.GetComponent<Garden>();
        if (gardenScript != null)
        {
            gardenScript.CreateGarden();
            gardens.Add(gardenScript);

            GardenTile[] allTiles = garden.GetComponentsInChildren<GardenTile>();
            foreach (GardenTile tile in allTiles)
            {
                SpriteRenderer sr = tile.GetComponentInChildren<SpriteRenderer>();
                if (sr != null && sr.gameObject != tile.gameObject)
                {
                    tile.tileVisual = sr.gameObject;
                }
                else
                {
                    tile.tileVisual = tile.gameObject;
                }
            }

            if (itemDatabase != null)
            {
                PlaceStartItems(gardenScript, itemDatabase);
            }
            else
            {
                Debug.LogWarning($"⚠️ No ItemDatabase found, skipping start items for {garden.name}");
            }
        }
        else
        {
            Debug.LogError($"❌ No Garden script on {garden.name}!");
        }
    }

    void PlaceStartItems(Garden garden, ItemDatabase itemDatabase)
    {
        if (itemDatabase == null) return;

        int itemCount = Random.Range(minStartItemsPerGarden, maxStartItemsPerGarden + 1);

        GardenTile[] tiles = garden.GetComponentsInChildren<GardenTile>();

        for (int i = 0; i < itemCount; i++)
        {
            List<GardenTile> emptyTiles = new List<GardenTile>();
            foreach (GardenTile tile in tiles)
            {
                if (!tile.occupied)
                {
                    emptyTiles.Add(tile);
                }
            }

            if (emptyTiles.Count == 0)
            {
                Debug.LogWarning($"⚠️ No empty tiles left in {garden.name}");
                break;
            }

            int randomTileIndex = Random.Range(0, emptyTiles.Count);
            GardenTile targetTile = emptyTiles[randomTileIndex];

            List<ItemData> availableItems = new List<ItemData>();
            foreach (ItemData item in itemDatabase.GetAvailableItems())
            {
                if (item.type != ItemType.Sabotage && item.type != ItemType.PowerUp)
                {
                    availableItems.Add(item);
                }
            }

            if (availableItems.Count == 0)
            {
                Debug.LogWarning("⚠️ No more normal items available in the pool!");
                break;
            }

            int randomItemIndex = Random.Range(0, availableItems.Count);
            ItemData randomItem = availableItems[randomItemIndex];

            if (itemDatabase.TryTakeItem(randomItem))
            {
                if (randomItem.prefab != null)
                {
                    GameObject placed = Instantiate(randomItem.prefab, targetTile.transform.position, Quaternion.identity);
                    placed.transform.parent = targetTile.transform;
                    placed.transform.localPosition = Vector3.zero;

                    SpriteRenderer sr = placed.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.enabled = true;
                        sr.sortingOrder = 10;
                    }

                    targetTile.placedItem = placed;
                    targetTile.placedItemData = randomItem;
                }

                targetTile.occupied = true;
            }
            else
            {
                Debug.LogWarning($"⚠️ Could not take {randomItem.itemName} from pool!");
                i--;
            }
        }
    }
}