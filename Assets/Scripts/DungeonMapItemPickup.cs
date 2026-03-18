using UnityEngine;

/// <summary>
/// Represents a single item placed on the dungeon grid that the player can collect.
/// </summary>
public class DungeonMapItemPickup : MonoBehaviour
{
    public InventoryItem Item { get; private set; }
    public Vector2Int GridPosition { get; private set; }

    private SpriteRenderer spriteRenderer;

    /// <summary>
    /// Initializes the pickup visual and data.
    /// </summary>
    public void Initialize(InventoryItem item, Vector2Int gridPosition, Sprite sprite, float tileSize)
    {
        Item = item;
        GridPosition = gridPosition;
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = item.Icon != null ? item.Icon : sprite;
        spriteRenderer.color = item.WorldColor;
        spriteRenderer.sortingOrder = 5;

        transform.position = new Vector3(gridPosition.x * tileSize, gridPosition.y * tileSize, -0.05f);
        transform.localScale = Vector3.one * tileSize * 0.6f;
        gameObject.name = "Pickup_" + item.ItemName.Replace(' ', '_');
    }
}
