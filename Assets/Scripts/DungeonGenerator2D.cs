using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates a simple 2D dungeon map made of rectangular rooms connected by corridors.
/// The map data is stored in a 2D array so each tile knows its type.
/// </summary>
public class DungeonGenerator2D : MonoBehaviour
{
    /// <summary>
    /// Tile categories used by the dungeon map.
    /// </summary>
    public enum TileType
    {
        Wall,
        Floor,
        Door
    }

    [Header("Map Size")]
    [SerializeField] private int mapWidth = 40;
    [SerializeField] private int mapHeight = 28;

    [Header("Room Settings")]
    [SerializeField] private int roomCount = 10;
    [SerializeField] private Vector2Int roomSizeMin = new Vector2Int(4, 4);
    [SerializeField] private Vector2Int roomSizeMax = new Vector2Int(8, 8);
    [SerializeField] private int roomPadding = 1;

    /// <summary>
    /// Public read-only access to the generated map.
    /// </summary>
    public TileType[,] Map { get; private set; }

    /// <summary>
    /// Width helper so other scripts can read map dimensions without touching serialized fields.
    /// </summary>
    public int Width => mapWidth;

    /// <summary>
    /// Height helper so other scripts can read map dimensions without touching serialized fields.
    /// </summary>
    public int Height => mapHeight;

    /// <summary>
    /// Internal room description used during generation.
    /// </summary>
    private struct Room
    {
        public RectInt Bounds;
        public Vector2Int Center => new Vector2Int(Bounds.x + Bounds.width / 2, Bounds.y + Bounds.height / 2);
    }

    private readonly List<Room> rooms = new List<Room>();


