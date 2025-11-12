using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VTools.RandomService;

namespace Components.ProceduralGeneration.BSP
{
    public class RoomPlacementService
    {
        private readonly BspDungeonParameters _p;
        private readonly DungeonRuntimeContext _ctx;

        public RoomPlacementService(BspDungeonParameters p, DungeonRuntimeContext ctx)
        {
            _p = p;
            _ctx = ctx;
        }

        public void CreateRooms(BSPNode node)
        {
            if (node.IsLeaf())
            {
                var b = node.bounds;
                int centerX = b.xMin + b.width / 2;
                int centerY = b.yMin + b.height / 2;

                int roomX = DungeonGridUtility.SnapToGrid(centerX - _p.roomSize / 2, _p.roomSize);
                int roomY = DungeonGridUtility.SnapToGrid(centerY - _p.roomSize / 2, _p.roomSize);

                var roomBounds = new RectInt(roomX, roomY, _p.roomSize, _p.roomSize);
                var roomCenter = new Vector2(roomX + _p.roomSize / 2f, roomY + _p.roomSize / 2f);

                var room = new RoomData
                {
                    bounds = roomBounds,
                    floorCenter = roomCenter
                };

                room.connectionPoints[RoomSide.Left] = new Vector2(roomX, roomCenter.y);
                room.connectionPoints[RoomSide.Right] = new Vector2(roomX + _p.roomSize, roomCenter.y);
                room.connectionPoints[RoomSide.Top] = new Vector2(roomCenter.x, roomY + _p.roomSize);
                room.connectionPoints[RoomSide.Bottom] = new Vector2(roomCenter.x, roomY);

                node.room = roomBounds;
                _ctx.Rooms.Add(room);
            }
            else
            {
                if (node.left != null) CreateRooms(node.left);
                if (node.right != null) CreateRooms(node.right);
            }
        }

        public RoomData GetRandomRoomFromNode(BSPNode node, RandomService rnd)
        {
            if (node.IsLeaf() && node.room.HasValue)
                return _ctx.Rooms.FirstOrDefault(r => r.bounds == node.room.Value);

            var nodeRooms = new List<RoomData>();
            CollectRooms(node, nodeRooms);
            return nodeRooms.Count > 0 ? nodeRooms[rnd.Range(0, nodeRooms.Count)] : null;
        }

        private void CollectRooms(BSPNode node, List<RoomData> list)
        {
            if (node.IsLeaf() && node.room.HasValue)
            {
                var room = _ctx.Rooms.FirstOrDefault(r => r.bounds == node.room.Value);
                if (room != null) list.Add(room);
            }
            else
            {
                if (node.left != null) CollectRooms(node.left, list);
                if (node.right != null) CollectRooms(node.right, list);
            }
        }

        public void DetermineRoomEntries()
        {
            foreach (var room in _ctx.Rooms)
            {
                room.activeEntry = RoomSide.None;
                RoomSide[] priorities = { RoomSide.Left, RoomSide.Right, RoomSide.Top, RoomSide.Bottom };
                foreach (var side in priorities)
                {
                    if (room.usedSides.Contains(side))
                    {
                        Vector2 cp = room.connectionPoints[side];
                        Vector2Int cell = DungeonGridUtility.WorldToCorridorCell(cp, _p.corridorSize);
                        if (_ctx.CorridorGrid.ContainsKey(cell) || HasCorridorNearby(cp))
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
            Vector2Int c = DungeonGridUtility.WorldToCorridorCell(point, _p.corridorSize);
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                    if (_ctx.CorridorGrid.ContainsKey(c + new Vector2Int(dx, dy)))
                        return true;
            return false;
        }
    }
}