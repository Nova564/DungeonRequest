using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Components.ProceduralGeneration;
using VTools.RandomService;

namespace Components.ProceduralGeneration.BSP
{
    [CreateAssetMenu(menuName = "Procedural Generation Method/BSP Prefab Dungeon")]
    public class BspPrefabDungeonGeneration : ProceduralGenerationMethod
    {
        [Header("Dungeon Settings")]
        [SerializeField] private int _dungeonWidth = 100;
        [SerializeField] private int _dungeonHeight = 100;
        [SerializeField] private int _minRoomSize = 15;
        [SerializeField] private int _maxIterations = 4;

        [Header("Grid Constants")]
        [SerializeField] private int _roomSize = 5;
        [SerializeField] private int _corridorSize = 1;

        [Header("Room Prefabs (Single Entry)")]
        [SerializeField] private GameObject _roomPrefabLeft;
        [SerializeField] private GameObject _roomPrefabRight;
        [SerializeField] private GameObject _roomPrefabTop;
        [SerializeField] private GameObject _roomPrefabBottom;

        [Header("Corridor Prefabs")]
        [SerializeField] private GameObject _corridorHorizontal;
        [SerializeField] private GameObject _corridorVertical;
        [SerializeField] private GameObject _corridorCornerBottomRight;
        [SerializeField] private GameObject _corridorCornerBottomLeft;
        [SerializeField] private GameObject _corridorCornerTopRight;
        [SerializeField] private GameObject _corridorCornerTopLeft;

        [Header("Debug")]
        [SerializeField] private bool _useDebugCubes = true;
        [SerializeField] private bool _showPivotDebug = false;
        [SerializeField] private Color _roomDebugColor = new Color(0.2f, 0.9f, 0.2f, 0.6f);
        [SerializeField] private Color _corridorDebugColor = new Color(0.2f, 0.6f, 1f, 0.6f);

        [System.NonSerialized] private BSPNode _rootNode;
        [System.NonSerialized] private readonly List<RoomData> _rooms = new List<RoomData>();
        [System.NonSerialized] private readonly Dictionary<Vector2Int, CorridorTile> _corridorGrid = new Dictionary<Vector2Int, CorridorTile>();
        [System.NonSerialized] private readonly List<GameObject> _spawned = new List<GameObject>();
        [System.NonSerialized] private readonly List<ConnectionPoint> _activeConnections = new List<ConnectionPoint>();

        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            ClearRuntime();

            int width = Mathf.Clamp(_dungeonWidth, 1, Grid.Width);
            int height = Mathf.Clamp(_dungeonHeight, 1, Grid.Lenght);

            var dungeonBounds = new RectInt(0, 0, width, height);
            _rootNode = new BSPNode(dungeonBounds);

            SplitNode(_rootNode, 0, RandomService);
            CreateRooms(_rootNode);
            ConnectRooms(_rootNode, RandomService);
            DetermineRoomEntries();
            await UniTask.Yield(cancellationToken);

            InstantiateRooms();
            InstantiateCorridors();

            Debug.Log($"[BSP Prefab Dungeon] Generated {_rooms.Count} rooms and {_corridorGrid.Count} corridor tiles.");
        }

        private void ClearRuntime()
        {
            foreach (var go in _spawned)
            {
                if (go) Object.Destroy(go);
            }
            _spawned.Clear();
            _rooms.Clear();
            _corridorGrid.Clear();
            _activeConnections.Clear();
        }

        //le bsp

        private void SplitNode(BSPNode node, int iteration, RandomService rnd)
        {
            if (iteration >= _maxIterations) return;

            RectInt bounds = node.bounds;

            bool canSplitHorizontally = bounds.height >= _minRoomSize * 2;
            bool canSplitVertically = bounds.width >= _minRoomSize * 2;

            if (!canSplitHorizontally && !canSplitVertically) return;

            bool splitHorizontally = (canSplitHorizontally && canSplitVertically)
                ? rnd.Chance(0.5f)
                : canSplitHorizontally;

            if (splitHorizontally)
            {
                int splitY = rnd.Range(bounds.yMin + _minRoomSize, bounds.yMax - _minRoomSize);
                node.left = new BSPNode(new RectInt(bounds.xMin, bounds.yMin, bounds.width, splitY - bounds.yMin));
                node.right = new BSPNode(new RectInt(bounds.xMin, splitY, bounds.width, bounds.yMax - splitY));
            }
            else
            {
                int splitX = rnd.Range(bounds.xMin + _minRoomSize, bounds.xMax - _minRoomSize);
                node.left = new BSPNode(new RectInt(bounds.xMin, bounds.yMin, splitX - bounds.xMin, bounds.height));
                node.right = new BSPNode(new RectInt(splitX, bounds.yMin, bounds.xMax - splitX, bounds.height));
            }

            SplitNode(node.left, iteration + 1, rnd);
            SplitNode(node.right, iteration + 1, rnd);
        }

