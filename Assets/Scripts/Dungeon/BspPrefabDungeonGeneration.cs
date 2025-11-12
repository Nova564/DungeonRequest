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
            _spawner.SpawnCorridors();

            Debug.Log($"[BSP Prefab Dungeon] Rooms={_runtimeContext.Rooms.Count} Corridors={_runtimeContext.CorridorGrid.Count}");
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
                _roomLeft, _roomRight, _roomTop, _roomBottom,
                _corridorHorizontal, _corridorVertical,
                _corridorCornerBR, _corridorCornerBL, _corridorCornerTR, _corridorCornerTL);
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
    }
}