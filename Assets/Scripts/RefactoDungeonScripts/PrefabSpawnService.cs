using UnityEngine;
using VTools.Grid;

namespace Components.ProceduralGeneration.BSP
{//ici aussi beaucoup de variables et initialisation obsolète comme les corridors/rooms n'ont plus de walls
    public class PrefabSpawnService
    {
        private readonly bool _useDebugCubes;
        private readonly bool _showPivotDebug;
        private readonly int _roomSize;
        private readonly int _corridorSize;
        private readonly Color _roomDebugColor;
        private readonly Color _corridorDebugColor;

        private readonly DungeonRuntimeContext _ctx;
        private readonly Transform _parent;
        private readonly VTools.Grid.Grid _grid;

        private readonly GameObject _roomLeft;
        private readonly GameObject _roomRight;
        private readonly GameObject _roomTop;
        private readonly GameObject _roomBottom;

        private readonly GameObject _corridorHorizontal;
        private readonly GameObject _corridorVertical;
        private readonly GameObject _corridorCornerBR;
        private readonly GameObject _corridorCornerBL;
        private readonly GameObject _corridorCornerTR;
        private readonly GameObject _corridorCornerTL;

        public PrefabSpawnService(
            bool useDebugCubes,
            bool showPivotDebug,
            int roomSize,
            int corridorSize,
            Color roomDebugColor,
            Color corridorDebugColor,
            DungeonRuntimeContext ctx,
            Transform parent,
            GameObject roomLeft,
            GameObject roomRight,
            GameObject roomTop,
            GameObject roomBottom,
            GameObject corridorHorizontal,
            GameObject corridorVertical,
            GameObject corridorCornerBR,
            GameObject corridorCornerBL,
            GameObject corridorCornerTR,
            GameObject corridorCornerTL)
        {
            _useDebugCubes = useDebugCubes;
            _showPivotDebug = showPivotDebug;
            _roomSize = roomSize;
            _corridorSize = corridorSize;
            _roomDebugColor = roomDebugColor;
            _corridorDebugColor = corridorDebugColor;

            _ctx = ctx;
            _parent = parent;

            _roomLeft = roomLeft;
            _roomRight = roomRight;
            _roomTop = roomTop;
            _roomBottom = roomBottom;

            _corridorHorizontal = corridorHorizontal;
            _corridorVertical = corridorVertical;
            _corridorCornerBR = corridorCornerBR;
            _corridorCornerBL = corridorCornerBL;
            _corridorCornerTR = corridorCornerTR;
            _corridorCornerTL = corridorCornerTL;

            _grid = null;
        }

        public PrefabSpawnService(
            bool useDebugCubes,
            bool showPivotDebug,
            int roomSize,
            int corridorSize,
            Color roomDebugColor,
            Color corridorDebugColor,
            DungeonRuntimeContext ctx,
            Transform parent,
            VTools.Grid.Grid grid,
            GameObject roomLeft,
            GameObject roomRight,
            GameObject roomTop,
            GameObject roomBottom,
            GameObject corridorHorizontal,
            GameObject corridorVertical,
            GameObject corridorCornerBR,
            GameObject corridorCornerBL,
            GameObject corridorCornerTR,
            GameObject corridorCornerTL)
            : this(useDebugCubes, showPivotDebug, roomSize, corridorSize, roomDebugColor, corridorDebugColor,
                   ctx, parent, roomLeft, roomRight, roomTop, roomBottom, corridorHorizontal, corridorVertical,
                   corridorCornerBR, corridorCornerBL, corridorCornerTR, corridorCornerTL)
        {
            _grid = grid;
        }

