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
        [Header("Parameters (SO)")]
        [SerializeField] private BspDungeonParameters _parameters;

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

        private DungeonRuntimeContext _ctx;
        private BspTreeBuilder _tree;
        private RoomPlacementService _rooms;
        private CorridorService _corridors;
        private PrefabSpawnService _spawner;

        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            if (_parameters == null)
            {
                Debug.LogError("[BSP] Missing parameters ScriptableObject.");
                return;
            }

            InitializeServices();

            _ctx.Clear();

            _ctx.RootNode = _tree.BuildRoot(Grid.Width, Grid.Lenght);
            _tree.SplitRecursive(_ctx.RootNode, 0);

            _rooms.CreateRooms(_ctx.RootNode);

            ConnectLeafRooms();

            _rooms.DetermineRoomEntries();

            await UniTask.Yield(cancellationToken);

            _spawner.SpawnRooms();
            _spawner.SpawnCorridors();

            Debug.Log($"[BSP Prefab Dungeon] Rooms={_ctx.Rooms.Count} Corridors={_ctx.CorridorGrid.Count}");
        }

        private void InitializeServices()
        {
            _ctx ??= new DungeonRuntimeContext();
            _tree ??= new BspTreeBuilder(_parameters, RandomService);
            _rooms ??= new RoomPlacementService(_parameters, _ctx);
            _corridors ??= new CorridorService(_parameters, _ctx);
            _spawner ??= new PrefabSpawnService(
                _parameters,
                _ctx,
                GridGenerator.transform,
                _roomLeft, _roomRight, _roomTop, _roomBottom,
                _corridorHorizontal, _corridorVertical,
                _corridorCornerBR, _corridorCornerBL, _corridorCornerTR, _corridorCornerTL);
        }

        private void ConnectLeafRooms()
        {
            ConnectRecursive(_ctx.RootNode);
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