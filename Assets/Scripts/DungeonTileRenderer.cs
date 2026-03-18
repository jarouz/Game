using UnityEngine;

/// <summary>
/// Instantiates simple colored tiles in the scene based on DungeonGenerator2D map data.
/// </summary>
public class DungeonTileRenderer : MonoBehaviour
{
    [SerializeField] private DungeonGenerator2D generator;
    [SerializeField] private float tileSize = 1f;

    [Header("Tile Colors")]
    [SerializeField] private Color wallColor = new Color(0.15f, 0.15f, 0.15f);
    [SerializeField] private Color floorColor = new Color(0.75f, 0.75f, 0.75f);
    [SerializeField] private Color doorColor = new Color(0.8f, 0.5f, 0.1f);

    // A shared 1x1 white sprite used as placeholder art for every tile.
    private Sprite placeholderSprite;

    private void Start()
    {
        if (generator == null)
        {
            generator = FindObjectOfType<DungeonGenerator2D>();
        }

        if (generator == null)
        {
            Debug.LogError("DungeonTileRenderer could not find a DungeonGenerator2D in the scene.");
            return;
        }

        // Ensure we have fresh map data before rendering.
        generator.GenerateDungeon();
        BuildVisualMap();
    }

    /// <summary>
    /// Creates tile GameObjects for each cell in the dungeon array.
    /// </summary>
    [ContextMenu("Build Visual Map")]
    public void BuildVisualMap()
    {
        // Remove existing tiles so the map can be regenerated cleanly.
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        // Lazily create a 1x1 white sprite and color it per tile type.
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

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CreateTile(x, y, map[x, y]);
            }
        }
    }

    /// <summary>
    /// Instantiates a single tile and colors it based on tile type.
    /// </summary>
    private void CreateTile(int x, int y, DungeonGenerator2D.TileType type)
    {
        GameObject tile = new GameObject($"Tile_{x}_{y}_{type}");
        tile.transform.SetParent(transform, false);
        tile.transform.position = new Vector3(x * tileSize, y * tileSize, 0f);
        tile.transform.localScale = Vector3.one * tileSize;

        SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
        renderer.sprite = placeholderSprite;
        renderer.color = ColorForType(type);
    }

    /// <summary>
    /// Returns the display color for each tile type.
    /// </summary>
    private Color ColorForType(DungeonGenerator2D.TileType type)
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

    /// <summary>
    /// Creates a plain white texture and turns it into a sprite.
    /// This lets us render colored placeholder tiles without importing art.
    /// </summary>
    private Sprite CreatePlaceholderSprite()
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}