        private void CreateRooms(BSPNode node)
        {
            if (node.IsLeaf())
            {
                RectInt bounds = node.bounds;
                int centerX = bounds.xMin + bounds.width / 2;
                int centerY = bounds.yMin + bounds.height / 2;

                int roomX = SnapToGrid(centerX - _roomSize / 2, _roomSize);
                int roomY = SnapToGrid(centerY - _roomSize / 2, _roomSize);

                RectInt roomBounds = new RectInt(roomX, roomY, _roomSize, _roomSize);
                Vector2 roomCenter = new Vector2(roomX + _roomSize / 2f, roomY + _roomSize / 2f);

                var room = new RoomData
                {
                    bounds = roomBounds,
                    floorCenter = roomCenter
                };

                room.connectionPoints[RoomSide.Left] = new Vector2(roomX, roomCenter.y);
                room.connectionPoints[RoomSide.Right] = new Vector2(roomX + _roomSize, roomCenter.y);
                room.connectionPoints[RoomSide.Top] = new Vector2(roomCenter.x, roomY + _roomSize);
                room.connectionPoints[RoomSide.Bottom] = new Vector2(roomCenter.x, roomY);

                node.room = roomBounds;
                _rooms.Add(room);
            }
            else
            {
                if (node.left != null) CreateRooms(node.left);
                if (node.right != null) CreateRooms(node.right);
            }
        }

        private void ConnectRooms(BSPNode node, RandomService rnd)
        {
            if (node.IsLeaf()) return;
            if (node.left != null) ConnectRooms(node.left, rnd);
            if (node.right != null) ConnectRooms(node.right, rnd);

            if (node.left != null && node.right != null)
            {
                var leftRoom = GetRandomRoomFromNode(node.left, rnd);
                var rightRoom = GetRandomRoomFromNode(node.right, rnd);
                if (leftRoom != null && rightRoom != null)
                {
                    CreateCorridor(leftRoom, rightRoom, rnd);
                }
            }
        }

        private RoomData GetRandomRoomFromNode(BSPNode node, RandomService rnd)
        {
            if (node.IsLeaf() && node.room.HasValue)
                return _rooms.FirstOrDefault(r => r.bounds == node.room.Value);

            var nodeRooms = new List<RoomData>();
            CollectRooms(node, nodeRooms);
            return nodeRooms.Count > 0 ? nodeRooms[rnd.Range(0, nodeRooms.Count)] : null;
        }

        private void CollectRooms(BSPNode node, List<RoomData> collection)
        {
            if (node.IsLeaf() && node.room.HasValue)
            {
                var room = _rooms.FirstOrDefault(r => r.bounds == node.room.Value);
                if (room != null) collection.Add(room);
            }
            else
            {
                if (node.left != null) CollectRooms(node.left, collection);
                if (node.right != null) CollectRooms(node.right, collection);
            }
        }

        private void CreateCorridor(RoomData room1, RoomData room2, RandomService rnd)
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

            _activeConnections.Add(new ConnectionPoint { position = startPoint, side = side1 });
            _activeConnections.Add(new ConnectionPoint { position = endPoint, side = side2 });

            Vector2 corner = rnd.Chance(0.5f)
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

                if (!_corridorGrid.ContainsKey(cellCoord))
                {
                    _corridorGrid[cellCoord] = new CorridorTile { cellPosition = cellCoord };
                }

                if (Mathf.Abs(direction.x) > 0.5f)
                {
                    _corridorGrid[cellCoord].AddDirection(CorridorDirection.Horizontal);
                }
                if (Mathf.Abs(direction.y) > 0.5f)
                {
                    _corridorGrid[cellCoord].AddDirection(CorridorDirection.Vertical);
                }

