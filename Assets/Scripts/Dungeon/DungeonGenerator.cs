using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Dungeon Settings")]
    [SerializeField] private int dungeonWidth = 100;
    [SerializeField] private int dungeonHeight = 100;
    [SerializeField] private int minRoomSize = 15;
    [SerializeField] private int maxIterations = 4;

    [Header("Grid Constants")]
    [SerializeField] private int ROOM_SIZE = 5;
    [SerializeField] private int CORRIDOR_SIZE = 1;

    [Header("Room Prefabs (Single Entry)")]
    [SerializeField] private GameObject roomPrefabLeft;
    [SerializeField] private GameObject roomPrefabRight;
    [SerializeField] private GameObject roomPrefabTop;
    [SerializeField] private GameObject roomPrefabBottom;

    [Header("Corridor Prefabs")]
    [SerializeField] private GameObject corridorHorizontal;
    [SerializeField] private GameObject corridorVertical;
    [SerializeField] private GameObject corridorCornerBottomRight;
    [SerializeField] private GameObject corridorCornerBottomLeft;
    [SerializeField] private GameObject corridorCornerTopRight;
    [SerializeField] private GameObject corridorCornerTopLeft;

    [Header("Debug Placement")]
    [SerializeField] private bool useDebugCubes = true;
    [SerializeField] private bool showPivotDebug = false; 
    [SerializeField] private Color roomDebugColor = new Color(0.2f, 0.9f, 0.2f, 0.6f);
    [SerializeField] private Color corridorDebugColor = new Color(0.2f, 0.6f, 1f, 0.6f);
    [SerializeField] private List<GameObject> allPrefabsToDebugCheck = new List<GameObject>();

    private BSPNode rootNode;
    private List<RoomData> rooms = new List<RoomData>();
    private Dictionary<Vector2Int, CorridorTile> corridorGrid = new Dictionary<Vector2Int, CorridorTile>();
    private List<GameObject> instantiatedObjects = new List<GameObject>();
    private List<ConnectionPoint> activeConnections = new List<ConnectionPoint>();

    void Awake()
    {
        allPrefabsToDebugCheck.Clear();
        if (roomPrefabLeft != null) allPrefabsToDebugCheck.Add(roomPrefabLeft);
        if (roomPrefabRight != null) allPrefabsToDebugCheck.Add(roomPrefabRight);
        if (roomPrefabTop != null) allPrefabsToDebugCheck.Add(roomPrefabTop);
        if (roomPrefabBottom != null) allPrefabsToDebugCheck.Add(roomPrefabBottom);

        if (corridorHorizontal != null) allPrefabsToDebugCheck.Add(corridorHorizontal);
        if (corridorVertical != null) allPrefabsToDebugCheck.Add(corridorVertical);
        if (corridorCornerBottomRight != null) allPrefabsToDebugCheck.Add(corridorCornerBottomRight);
        if (corridorCornerBottomLeft != null) allPrefabsToDebugCheck.Add(corridorCornerBottomLeft);
        if (corridorCornerTopRight != null) allPrefabsToDebugCheck.Add(corridorCornerTopRight);
        if (corridorCornerTopLeft != null) allPrefabsToDebugCheck.Add(corridorCornerTopLeft);
    }

    void Start()
    {
        GenerateDungeon();
    }

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            RegenerateDungeon();
        }
    }

    public void RegenerateDungeon()
    {
        ClearDungeon();
        GenerateDungeon();
    }

    private void ClearDungeon()
    {
        foreach (var obj in instantiatedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        instantiatedObjects.Clear();
        rooms.Clear();
        corridorGrid.Clear();
        activeConnections.Clear();
    }

    private void GenerateDungeon()
    {
        RectInt dungeonBounds = new RectInt(0, 0, dungeonWidth, dungeonHeight);
        rootNode = new BSPNode(dungeonBounds);
        SplitNode(rootNode, 0);
        CreateRooms(rootNode);
        ConnectRooms(rootNode);
        DetermineRoomEntries();
        InstantiateRooms();
        InstantiateCorridors();
        Debug.Log($"Generated {rooms.Count} rooms and {corridorGrid.Count} corridor tiles");
    }

    private void SplitNode(BSPNode node, int iteration)
    {
        if (iteration >= maxIterations) return;

        RectInt bounds = node.bounds;

        bool canSplitHorizontally = bounds.height >= minRoomSize * 2;
        bool canSplitVertically = bounds.width >= minRoomSize * 2;

        if (!canSplitHorizontally && !canSplitVertically) return;

        bool splitHorizontally = (canSplitHorizontally && canSplitVertically)
            ? Random.value > 0.5f
            : canSplitHorizontally;

        if (splitHorizontally)
        {
            int splitY = Random.Range(bounds.yMin + minRoomSize, bounds.yMax - minRoomSize);
            node.left = new BSPNode(new RectInt(bounds.xMin, bounds.yMin, bounds.width, splitY - bounds.yMin));
            node.right = new BSPNode(new RectInt(bounds.xMin, splitY, bounds.width, bounds.yMax - splitY));
        }
        else
        {
            int splitX = Random.Range(bounds.xMin + minRoomSize, bounds.xMax - minRoomSize);
            node.left = new BSPNode(new RectInt(bounds.xMin, bounds.yMin, splitX - bounds.xMin, bounds.height));
            node.right = new BSPNode(new RectInt(splitX, bounds.yMin, bounds.xMax - splitX, bounds.height));
        }

        SplitNode(node.left, iteration + 1);
        SplitNode(node.right, iteration + 1);
    }

    private void CreateRooms(BSPNode node)
    {
        if (node.IsLeaf())
        {
            RectInt bounds = node.bounds;
            int centerX = bounds.xMin + bounds.width / 2;
            int centerY = bounds.yMin + bounds.height / 2;

            int roomX = SnapToGrid(centerX - ROOM_SIZE / 2, ROOM_SIZE);
            int roomY = SnapToGrid(centerY - ROOM_SIZE / 2, ROOM_SIZE);

            RectInt roomBounds = new RectInt(roomX, roomY, ROOM_SIZE, ROOM_SIZE);
            Vector2 roomCenter = new Vector2(roomX + ROOM_SIZE / 2f, roomY + ROOM_SIZE / 2f);

            RoomData room = new RoomData
            {
                bounds = roomBounds,
                floorCenter = roomCenter
            };

            room.connectionPoints[RoomSide.Left] = new Vector2(roomX, roomCenter.y);
            room.connectionPoints[RoomSide.Right] = new Vector2(roomX + ROOM_SIZE, roomCenter.y);
            room.connectionPoints[RoomSide.Top] = new Vector2(roomCenter.x, roomY + ROOM_SIZE);
            room.connectionPoints[RoomSide.Bottom] = new Vector2(roomCenter.x, roomY);

            node.room = roomBounds;
            rooms.Add(room);
        }
        else
        {
            if (node.left != null) CreateRooms(node.left);
            if (node.right != null) CreateRooms(node.right);
        }
    }

    private void ConnectRooms(BSPNode node)
    {
        if (node.IsLeaf()) return;
        if (node.left != null) ConnectRooms(node.left);
        if (node.right != null) ConnectRooms(node.right);

        if (node.left != null && node.right != null)
        {
            RoomData leftRoom = GetRandomRoomFromNode(node.left);
            RoomData rightRoom = GetRandomRoomFromNode(node.right);
            if (leftRoom != null && rightRoom != null)
            {
                CreateCorridor(leftRoom, rightRoom);
            }
        }
    }

    private RoomData GetRandomRoomFromNode(BSPNode node)
    {
        if (node.IsLeaf() && node.room.HasValue)
            return rooms.FirstOrDefault(r => r.bounds == node.room.Value);

        List<RoomData> nodeRooms = new List<RoomData>();
        CollectRooms(node, nodeRooms);
        return nodeRooms.Count > 0 ? nodeRooms[Random.Range(0, nodeRooms.Count)] : null;
    }

    private void CollectRooms(BSPNode node, List<RoomData> collection)
    {
        if (node.IsLeaf() && node.room.HasValue)
        {
            RoomData room = rooms.FirstOrDefault(r => r.bounds == node.room.Value);
            if (room != null) collection.Add(room);
        }
        else
        {
            if (node.left != null) CollectRooms(node.left, collection);
            if (node.right != null) CollectRooms(node.right, collection);
        }
    }

    private void CreateCorridor(RoomData room1, RoomData room2)
    {
        Vector2 start = room1.floorCenter;
        Vector2 end = room2.floorCenter;

        float dx = end.x - start.x;
        float dy = end.y - start.y;

        RoomSide side1, side2;
        if (Mathf.Abs(dx) > Mathf.Abs(dy))
        {
            side1 = dx > 0 ? RoomSide.Right : RoomSide.Left;
            side2 = dx > 0 ? RoomSide.Left : RoomSide.Right;
        }
        else
        {
            side1 = dy > 0 ? RoomSide.Top : RoomSide.Bottom;
            side2 = dy > 0 ? RoomSide.Bottom : RoomSide.Top;
        }

        Vector2 startPoint = room1.connectionPoints[side1];
        Vector2 endPoint = room2.connectionPoints[side2];

        startPoint = SnapToCorridor(startPoint);
        endPoint = SnapToCorridor(endPoint);

        room1.usedSides.Add(side1);
        room2.usedSides.Add(side2);

        activeConnections.Add(new ConnectionPoint { position = startPoint, side = side1 });
        activeConnections.Add(new ConnectionPoint { position = endPoint, side = side2 });

        Vector2 corner = Random.value > 0.5f
            ? new Vector2(endPoint.x, startPoint.y)
            : new Vector2(startPoint.x, endPoint.y);

        DrawCorridorLine(startPoint, corner);
        DrawCorridorLine(corner, endPoint);
    }

    private void DrawCorridorLine(Vector2 from, Vector2 to)
    {
        Vector2 current = from;
        Vector2 direction = (to - from).normalized;

        while (Vector2.Distance(current, to) > 0.1f)
        {
            Vector2Int cellCoord = WorldToCorridorCell(current);

            if (!corridorGrid.ContainsKey(cellCoord))
            {
                corridorGrid[cellCoord] = new CorridorTile { cellPosition = cellCoord };
            }

            if (Mathf.Abs(direction.x) > 0.5f)
            {
                corridorGrid[cellCoord].AddDirection(CorridorDirection.Horizontal);
            }
            if (Mathf.Abs(direction.y) > 0.5f)
            {
                corridorGrid[cellCoord].AddDirection(CorridorDirection.Vertical);
            }

            current += direction * CORRIDOR_SIZE;
            if (Vector2.Distance(from, current) > 1000f) break;
        }
    }

    private void DetermineRoomEntries()
    {
        foreach (var room in rooms)
        {
            room.activeEntry = RoomSide.None;
            RoomSide[] priorities = { RoomSide.Left, RoomSide.Right, RoomSide.Top, RoomSide.Bottom };
            foreach (var side in priorities)
            {
                if (room.usedSides.Contains(side))
                {
                    Vector2 connectionPoint = room.connectionPoints[side];
                    Vector2Int cellCoord = WorldToCorridorCell(connectionPoint);
                    if (corridorGrid.ContainsKey(cellCoord) || HasCorridorNearby(connectionPoint))
                    {
                        room.activeEntry = side;
                        break;
                    }
                }
            }
            if (room.activeEntry == RoomSide.None && room.usedSides.Count > 0)
                room.activeEntry = room.usedSides[0];
        }
    }

    private bool HasCorridorNearby(Vector2 point)
    {
        Vector2Int center = WorldToCorridorCell(point);
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
                if (corridorGrid.ContainsKey(center + new Vector2Int(dx, dy)))
                    return true;
        return false;
    }

    private void InstantiateRooms()
    {
        foreach (var room in rooms)
        {
            Vector3 position = new Vector3(room.floorCenter.x, room.floorCenter.y, 0);

            if (useDebugCubes)
            {
                var cube = CreateDebugCube(position, new Vector3(ROOM_SIZE, ROOM_SIZE, 1f), roomDebugColor, $"RoomCube_{room.bounds.xMin}_{room.bounds.yMin}");
                instantiatedObjects.Add(cube);
            }
            else
            {
                GameObject prefab = GetRoomPrefab(room.activeEntry);
                if (prefab == null) continue;

                Quaternion rotation = Quaternion.identity;
                switch (room.activeEntry)
                {
                    case RoomSide.Right: rotation = Quaternion.identity; break;
                    case RoomSide.Left: rotation = Quaternion.Euler(0, 0, 180); break;
                    case RoomSide.Top: rotation = Quaternion.Euler(0, 0, 90); break;
                    case RoomSide.Bottom: rotation = Quaternion.Euler(0, 0, -90); break;
                }
                GameObject instance = Instantiate(prefab, position, rotation, transform);
                instantiatedObjects.Add(instance);

                if (showPivotDebug)
                    CreatePivotSphere(instance);
            }
        }
    }

    private void InstantiateCorridors()
    {
        foreach (var kvp in corridorGrid)
        {
            CorridorTile tile = kvp.Value;
            CorridorTileType type = tile.GetTileType();
            Vector3 position = CorridorCellToWorld(tile.cellPosition);

            if (useDebugCubes)
            {
                var cube = CreateDebugCube(position, new Vector3(CORRIDOR_SIZE, CORRIDOR_SIZE, 1f), corridorDebugColor, $"CorridorCube_{tile.cellPosition.x}_{tile.cellPosition.y}");
                instantiatedObjects.Add(cube);
            }
            else
            {
                GameObject prefab = GetCorridorPrefab(type);
                if (prefab == null) continue;
                Quaternion rotation = Quaternion.identity;
                if (type == CorridorTileType.Horizontal)
                {
                    rotation = Quaternion.Euler(0, 0, 90);
                }
                GameObject instance = Instantiate(prefab, position, rotation, transform);
                instantiatedObjects.Add(instance);

                if (showPivotDebug)
                    CreatePivotSphere(instance);
            }
        }
    }

    private void CreatePivotSphere(GameObject parent)
    {
        GameObject pivotDebug = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pivotDebug.transform.position = parent.transform.position;
        pivotDebug.transform.localScale = Vector3.one * 0.5f;
        var r = pivotDebug.GetComponent<Renderer>();
        if (r != null)
        {
            r.material.color = Color.red;
        }
        pivotDebug.name = "PivotDebug";
        pivotDebug.transform.SetParent(parent.transform, true);
        Destroy(pivotDebug.GetComponent<Collider>());
    }

    private GameObject GetRoomPrefab(RoomSide entry)
    {
        switch (entry)
        {
            case RoomSide.Left: return roomPrefabLeft;
            case RoomSide.Right: return roomPrefabRight;
            case RoomSide.Top: return roomPrefabTop;
            case RoomSide.Bottom: return roomPrefabBottom;
            default: return roomPrefabLeft;
        }
    }

    private GameObject GetCorridorPrefab(CorridorTileType type)
    {
        switch (type)
        {
            case CorridorTileType.Horizontal: return corridorHorizontal;
            case CorridorTileType.Vertical: return corridorVertical;
            case CorridorTileType.CornerBottomRight: return corridorCornerBottomRight;
            case CorridorTileType.CornerBottomLeft: return corridorCornerBottomLeft;
            case CorridorTileType.CornerTopRight: return corridorCornerTopRight;
            case CorridorTileType.CornerTopLeft: return corridorCornerTopLeft;
            default: return corridorHorizontal;
        }
    }

    private GameObject CreateDebugCube(Vector3 center, Vector3 scale, Color color, string name)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(transform, false);
        go.transform.position = center;
        go.transform.localScale = scale;

        var renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            var mat = new Material(renderer.sharedMaterial);
            mat.color = color;
            renderer.sharedMaterial = mat;
        }

        var col3d = go.GetComponent<Collider>();
        if (col3d != null) Destroy(col3d);
        var col2d = go.GetComponent<Collider2D>();
        if (col2d != null) Destroy(col2d);

        return go;
    }

    private int SnapToGrid(int value, int gridSize)
    {
        return Mathf.RoundToInt(value / (float)gridSize) * gridSize;
    }

    private Vector2 SnapToCorridor(Vector2 point)
    {
        return new Vector2(
            SnapToGrid(Mathf.RoundToInt(point.x), CORRIDOR_SIZE),
            SnapToGrid(Mathf.RoundToInt(point.y), CORRIDOR_SIZE)
        );
    }

    private Vector2Int WorldToCorridorCell(Vector2 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x / CORRIDOR_SIZE),
            Mathf.RoundToInt(worldPos.y / CORRIDOR_SIZE)
        );
    }

    private Vector3 CorridorCellToWorld(Vector2Int cell)
    {
        return new Vector3(
            cell.x * CORRIDOR_SIZE + CORRIDOR_SIZE / 2f,
            cell.y * CORRIDOR_SIZE + CORRIDOR_SIZE / 2f,
            0
        );
    }

    private void OnDrawGizmos()
    {
        if (rooms == null || rooms.Count == 0) return;

        Gizmos.color = Color.green;
        foreach (var room in rooms)
        {
            Vector3 center = new Vector3(room.floorCenter.x, room.floorCenter.y, 0);
            Vector3 size = new Vector3(ROOM_SIZE, ROOM_SIZE, 1);
            Gizmos.DrawWireCube(center, size);
        }

        Gizmos.color = Color.yellow;
        foreach (var conn in activeConnections)
        {
            Gizmos.DrawSphere(new Vector3(conn.position.x, conn.position.y, 0), 0.3f);
        }

        Gizmos.color = Color.blue;
        foreach (var tile in corridorGrid.Values)
        {
            Vector3 pos = CorridorCellToWorld(tile.cellPosition);
            Vector3 size = new Vector3(CORRIDOR_SIZE, CORRIDOR_SIZE, 1);
            Gizmos.DrawWireCube(pos, size);
        }
    }
}