using System.Collections.Generic;

public static class ScoreCalculator
{
    public static int CalculateItemScore(ItemData item, Player player)
    {
        if (item == null) return 0;

        int baseScore = item.score;
        int bonus = 0;

        if (player != null)
        {
            bonus += player.soilBoost;
            bonus += player.communityPenalty;
            bonus += player.communityBoost;

            if (IsPlantType(item.type))
            {
                bonus += player.weatherBoost;
            }
        }

        return baseScore + bonus;
    }

    public static int CalculatePlayerScore(Player player)
    {
        if (player == null || player.assignedGarden == null) return 0;

        int total = 0;
        GardenTile[] tiles = player.assignedGarden.GetComponentsInChildren<GardenTile>();

        foreach (GardenTile tile in tiles)
        {
            if (tile.occupied && tile.placedItemData != null)
            {
                total += CalculateItemScore(tile.placedItemData, player);
            }
        }

        return total;
    }

    public static int CalculateTotalScore(List<Player> players)
    {
        int total = 0;
        foreach (Player player in players)
        {
            total += CalculatePlayerScore(player);
        }
        return total;
    }

    public static int GetBiodiversityScore(Player player)
    {
        if (player == null || player.assignedGarden == null) return 0;

        int total = 0;
        GardenTile[] tiles = player.assignedGarden.GetComponentsInChildren<GardenTile>();

        foreach (GardenTile tile in tiles)
        {
            if (tile.occupied && tile.placedItemData != null)
            {
                total += tile.placedItemData.biodiversity;
            }
        }

        return total + player.soilBoost;
    }

    public static int GetWaterScore(Player player)
    {
        if (player == null || player.assignedGarden == null) return 0;

        int total = 0;
        GardenTile[] tiles = player.assignedGarden.GetComponentsInChildren<GardenTile>();

        foreach (GardenTile tile in tiles)
        {
            if (tile.occupied && tile.placedItemData != null)
            {
                total += tile.placedItemData.water;
            }
        }

        return total;
    }

    public static int GetSoilScore(Player player)
    {
        if (player == null || player.assignedGarden == null) return 0;

        int total = 0;
        GardenTile[] tiles = player.assignedGarden.GetComponentsInChildren<GardenTile>();

        foreach (GardenTile tile in tiles)
        {
            if (tile.occupied && tile.placedItemData != null)
            {
                total += tile.placedItemData.soilHealth;
            }
        }

        return total + player.soilBoost;
    }

    public static int GetAestheticScore(Player player)
    {
        if (player == null || player.assignedGarden == null) return 0;

        int total = 0;
        GardenTile[] tiles = player.assignedGarden.GetComponentsInChildren<GardenTile>();

        foreach (GardenTile tile in tiles)
        {
            if (tile.occupied && tile.placedItemData != null)
            {
                total += tile.placedItemData.aesthetic;
            }
        }

        return total;
    }

    private static bool IsPlantType(ItemType type)
    {
        return type == ItemType.Plant || type == ItemType.Tree_Big || type == ItemType.Tree_Small;
    }
}