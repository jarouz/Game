using UnityEngine;

/// <summary>
/// Builds colored placeholder tiles for the dungeon and manages fog-of-war visibility.
/// </summary>
public class DungeonTileRenderer : MonoBehaviour
{
    [SerializeField] private DungeonGenerator2D generator;
    [SerializeField] private float tileSize = 1f;

    [Header("Tile Colors")]
    [SerializeField] private Color wallColor = new Color(0.15f, 0.15f, 0.15f);
    [SerializeField] private Color floorColor = new Color(0.75f, 0.75f, 0.75f);
    [SerializeField] private Color doorColor = new Color(0.8f, 0.5f, 0.1f);

    [Header("Fog Of War")]
    [SerializeField] private Color unrevealedColor = Color.black;
    [SerializeField] private float exploredBrightness = 0.35f;

    private Sprite placeholderSprite;
    private SpriteRenderer[,] tileRenderers;
    private bool[,] revealedTiles;

    /// <summary>
    /// Tile size is exposed so the player controller can align movement to the visual grid.
    /// </summary>
    public float TileSize => tileSize;


    /// <summary>
    /// Creates tile GameObjects for each cell in the dungeon array and hides them behind fog.
    /// </summary>
    [ContextMenu("Build Visual Map")]
    public void BuildVisualMap()
    {
        ResolveGenerator();
        if (generator == null)
        {
            Debug.LogError("DungeonTileRenderer requires a DungeonGenerator2D reference.");
            return;
        }

        ClearExistingTiles();

        if (placeholderSprite == null)
        {
            placeholderSprite = CreatePlaceholderSprite();
        }

        DungeonGenerator2D.TileType[,] map = generator.Map;
        if (map == null)
        {
            Debug.LogWarning("No map data found. Generate a dungeon first.");
            return;
        }

        int width = map.GetLength(0);
        int height = map.GetLength(1);
        tileRenderers = new SpriteRenderer[width, height];
        revealedTiles = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tileRenderers[x, y] = CreateTile(x, y, map[x, y]);
                tileRenderers[x, y].color = unrevealedColor;
            }
        }
    }

    /// <summary>
    /// Updates fog-of-war so tiles inside radius are fully visible and previously seen tiles remain dimly visible.
    /// </summary>
    public void UpdateVisibility(Vector2Int playerGridPosition, int sightRadius)
    {
        if (tileRenderers == null || revealedTiles == null || generator.Map == null)
        {
            return;
        }

        int width = generator.Map.GetLength(0);
        int height = generator.Map.GetLength(1);

        // First dim any previously revealed tiles so old vision remains explored but not fully lit.
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                SpriteRenderer renderer = tileRenderers[x, y];
                if (renderer == null)
                {
                    continue;
                }

                if (revealedTiles[x, y])
                {
                    renderer.color = Color.Lerp(unrevealedColor, BaseColorForType(generator.Map[x, y]), exploredBrightness);
                }
                else
                {
                    renderer.color = unrevealedColor;
                }
            }
        }

        // Then reveal the tiles inside the player's field of view.
        for (int dx = -sightRadius; dx <= sightRadius; dx++)
        {
            for (int dy = -sightRadius; dy <= sightRadius; dy++)
            {
                Vector2Int tilePosition = new Vector2Int(playerGridPosition.x + dx, playerGridPosition.y + dy);

                if (!IsInside(tilePosition))
                {
                    continue;
                }

                // A radius of 1 reveals a compact 3x3 area around the player.
                if (Mathf.Abs(dx) > sightRadius || Mathf.Abs(dy) > sightRadius)
                {
                    continue;
                }

                SpriteRenderer renderer = tileRenderers[tilePosition.x, tilePosition.y];
                if (renderer == null)
                {
                    continue;
                }

                revealedTiles[tilePosition.x, tilePosition.y] = true;
                renderer.color = BaseColorForType(generator.Map[tilePosition.x, tilePosition.y]);
            }
        }
    }

    /// <summary>
    /// Converts a grid cell into a world position so other scripts can place objects on the map.
    /// </summary>
    public Vector3 GridToWorld(Vector2Int gridPosition)
    {
        Vector2 originOffset = GetMapOriginOffset();
        return new Vector3(originOffset.x + gridPosition.x * tileSize, originOffset.y + gridPosition.y * tileSize, 0f);
    }

    /// <summary>
    /// Offsets the dungeon so it is centered near the world origin instead of starting far in the positive quadrant.
    /// </summary>
    private Vector2 GetMapOriginOffset()
    {
        if (generator == null)
        {
            return Vector2.zero;
        }

        float xOffset = -((generator.Width - 1) * tileSize) * 0.5f;
        float yOffset = -((generator.Height - 1) * tileSize) * 0.5f;
        return new Vector2(xOffset, yOffset);
    }

    private SpriteRenderer CreateTile(int x, int y, DungeonGenerator2D.TileType type)
    {
        GameObject tile = new GameObject($"Tile_{x}_{y}_{type}");
        tile.transform.SetParent(transform, false);
        tile.transform.position = GridToWorld(new Vector2Int(x, y));
        tile.transform.localScale = Vector3.one * tileSize;

        SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
        renderer.sprite = placeholderSprite;
        renderer.color = BaseColorForType(type);
        renderer.sortingOrder = 0;
        return renderer;
    }

    private void ClearExistingTiles()
    {
        tileRenderers = null;
        revealedTiles = null;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }
    }

    private void ResolveGenerator()
    {
        if (generator == null)
        {
            generator = FindObjectOfType<DungeonGenerator2D>();
        }
    }

    private bool IsInside(Vector2Int point)
    {
        return point.x >= 0 && point.x < generator.Width && point.y >= 0 && point.y < generator.Height;
    }

    private Color BaseColorForType(DungeonGenerator2D.TileType type)
    {
        switch (type)
        {
            case DungeonGenerator2D.TileType.Floor:
                return floorColor;
            case DungeonGenerator2D.TileType.Door:
                return doorColor;
            default:
                return wallColor;
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