        public void SpawnRooms()
        {
            foreach (var room in _ctx.Rooms)
            {
                Vector3 pos = new Vector3(room.floorCenter.x, room.floorCenter.y, 0f);

                if (_grid != null)
                {
                    pos.x += _grid.OriginPosition.x;
                    pos.y += _grid.OriginPosition.y;
                }

                if (_useDebugCubes)
                {
                    var cube = CreateDebugCube(pos, new Vector3(_roomSize, _roomSize, 1f), _roomDebugColor, $"Room_{room.bounds.xMin}_{room.bounds.yMin}");
                    _ctx.SpawnedObjects.Add(cube);
                    continue;
                }

                GameObject prefab = GetRoomPrefab(room.activeEntry);
                if (!prefab) continue;

                Quaternion rot = room.activeEntry switch
                {
                    RoomSide.Right => Quaternion.identity,
                    RoomSide.Left => Quaternion.Euler(0, 0, 180),
                    RoomSide.Top => Quaternion.Euler(0, 0, 90),
                    RoomSide.Bottom => Quaternion.Euler(0, 0, -90),
                    _ => Quaternion.identity
                };

                var instance = Object.Instantiate(prefab, pos, rot, _parent);
                _ctx.SpawnedObjects.Add(instance);

                if (_showPivotDebug)
                    CreatePivotSphere(instance);
            }
        }

        public void SpawnCorridors()
        {
            foreach (var kvp in _ctx.CorridorGrid)
            {
                var tile = kvp.Value;
                var type = tile.GetTileType();
                Vector3 pos = DungeonGridUtility.CorridorCellToWorld(tile.cellPosition, _corridorSize);

                if (_grid != null)
                {
                    pos.x += _grid.OriginPosition.x;
                    pos.y += _grid.OriginPosition.y;
                }

                if (_useDebugCubes)
                {
                    var cube = CreateDebugCube(pos, new Vector3(_corridorSize, _corridorSize, 1f), _corridorDebugColor, $"Corridor_{tile.cellPosition.x}_{tile.cellPosition.y}");
                    _ctx.SpawnedObjects.Add(cube);
                    continue;
                }

                GameObject prefab = GetCorridorPrefab(type);
                if (!prefab) continue;

                Quaternion rot = type == CorridorTileType.Horizontal ? Quaternion.Euler(0, 0, 90) : Quaternion.identity;
                var instance = Object.Instantiate(prefab, pos, rot, _parent);
                _ctx.SpawnedObjects.Add(instance);

                if (_showPivotDebug)
                    CreatePivotSphere(instance);
            }
        }

        private GameObject GetRoomPrefab(RoomSide side) =>
            side switch
            {
                RoomSide.Left => _roomLeft,
                RoomSide.Right => _roomRight,
                RoomSide.Top => _roomTop,
                RoomSide.Bottom => _roomBottom,
                _ => _roomLeft
            };

        private GameObject GetCorridorPrefab(CorridorTileType type) =>
            type switch
            {
                CorridorTileType.Horizontal => _corridorHorizontal,
                CorridorTileType.Vertical => _corridorVertical,
                CorridorTileType.CornerBottomRight => _corridorCornerBR,
                CorridorTileType.CornerBottomLeft => _corridorCornerBL,
                CorridorTileType.CornerTopRight => _corridorCornerTR,
                CorridorTileType.CornerTopLeft => _corridorCornerTL,
                _ => _corridorHorizontal
            };
        //Debug
        private GameObject CreateDebugCube(Vector3 center, Vector3 scale, Color color, string name)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(_parent, false);
            go.transform.position = center;
            go.transform.localScale = scale;

            if (go.TryGetComponent<Renderer>(out var r))
            {
                var mat = new Material(r.sharedMaterial) { color = color };
                r.sharedMaterial = mat;
            }

            if (go.TryGetComponent<Collider>(out var c3d)) Object.Destroy(c3d);
            if (go.TryGetComponent<Collider2D>(out var c2d)) Object.Destroy(c2d);

            return go;
        }
        
        private void CreatePivotSphere(GameObject parent)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(parent.transform, true);
            sphere.transform.position = parent.transform.position;
            sphere.transform.localScale = Vector3.one * 0.5f;
            if (sphere.TryGetComponent<Renderer>(out var r))
                r.material.color = Color.red;
            if (sphere.TryGetComponent<Collider>(out var c)) Object.Destroy(c);
            sphere.name = "PivotDebug";
        }
    }
}