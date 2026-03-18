using UnityEngine;

/// <summary>
/// Handles tile-by-tile WASD movement for a dungeon player and updates local field of view.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerDungeonController2D : MonoBehaviour
{
    [SerializeField] private DungeonGenerator2D generator;
    [SerializeField] private DungeonTileRenderer tileRenderer;
    [SerializeField] private int viewRadius = 1;
    [SerializeField] private Color playerColor = new Color(0.2f, 0.8f, 1f);

    /// <summary>
    /// Current player position in grid coordinates.
    /// </summary>
    public Vector2Int GridPosition { get; private set; }

    private SpriteRenderer spriteRenderer;
    private Sprite playerSprite;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ResolveReferences();

        if (generator == null || tileRenderer == null)
        {
            Debug.LogError("PlayerDungeonController2D requires both DungeonGenerator2D and DungeonTileRenderer in the scene.");
            enabled = false;
            return;
        }

        // Ensure the dungeon visuals exist before placing the player.
        if (generator.Map == null)
        {
            generator.GenerateDungeon();
        }

        tileRenderer.BuildVisualMap();
        SetupPlayerVisual();
        GridPosition = generator.GetPlayerSpawnPosition();
        SnapToGrid();
        UpdateVisibleTiles();
    }

    private void Update()
    {
        Vector2Int inputDirection = ReadMovementInput();
        if (inputDirection == Vector2Int.zero)
        {
            return;
        }

        TryMove(inputDirection);
    }

    /// <summary>
    /// Attempts to move a single tile in the requested direction.
    /// </summary>
    private void TryMove(Vector2Int direction)
    {
        Vector2Int targetPosition = GridPosition + direction;
        if (!generator.IsWalkable(targetPosition))
        {
            return;
        }

        GridPosition = targetPosition;
        SnapToGrid();
        UpdateVisibleTiles();
    }

    /// <summary>
    /// Reads one-step movement commands from WASD.
    /// GetKeyDown is used so the player moves exactly one tile per key press.
    /// </summary>
    private Vector2Int ReadMovementInput()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            return Vector2Int.up;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            return Vector2Int.down;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            return Vector2Int.left;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            return Vector2Int.right;
        }

        return Vector2Int.zero;
    }

    /// <summary>
    /// Updates fog-of-war around the player using the configured field-of-view radius.
    /// </summary>
    private void UpdateVisibleTiles()
    {
        tileRenderer.UpdateVisibility(GridPosition, viewRadius);
    }

    /// <summary>
    /// Converts the player's grid position to a world position on the tile map.
    /// </summary>
    private void SnapToGrid()
    {
        Vector3 worldPosition = tileRenderer.GridToWorld(GridPosition);
        transform.position = new Vector3(worldPosition.x, worldPosition.y, -0.1f);
    }

    /// <summary>
    /// Creates a colored square sprite for the player so no imported art is required.
    /// </summary>
    private void SetupPlayerVisual()
    {
        if (playerSprite == null)
        {
            playerSprite = CreatePlaceholderSprite();
        }

        spriteRenderer.sprite = playerSprite;
        spriteRenderer.color = playerColor;
        spriteRenderer.sortingOrder = 10;
        transform.localScale = Vector3.one * tileRenderer.TileSize;
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
