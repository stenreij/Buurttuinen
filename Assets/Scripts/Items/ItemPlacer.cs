using UnityEngine;

public static class ItemPlacer
{
    public static bool PlaceItemOnTile(GardenTile targetTile, ItemData item, bool consumeItemFromDatabase = true)
    {
        if (targetTile == null || item == null || targetTile.occupied)
            return false;
        
        if (item.prefab == null)
            return false;
        
        ItemDatabase database = UnityEngine.Object.FindFirstObjectByType<ItemDatabase>();
        if (consumeItemFromDatabase && database != null)
        {
            if (!database.TryTakeItem(item))
                return false;
        }
        
        GameObject placed = UnityEngine.Object.Instantiate(item.prefab, targetTile.transform.position, Quaternion.identity);
        placed.transform.parent = targetTile.transform;
        placed.transform.localPosition = Vector3.zero;
        placed.transform.localScale = Vector3.one;
        
        SpriteRenderer sr = placed.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = true;
            sr.sortingOrder = 10;
        }
        
        targetTile.placedItem = placed;
        targetTile.placedItemData = item;
        targetTile.occupied = true;
        
        return true;
    }
    
    public static void RemoveItemFromTile(GardenTile tile)
    {
        if (tile == null || !tile.occupied) return;
        
        if (tile.placedItem != null)
        {
            UnityEngine.Object.Destroy(tile.placedItem);
        }
        
        tile.occupied = false;
        tile.placedItem = null;
        tile.placedItemData = null;
    }
    
    public static bool MoveItem(GardenTile sourceTile, GardenTile targetTile)
    {
        if (sourceTile == null || targetTile == null)
            return false;
        if (!sourceTile.occupied || sourceTile.placedItem == null)
            return false;
        if (targetTile.occupied)
            return false;
        
        targetTile.occupied = true;
        targetTile.placedItem = sourceTile.placedItem;
        targetTile.placedItemData = sourceTile.placedItemData;
        targetTile.isProtected = sourceTile.isProtected;
        
        if (sourceTile.placedItem != null)
        {
            sourceTile.placedItem.transform.position = targetTile.transform.position;
            sourceTile.placedItem.transform.parent = targetTile.transform;
        }
        
        sourceTile.occupied = false;
        sourceTile.placedItem = null;
        sourceTile.placedItemData = null;
        sourceTile.isProtected = false;
        
        return true;
    }
}