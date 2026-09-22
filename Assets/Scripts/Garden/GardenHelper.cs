using System.Collections.Generic;
using UnityEngine;

public static class GardenHelper
{
    public static GardenTile[] GetTiles(Player player)
    {
        if (player == null || player.assignedGarden == null)
            return new GardenTile[0];

        return player.assignedGarden.GetComponentsInChildren<GardenTile>();
    }

    public static List<GardenTile> GetOccupiedTiles(Player player)
    {
        List<GardenTile> result = new List<GardenTile>();
        GardenTile[] tiles = GetTiles(player);

        foreach (GardenTile tile in tiles)
        {
            if (tile.occupied && tile.placedItem != null)
            {
                result.Add(tile);
            }
        }

        return result;
    }

    public static List<GardenTile> GetEmptyTiles(Player player)
    {
        List<GardenTile> result = new List<GardenTile>();
        GardenTile[] tiles = GetTiles(player);

        foreach (GardenTile tile in tiles)
        {
            if (!tile.occupied)
            {
                result.Add(tile);
            }
        }

        return result;
    }

    public static GardenTile GetRandomEmptyTile(Player player)
    {
        List<GardenTile> emptyTiles = GetEmptyTiles(player);
        if (emptyTiles.Count == 0) return null;
        return emptyTiles[Random.Range(0, emptyTiles.Count)];
    }

    public static List<GardenTile> GetUnprotectedTiles(Player player)
    {
        List<GardenTile> result = new List<GardenTile>();
        GardenTile[] tiles = GetTiles(player);

        foreach (GardenTile tile in tiles)
        {
            if (tile.occupied && tile.placedItem != null && !tile.isProtected)
            {
                result.Add(tile);
            }
        }

        return result;
    }

    public static List<GardenTile> GetTilesByType(Player player, ItemType type)
    {
        List<GardenTile> result = new List<GardenTile>();
        GardenTile[] tiles = GetTiles(player);

        foreach (GardenTile tile in tiles)
        {
            if (tile.occupied && tile.placedItemData != null && tile.placedItemData.type == type)
            {
                result.Add(tile);
            }
        }

        return result;
    }

    public static bool IsTileInGarden(GardenTile tile, Player player)
    {
        if (player == null || player.assignedGarden == null)
            return false;

        return tile.transform.IsChildOf(player.assignedGarden.transform);
    }

    public static Player GetPlayerForTile(GardenTile tile, List<Player> players)
    {
        foreach (Player player in players)
        {
            if (IsTileInGarden(tile, player))
            {
                return player;
            }
        }
        return null;
    }
}