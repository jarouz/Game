using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Places a few sample items on random walkable map tiles and lets the player collect them.
/// </summary>
public class DungeonItemSpawner2D : MonoBehaviour
{
    [SerializeField] private DungeonGenerator2D generator;
    [SerializeField] private DungeonTileRenderer tileRenderer;
    [SerializeField] private int itemCount = 6;

    private readonly Dictionary<Vector2Int, DungeonMapItemPickup> pickupsByPosition = new Dictionary<Vector2Int, DungeonMapItemPickup>();
    private Sprite placeholderSprite;

    /// <summary>
    /// Spawns collectible items on random floor or door tiles.
    /// </summary>
    [ContextMenu("Spawn Items")]
    public void SpawnItems()
    {
        ResolveReferences();
        if (generator == null || tileRenderer == null || generator.Map == null)
        {
            Debug.LogWarning("DungeonItemSpawner2D could not spawn items because the dungeon is not ready.");
            return;
        }

        ClearExistingItems();

        if (placeholderSprite == null)
        {
            placeholderSprite = CreatePlaceholderSprite();
        }

        List<Vector2Int> walkableTiles = new List<Vector2Int>();
        for (int x = 0; x < generator.Width; x++)
        {
            for (int y = 0; y < generator.Height; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                if (generator.IsWalkable(position) && position != generator.GetPlayerSpawnPosition())
                {
                    walkableTiles.Add(position);
                }
            }
        }

        int spawnTotal = Mathf.Min(itemCount, walkableTiles.Count);
        for (int i = 0; i < spawnTotal; i++)
        {
            int randomIndex = Random.Range(0, walkableTiles.Count);
            Vector2Int position = walkableTiles[randomIndex];
            walkableTiles.RemoveAt(randomIndex);

            InventoryItem item = CreateRandomItem();
            GameObject pickupObject = new GameObject();
            pickupObject.transform.SetParent(transform, false);

            DungeonMapItemPickup pickup = pickupObject.AddComponent<DungeonMapItemPickup>();
            pickup.Initialize(item, position, tileRenderer.GridToWorld(position), placeholderSprite, tileRenderer.TileSize);
            pickupsByPosition[position] = pickup;
        }
    }

    /// <summary>
    /// Gives the item at the player's current tile to the stats/inventory system.
    /// </summary>
    public bool TryCollectItem(Vector2Int gridPosition, PlayerStats playerStats)
    {
        if (!pickupsByPosition.TryGetValue(gridPosition, out DungeonMapItemPickup pickup))
        {
            return false;
        }

        playerStats.AddItem(pickup.Item);
        playerStats.AddExperience(5);
        pickupsByPosition.Remove(gridPosition);
        Destroy(pickup.gameObject);
        return true;
    }

    private InventoryItem CreateRandomItem()
    {
        int roll = Random.Range(0, 4);

        switch (roll)
        {
            case 0:
                return new InventoryItem
                {
                    ItemName = "Bronze Sword",
                    ItemType = InventoryItemType.Weapon,
                    EquipmentSlot = EquipmentSlot.Weapon,
                    StrengthBonus = 2,
                    WorldColor = new Color(0.8f, 0.8f, 0.2f)
                };
            case 1:
                return new InventoryItem
                {
                    ItemName = "Leather Armor",
                    ItemType = InventoryItemType.Armor,
                    EquipmentSlot = EquipmentSlot.Armor,
                    DefenseBonus = 2,
                    AgilityBonus = 1,
                    WorldColor = new Color(0.3f, 0.6f, 0.9f)
                };
            case 2:
                return new InventoryItem
                {
                    ItemName = "Healing Potion",
                    ItemType = InventoryItemType.Potion,
                    HealAmount = 6,
                    WorldColor = new Color(0.9f, 0.1f, 0.3f)
                };
            default:
                return new InventoryItem
                {
                    ItemName = "Gold Pouch",
                    ItemType = InventoryItemType.Gold,
                    GoldAmount = 15,
                    WorldColor = new Color(1f, 0.75f, 0.15f)
                };
        }
    }

    private void ResolveReferences()
    {
        if (generator == null)
        {
            generator = FindObjectOfType<DungeonGenerator2D>();
        }

        if (tileRenderer == null)
        {
            tileRenderer = FindObjectOfType<DungeonTileRenderer>();
        }
    }

    private void ClearExistingItems()
    {
        pickupsByPosition.Clear();
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    private Sprite CreatePlaceholderSprite()
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
    }
}