                current += direction * _corridorSize;
                if (Vector2.Distance(from, current) > 1000f) break;
            }
        }

        private void DetermineRoomEntries()
        {
            foreach (var room in _rooms)
            {
                room.activeEntry = RoomSide.None;
                RoomSide[] priorities = { RoomSide.Left, RoomSide.Right, RoomSide.Top, RoomSide.Bottom };
                foreach (var side in priorities)
                {
                    if (room.usedSides.Contains(side))
                    {
                        Vector2 connectionPoint = room.connectionPoints[side];
                        Vector2Int cellCoord = WorldToCorridorCell(connectionPoint);
                        if (_corridorGrid.ContainsKey(cellCoord) || HasCorridorNearby(connectionPoint))
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
                    if (_corridorGrid.ContainsKey(center + new Vector2Int(dx, dy)))
                        return true;
            return false;
        }

        private void InstantiateRooms()
        {
            Transform parent = GridGenerator.transform;

            foreach (var room in _rooms)
            {
                Vector3 position = new Vector3(room.floorCenter.x, room.floorCenter.y, 0);

                if (_useDebugCubes)
                {
                    var cube = CreateDebugCube(position, new Vector3(_roomSize, _roomSize, 1f), _roomDebugColor, $"RoomCube_{room.bounds.xMin}_{room.bounds.yMin}", parent);
                    _spawned.Add(cube);
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

                    GameObject instance = Object.Instantiate(prefab, position, rotation, parent);
                    _spawned.Add(instance);

                    if (_showPivotDebug)
                        CreatePivotSphere(instance);
                }
            }
        }

        private void InstantiateCorridors()
        {
            Transform parent = GridGenerator.transform;

            foreach (var kvp in _corridorGrid)
            {
                CorridorTile tile = kvp.Value;
                CorridorTileType type = tile.GetTileType();
                Vector3 position = CorridorCellToWorld(tile.cellPosition);

                if (_useDebugCubes)
                {
                    var cube = CreateDebugCube(position, new Vector3(_corridorSize, _corridorSize, 1f), _corridorDebugColor, $"CorridorCube_{tile.cellPosition.x}_{tile.cellPosition.y}", parent);
                    _spawned.Add(cube);
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

                    GameObject instance = Object.Instantiate(prefab, position, rotation, parent);
                    _spawned.Add(instance);

                    if (_showPivotDebug)
                        CreatePivotSphere(instance);
                }
            }
        }

        // -------------------- Helpers --------------------

        private GameObject GetRoomPrefab(RoomSide entry)
        {
            switch (entry)
            {
                case RoomSide.Left: return _roomPrefabLeft;
                case RoomSide.Right: return _roomPrefabRight;
                case RoomSide.Top: return _roomPrefabTop;
                case RoomSide.Bottom: return _roomPrefabBottom;
                default: return _roomPrefabLeft;
            }
        }

        private GameObject GetCorridorPrefab(CorridorTileType type)
        {
            switch (type)
            {
                case CorridorTileType.Horizontal: return _corridorHorizontal;
                case CorridorTileType.Vertical: return _corridorVertical;
                case CorridorTileType.CornerBottomRight: return _corridorCornerBottomRight;
                case CorridorTileType.CornerBottomLeft: return _corridorCornerBottomLeft;
                case CorridorTileType.CornerTopRight: return _corridorCornerTopRight;
                case CorridorTileType.CornerTopLeft: return _corridorCornerTopLeft;
                default: return _corridorHorizontal;
            }
        }

        private GameObject CreateDebugCube(Vector3 center, Vector3 scale, Color color, string name, Transform parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
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
            if (col3d != null) Object.Destroy(col3d);
            var col2d = go.GetComponent<Collider2D>();
            if (col2d != null) Object.Destroy(col2d);

            return go;
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
            Object.Destroy(pivotDebug.GetComponent<Collider>());
        }

        private int SnapToGrid(int value, int gridSize) => Mathf.RoundToInt(value / (float)gridSize) * gridSize;

        private Vector2 SnapToCorridor(Vector2 point)
        {
            return new Vector2(
                SnapToGrid(Mathf.RoundToInt(point.x), _corridorSize),
                SnapToGrid(Mathf.RoundToInt(point.y), _corridorSize)
            );
        }

        private Vector2Int WorldToCorridorCell(Vector2 worldPos)
        {
            return new Vector2Int(
                Mathf.RoundToInt(worldPos.x / _corridorSize),
                Mathf.RoundToInt(worldPos.y / _corridorSize)
            );
        }

        private Vector3 CorridorCellToWorld(Vector2Int cell)
        {
            return new Vector3(
                cell.x * _corridorSize + _corridorSize / 2f,
                cell.y * _corridorSize + _corridorSize / 2f,
                0
            );
        }
    }
}