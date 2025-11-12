using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Components.ProceduralGeneration;
using VTools.RandomService;

namespace Components.ProceduralGeneration.BSP
{
    [CreateAssetMenu(menuName = "Procedural Generation Method/BSP Prefab Dungeon (Refactored)")]
    public class BspPrefabDungeonGeneration : ProceduralGenerationMethod
    {
        [Header("Dungeon Bounds")]
        [SerializeField] private int _dungeonWidth = 100;
        [SerializeField] private int _dungeonHeight = 100;

        [Header("BSP")]
        [SerializeField] private int _minRoomSize = 15;
        [SerializeField] private int _maxIterations = 4;

        [Header("Rooms")]
        [SerializeField] private int _roomSize = 5;

        [Header("Corridors")]
        [SerializeField] private int _corridorSize = 1;

        [Header("Walls")]
        [SerializeField] private bool _generateWalls = true;
        [SerializeField] private GameObject _wallPrefab;

        [Header("Player Spawn")]
        [SerializeField, Tooltip("Transform du player (si pas renseigner il ira chercher un tag Player")]
        private Transform _player;

        [Header("Debug")]
        [SerializeField] private bool _useDebugCubes = true;
        [SerializeField] private bool _showPivotDebug = false;
        [SerializeField] private Color _roomDebugColor = new Color(0.2f, 0.9f, 0.2f, 0.6f);
        [SerializeField] private Color _corridorDebugColor = new Color(0.2f, 0.6f, 1f, 0.6f);

        [Header("Room Prefabs")]
        [SerializeField] private GameObject _roomLeft;
        [SerializeField] private GameObject _roomRight;
        [SerializeField] private GameObject _roomTop;
        [SerializeField] private GameObject _roomBottom;

        [Header("Corridor Prefabs")]
        [SerializeField] private GameObject _corridorHorizontal;
        [SerializeField] private GameObject _corridorVertical;
        [SerializeField] private GameObject _corridorCornerBR;
        [SerializeField] private GameObject _corridorCornerBL;
        [SerializeField] private GameObject _corridorCornerTR;
        [SerializeField] private GameObject _corridorCornerTL;

        private DungeonRuntimeContext _runtimeContext;
        private BspTreeBuilder _tree;
        private RoomPlacementService _rooms;
        private CorridorService _corridors;
        private PrefabSpawnService _spawner;
        private WallGenerationService _walls;

        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            InitializeServices();

            _runtimeContext.Clear();

            _runtimeContext.RootNode = _tree.BuildRoot(Grid.Width, Grid.Lenght);
            _tree.SplitRecursive(_runtimeContext.RootNode, 0);

            _rooms.CreateRooms(_runtimeContext.RootNode);

            ConnectLeafRooms();

            _rooms.DetermineRoomEntries();

            await UniTask.Yield(cancellationToken);

            _spawner.SpawnRooms();
            Debug.Log("[BSP] SpawnRooms() done");

            _spawner.SpawnCorridors();
            Debug.Log("[BSP] SpawnCorridors() done");

            if (_generateWalls)
            {
                Debug.Log("[BSP] Walls -> GenerateWalls() start");
                _walls.GenerateWalls();
                Debug.Log("[BSP] Walls -> GenerateWalls() done");
                Debug.Log("[BSP] Generation terminer");
            }

            PlacePlayerRandom();

            Debug.Log($"[BSP Prefab Dongeon] Rooms={_runtimeContext.Rooms.Count} Corridors={_runtimeContext.CorridorGrid.Count}");
        }

        private void InitializeServices()
        {
            _runtimeContext ??= new DungeonRuntimeContext();
            _tree ??= new BspTreeBuilder(_dungeonWidth, _dungeonHeight, _minRoomSize, _maxIterations, RandomService);
            _rooms ??= new RoomPlacementService(_roomSize, _corridorSize, _runtimeContext);
            _corridors ??= new CorridorService(_corridorSize, _runtimeContext);
            _spawner ??= new PrefabSpawnService(
                _useDebugCubes,
                _showPivotDebug,
                _roomSize,
                _corridorSize,
                _roomDebugColor,
                _corridorDebugColor,
                _runtimeContext,
                GridGenerator.transform,
                Grid,
                _roomLeft, _roomRight, _roomTop, _roomBottom,
                _corridorHorizontal, _corridorVertical,
                _corridorCornerBR, _corridorCornerBL, _corridorCornerTR, _corridorCornerTL);

            _walls ??= new WallGenerationService(
                _runtimeContext,
                Grid,
                _roomSize,
                _corridorSize,
                GridGenerator,
                _wallPrefab);
        }

        private void ConnectLeafRooms()
        {
            ConnectRecursive(_runtimeContext.RootNode);
        }

        private void ConnectRecursive(BSPNode node)
        {
            if (node == null || node.IsLeaf()) return;

            if (node.left != null) ConnectRecursive(node.left);
            if (node.right != null) ConnectRecursive(node.right);

            if (node.left != null && node.right != null)
            {
                var leftRoom = _rooms.GetRandomRoomFromNode(node.left, RandomService);
                var rightRoom = _rooms.GetRandomRoomFromNode(node.right, RandomService);
                if (leftRoom != null && rightRoom != null)
                {
                    _corridors.CreateCorridor(leftRoom, rightRoom, RandomService);
                }
            }
        }

        //le spawn du player

        private void PlacePlayerRandom()
        {
            var target = _player;
            if (target == null)
            {
                var go = GameObject.FindGameObjectWithTag("Player");
                if (go != null) target = go.transform;
            }

            if (target == null)
            {
                Debug.LogWarning("[BSP] Pas de player assigné ni de tag Player dans la scène skip");
                return;
            }

            bool canUseRooms = _runtimeContext.Rooms.Count > 0;
            bool canUseCorridors = _runtimeContext.CorridorGrid.Count > 0;

            if (!canUseRooms && !canUseCorridors)
            {
                Debug.LogWarning("[BSP] Pas de place pour le player");
                return;
            }

            Vector3 spawnPos;

            if (canUseRooms && canUseCorridors)
            {
                bool pickRoom = RandomService.Range(0, 2) == 0;
                spawnPos = pickRoom ? GetRandomRoomCenterWorld() : GetRandomCorridorWorld();
            }
            else if (canUseRooms)
            {
                spawnPos = GetRandomRoomCenterWorld();
            }
            else
            {
                spawnPos = GetRandomCorridorWorld();
            }

            target.position = spawnPos;
        }

        private Vector3 GetRandomRoomCenterWorld()
        {
            int idx = RandomService.Range(0, _runtimeContext.Rooms.Count);
            var room = _runtimeContext.Rooms[idx];
            return new Vector3(
                room.floorCenter.x + Grid.OriginPosition.x,
                room.floorCenter.y + Grid.OriginPosition.y,
                0f);
        }

        private Vector3 GetRandomCorridorWorld()
        {
            int idx = RandomService.Range(0, _runtimeContext.CorridorGrid.Count);
            int i = 0;
            Vector2Int cellPos = default;
            foreach (var kv in _runtimeContext.CorridorGrid)
            {
                if (i == idx) { cellPos = kv.Key; break; }
                i++;
            }

            var world = DungeonGridUtility.CorridorCellToWorld(cellPos, _corridorSize);
            world.x += Grid.OriginPosition.x;
            world.y += Grid.OriginPosition.y;
            return world;
        }
    }
}