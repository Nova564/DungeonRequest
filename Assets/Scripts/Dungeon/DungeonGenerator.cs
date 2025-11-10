using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Dungeon Settings (en tiles Unity)")]
    [SerializeField] private int dungeonWidth = 50;
    [SerializeField] private int dungeonHeight = 50;
    [SerializeField] private int minRoomSize = 10;
    [SerializeField] private int maxIterations = 4;

    [Header("Constants (NE PAS MODIFIER)")]
    [SerializeField] private int roomSize = 5;
    [SerializeField] private int corridorSize = 3;

    [Header("Room Prefabs - Single Entry (5x5)")]
    [SerializeField] private GameObject roomSingleLeft;
    [SerializeField] private GameObject roomSingleRight;
    [SerializeField] private GameObject roomSingleTop;
    [SerializeField] private GameObject roomSingleBottom;

    [Header("Corridor Prefabs - Straight (3x3)")]
    [SerializeField] private GameObject corridorHorizontal;
    [SerializeField] private GameObject corridorVertical;

    [Header("Corridor Prefabs - Corners (3x3)")]
    [SerializeField] private GameObject cornerBottomRight;
    [SerializeField] private GameObject cornerBottomLeft;
    [SerializeField] private GameObject cornerTopRight;
    [SerializeField] private GameObject cornerTopLeft;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;

    private BSPNode root;
    private List<RoomData> rooms = new List<RoomData>();
    private Dictionary<Vector2Int, CorridorTile> corridorGrid = new Dictionary<Vector2Int, CorridorTile>();
    private Dictionary<Vector2Int, RoomEntries> activeConnections = new Dictionary<Vector2Int, RoomEntries>();

    void Start()
    {
        GenerateDungeon();
    }

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("Regeneration du donjon...");
            GenerateDungeon();
        }
    }

    public void GenerateDungeon()
    {
        ClearDungeon();

        Debug.Log("=== GENERATION DONJON ===");

        root = new BSPNode(new RectInt(0, 0, dungeonWidth, dungeonHeight));
        SplitNode(root, 0);

        CreateRooms(root);
        Debug.Log($"Rooms: {rooms.Count}");

        CreateCorridors(root);
        Debug.Log($"Corridors: {corridorGrid.Count}");

        CalculateActiveConnections();
        InstantiateRooms();
        InstantiateCorridors();

        Debug.Log("=== GENERATION TERMINEE ===");
    }

    private void SplitNode(BSPNode node, int iteration)
    {
        if (iteration >= maxIterations)
            return;

        bool canSplitH = node.bounds.height >= minRoomSize * 2;
        bool canSplitV = node.bounds.width >= minRoomSize * 2;

        if (!canSplitH && !canSplitV)
            return;

        bool splitH = (canSplitH && canSplitV) ? Random.value > 0.5f : canSplitH;

        if (splitH)
        {
            int splitY = Random.Range(node.bounds.y + minRoomSize, node.bounds.y + node.bounds.height - minRoomSize);
            splitY = Mathf.RoundToInt(splitY / (float)roomSize) * roomSize;

            node.left = new BSPNode(new RectInt(node.bounds.x, node.bounds.y, node.bounds.width, splitY - node.bounds.y));
            node.right = new BSPNode(new RectInt(node.bounds.x, splitY, node.bounds.width, node.bounds.y + node.bounds.height - splitY));
        }
        else
        {
            int splitX = Random.Range(node.bounds.x + minRoomSize, node.bounds.x + node.bounds.width - minRoomSize);
            splitX = Mathf.RoundToInt(splitX / (float)roomSize) * roomSize;

            node.left = new BSPNode(new RectInt(node.bounds.x, node.bounds.y, splitX - node.bounds.x, node.bounds.height));
            node.right = new BSPNode(new RectInt(splitX, node.bounds.y, node.bounds.x + node.bounds.width - splitX, node.bounds.height));
        }

        SplitNode(node.left, iteration + 1);
        SplitNode(node.right, iteration + 1);
    }

    private void CreateRooms(BSPNode node)
    {
        if (node == null) return;

        if (node.IsLeaf())
        {
            int maxRoomsX = node.bounds.width / roomSize;
            int maxRoomsY = node.bounds.height / roomSize;

            if (maxRoomsX < 1 || maxRoomsY < 1)
                return;

            int roomX = node.bounds.x + (node.bounds.width - roomSize) / 2;
            int roomY = node.bounds.y + (node.bounds.height - roomSize) / 2;

            roomX = Mathf.RoundToInt(roomX / (float)roomSize) * roomSize;
            roomY = Mathf.RoundToInt(roomY / (float)roomSize) * roomSize;

            node.room = new RectInt(roomX, roomY, roomSize, roomSize);
            rooms.Add(new RoomData(node.room));
        }
        else
        {
            CreateRooms(node.left);
            CreateRooms(node.right);
        }
    }

    private void CreateCorridors(BSPNode node)
    {
        if (node == null || node.IsLeaf())
            return;

        CreateCorridors(node.left);
        CreateCorridors(node.right);

        RoomData room1 = GetRandomRoomInNode(node.left);
        RoomData room2 = GetRandomRoomInNode(node.right);

        if (room1 != null && room2 != null)
        {
            ConnectRooms(room1, room2);
        }
    }

    private RoomData GetRandomRoomInNode(BSPNode node)
    {
        if (node == null) return null;

        if (node.IsLeaf())
        {
            return rooms.FirstOrDefault(r => r.bounds == node.room);
        }

        var leftRoom = GetRandomRoomInNode(node.left);
        var rightRoom = GetRandomRoomInNode(node.right);

        if (leftRoom != null && rightRoom != null)
            return Random.value > 0.5f ? leftRoom : rightRoom;

        return leftRoom ?? rightRoom;
    }

    private void ConnectRooms(RoomData room1, RoomData room2)
    {
        RoomEntries side1, side2;
        DetermineBestConnectionSides(room1, room2, out side1, out side2);

        ConnectionPoint point1 = room1.GetConnection(side1);
        ConnectionPoint point2 = room2.GetConnection(side2);

        Vector2Int start = new Vector2Int(
            Mathf.RoundToInt(point1.position.x / (float)corridorSize) * corridorSize,
            Mathf.RoundToInt(point1.position.y / (float)corridorSize) * corridorSize
        );

        Vector2Int end = new Vector2Int(
            Mathf.RoundToInt(point2.position.x / (float)corridorSize) * corridorSize,
            Mathf.RoundToInt(point2.position.y / (float)corridorSize) * corridorSize
        );

        CreateLShapedCorridor(start, end);
    }

    private void DetermineBestConnectionSides(RoomData room1, RoomData room2, out RoomEntries side1, out RoomEntries side2)
    {
        Vector2Int c1 = room1.floorCenter;
        Vector2Int c2 = room2.floorCenter;

        int dx = c2.x - c1.x;
        int dy = c2.y - c1.y;

        if (Mathf.Abs(dx) > Mathf.Abs(dy))
        {
            if (dx > 0)
            {
                side1 = RoomEntries.Right;
                side2 = RoomEntries.Left;
            }
            else
            {
                side1 = RoomEntries.Left;
                side2 = RoomEntries.Right;
            }
        }
        else
        {
            if (dy > 0)
            {
                side1 = RoomEntries.Top;
                side2 = RoomEntries.Bottom;
            }
            else
            {
                side1 = RoomEntries.Bottom;
                side2 = RoomEntries.Top;
            }
        }
    }

    private void CreateLShapedCorridor(Vector2Int start, Vector2Int end)
    {
        if (Random.value > 0.5f)
        {
            Vector2Int corner = new Vector2Int(end.x, start.y);
            AddCorridorPath(start, corner);
            AddCorridorPath(corner, end);
        }
        else
        {
            Vector2Int corner = new Vector2Int(start.x, end.y);
            AddCorridorPath(start, corner);
            AddCorridorPath(corner, end);
        }
    }

    private void AddCorridorPath(Vector2Int from, Vector2Int to)
    {
        if (from == to)
            return;

        Vector2Int dir = new Vector2Int(
            Mathf.Clamp(to.x - from.x, -1, 1),
            Mathf.Clamp(to.y - from.y, -1, 1)
        );

        Vector2Int current = from;

        while (current != to)
        {
            Vector2Int gridPos = new Vector2Int(
                current.x / corridorSize,
                current.y / corridorSize
            );

            if (!corridorGrid.ContainsKey(gridPos))
            {
                corridorGrid[gridPos] = new CorridorTile(gridPos);
            }

            corridorGrid[gridPos].AddDirection(dir);
            corridorGrid[gridPos].AddDirection(-dir);

            current += dir * corridorSize;
        }

        Vector2Int lastGrid = new Vector2Int(to.x / corridorSize, to.y / corridorSize);

        if (!corridorGrid.ContainsKey(lastGrid))
        {
            corridorGrid[lastGrid] = new CorridorTile(lastGrid);
        }
        corridorGrid[lastGrid].AddDirection(-dir);
    }

    private void CalculateActiveConnections()
    {
        activeConnections.Clear();

        RoomEntries[] order = new[] { RoomEntries.Left, RoomEntries.Right, RoomEntries.Top, RoomEntries.Bottom };

        foreach (RoomData room in rooms)
        {
            RoomEntries chosen = RoomEntries.None;

            foreach (var side in order)
            {
                if (!room.connections.ContainsKey(side)) continue;

                Vector2Int connPos = room.connections[side].position;
                bool found = false;
                for (int dx = -1; dx <= 1 && !found; dx++)
                {
                    for (int dy = -1; dy <= 1 && !found; dy++)
                    {
                        Vector2Int checkGrid = new Vector2Int(
                            (connPos.x + dx * corridorSize) / corridorSize,
                            (connPos.y + dy * corridorSize) / corridorSize
                        );

                        if (corridorGrid.ContainsKey(checkGrid))
                        {
                            found = true;
                            chosen = side;
                        }
                    }
                }

                if (found) break; // keep only one entry
            }

            activeConnections[room.floorCenter] = chosen;
        }
    }

    private void InstantiateRooms()
    {
        foreach (RoomData room in rooms)
        {
            RoomEntries entries = activeConnections.ContainsKey(room.floorCenter) ?
                activeConnections[room.floorCenter] : RoomEntries.None;

            GameObject prefab = GetRoomPrefab(entries);

            if (prefab != null)
            {
                Vector3 pos = new Vector3(room.bounds.x, room.bounds.y, 0);
                GameObject instance = Instantiate(prefab, pos, Quaternion.identity, transform);
                instance.name = $"Room_{entries}_{room.floorCenter.x}_{room.floorCenter.y}";
            }
        }
    }

    private GameObject GetRoomPrefab(RoomEntries entries)
    {
        // Only spawn when exactly one entry is chosen
        if (entries == RoomEntries.Left) return roomSingleLeft;
        if (entries == RoomEntries.Right) return roomSingleRight;
        if (entries == RoomEntries.Top) return roomSingleTop;
        if (entries == RoomEntries.Bottom) return roomSingleBottom;

        // No active entry -> do not place a single-entry prefab
        return null;
    }

    private void InstantiateCorridors()
    {
        foreach (var kvp in corridorGrid)
        {
            CorridorTile tile = kvp.Value;
            Vector3 pos = new Vector3(tile.position.x * corridorSize, tile.position.y * corridorSize, 0);

            GameObject prefab = GetCorridorPrefab(tile.type);

            if (prefab != null)
            {
                GameObject instance = Instantiate(prefab, pos, Quaternion.identity, transform);
                instance.name = $"Corridor_{tile.type}_{tile.position.x}_{tile.position.y}";
            }
        }
    }

    private GameObject GetCorridorPrefab(CorridorTileType type)
    {
        switch (type)
        {
            case CorridorTileType.Horizontal: return corridorHorizontal;
            case CorridorTileType.Vertical: return corridorVertical;
            case CorridorTileType.CornerBottomRight: return cornerBottomRight;
            case CorridorTileType.CornerBottomLeft: return cornerBottomLeft;
            case CorridorTileType.CornerTopRight: return cornerTopRight;
            case CorridorTileType.CornerTopLeft: return cornerTopLeft;
            default: return corridorHorizontal;
        }
    }

    private void ClearDungeon()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        rooms.Clear();
        corridorGrid.Clear();
        activeConnections.Clear();
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || rooms == null || rooms.Count == 0)
            return;

        Gizmos.color = Color.green;
        foreach (RoomData room in rooms)
        {
            Gizmos.DrawWireCube(
                new Vector3(room.bounds.x + room.bounds.width / 2f, room.bounds.y + room.bounds.height / 2f, 0),
                new Vector3(room.bounds.width, room.bounds.height, 0.1f)
            );

            RoomEntries used = activeConnections.ContainsKey(room.floorCenter) ? activeConnections[room.floorCenter] : RoomEntries.None;

            if (used != RoomEntries.None && room.connections.ContainsKey(used))
            {
                Gizmos.color = Color.yellow;
                Vector2Int p = room.connections[used].position;
                Vector3 connPos = new Vector3(p.x, p.y, 0);
                Gizmos.DrawSphere(connPos, 0.3f);
                Gizmos.color = Color.green;
            }
        }

        if (corridorGrid != null)
        {
            Gizmos.color = Color.blue;
            foreach (var kvp in corridorGrid)
            {
                Vector3 pos = new Vector3(
                    kvp.Value.position.x * corridorSize + corridorSize / 2f,
                    kvp.Value.position.y * corridorSize + corridorSize / 2f,
                    0
                );
                Gizmos.DrawWireCube(pos, new Vector3(corridorSize, corridorSize, 0.1f));
            }
        }
    }
}