    /// <summary>
    /// Creates a fresh dungeon map.
    /// 1) Fill map with walls.
    /// 2) Place non-overlapping rooms.
    /// 3) Connect room centers with L-shaped corridors.
    /// 4) Add doors where corridors meet room edges.
    /// </summary>
    [ContextMenu("Generate Dungeon")]
    public void GenerateDungeon()
    {
        // Initialize map and set every tile to wall by default.
        Map = new TileType[mapWidth, mapHeight];
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Map[x, y] = TileType.Wall;
            }
        }

        rooms.Clear();

        // Attempt to place the requested number of rooms.
        // We allow extra attempts so generation still works when space is tight.
        int maxAttempts = roomCount * 12;
        int attempts = 0;

        while (rooms.Count < roomCount && attempts < maxAttempts)
        {
            attempts++;

            int width = Random.Range(roomSizeMin.x, roomSizeMax.x + 1);
            int height = Random.Range(roomSizeMin.y, roomSizeMax.y + 1);
            int x = Random.Range(1, mapWidth - width - 1);
            int y = Random.Range(1, mapHeight - height - 1);

            Room newRoom = new Room { Bounds = new RectInt(x, y, width, height) };

            if (OverlapsExistingRoom(newRoom))
            {
                continue;
            }

            CarveRoom(newRoom);

            if (rooms.Count > 0)
            {
                Room previous = rooms[rooms.Count - 1];
                CarveCorridor(previous.Center, newRoom.Center);
                PlaceDoor(previous.Center, newRoom.Center);
                PlaceDoor(newRoom.Center, previous.Center);
            }

            rooms.Add(newRoom);
        }
    }

    /// <summary>
    /// Returns the center of the first room, which is a safe player spawn point.
    /// </summary>
    public Vector2Int GetPlayerSpawnPosition()
    {
        if (rooms.Count == 0)
        {
            GenerateDungeon();
        }

        if (rooms.Count == 0)
        {
            return new Vector2Int(1, 1);
        }

        return rooms[0].Center;
    }

    /// <summary>
    /// Returns true when the tile can be entered by the player.
    /// </summary>
    public bool IsWalkable(Vector2Int position)
    {
        if (!IsInside(position) || Map == null)
        {
            return false;
        }

        TileType tileType = Map[position.x, position.y];
        return tileType == TileType.Floor || tileType == TileType.Door;
    }

    /// <summary>
    /// Returns the tile type at a given cell. Cells outside the map are treated as walls.
    /// </summary>
    public TileType GetTileType(Vector2Int position)
    {
        if (!IsInside(position) || Map == null)
        {
            return TileType.Wall;
        }

        return Map[position.x, position.y];
    }

    /// <summary>
    /// Returns true if the room overlaps an existing room (including padding).
    /// </summary>
    private bool OverlapsExistingRoom(Room candidate)
    {
        RectInt expandedCandidate = new RectInt(
            candidate.Bounds.xMin - roomPadding,
            candidate.Bounds.yMin - roomPadding,
            candidate.Bounds.width + roomPadding * 2,
            candidate.Bounds.height + roomPadding * 2
        );

        foreach (Room room in rooms)
        {
            if (expandedCandidate.Overlaps(room.Bounds))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Carves room interior into floor tiles.
    /// </summary>
    private void CarveRoom(Room room)
    {
        for (int x = room.Bounds.xMin; x < room.Bounds.xMax; x++)
        {
            for (int y = room.Bounds.yMin; y < room.Bounds.yMax; y++)
            {
                Map[x, y] = TileType.Floor;
            }
        }
    }

    /// <summary>
    /// Carves an L-shaped corridor between two points.
    /// </summary>
    private void CarveCorridor(Vector2Int start, Vector2Int end)
    {
        // Randomize corridor bend direction to add variety.
        bool horizontalFirst = Random.value > 0.5f;

        if (horizontalFirst)
        {
            CarveHorizontal(start.x, end.x, start.y);
            CarveVertical(start.y, end.y, end.x);
        }
        else
        {
            CarveVertical(start.y, end.y, start.x);
            CarveHorizontal(start.x, end.x, end.y);
        }
    }

    /// <summary>
    /// Places a single door where a corridor leaves a room.
    /// </summary>
    private void PlaceDoor(Vector2Int roomCenter, Vector2Int targetCenter)
    {
        Vector2Int direction = targetCenter - roomCenter;

        if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y))
        {
            direction = new Vector2Int(direction.x >= 0 ? 1 : -1, 0);
        }
        else
        {
            direction = new Vector2Int(0, direction.y >= 0 ? 1 : -1);
        }

        Vector2Int current = roomCenter;
        Vector2Int previous = roomCenter;

        while (IsInside(current) && Map[current.x, current.y] != TileType.Wall)
        {
            previous = current;
            current += direction;
        }

        if (IsInside(previous) && Map[previous.x, previous.y] == TileType.Floor)
        {
            Map[previous.x, previous.y] = TileType.Door;
        }
    }

    /// <summary>
    /// Carves a horizontal corridor segment.
    /// </summary>
    private void CarveHorizontal(int xStart, int xEnd, int y)
    {
        int min = Mathf.Min(xStart, xEnd);
        int max = Mathf.Max(xStart, xEnd);

        for (int x = min; x <= max; x++)
        {
            if (IsInside(x, y) && Map[x, y] == TileType.Wall)
            {
                Map[x, y] = TileType.Floor;
            }
        }
    }

    /// <summary>
    /// Carves a vertical corridor segment.
    /// </summary>
    private void CarveVertical(int yStart, int yEnd, int x)
    {
        int min = Mathf.Min(yStart, yEnd);
        int max = Mathf.Max(yStart, yEnd);

        for (int y = min; y <= max; y++)
        {
            if (IsInside(x, y) && Map[x, y] == TileType.Wall)
            {
                Map[x, y] = TileType.Floor;
            }
        }
    }

    private bool IsInside(Vector2Int point)
    {
        return IsInside(point.x, point.y);
    }

    private bool IsInside(int x, int y)
    {
        return x >= 0 && x < mapWidth && y >= 0 && y < mapHeight;
    }
}
