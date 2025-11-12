using UnityEngine;
using VTools.RandomService;

namespace Components.ProceduralGeneration.BSP
{
    public class CorridorService
    {
        private readonly int _corridorSize;
        private readonly DungeonRuntimeContext _ctx;

        public CorridorService(int corridorSize, DungeonRuntimeContext ctx)
        {
            _corridorSize = corridorSize;
            _ctx = ctx;
        }

        public void CreateCorridor(RoomData a, RoomData b, RandomService rnd)
        {
            Vector2 start = a.floorCenter;
            Vector2 end = b.floorCenter;
            float dx = end.x - start.x;
            float dy = end.y - start.y;

            RoomSide sideA, sideB;
            if (Mathf.Abs(dx) > Mathf.Abs(dy))
            {
                sideA = dx > 0 ? RoomSide.Right : RoomSide.Left;
                sideB = dx > 0 ? RoomSide.Left : RoomSide.Right;
            }
            else
            {
                sideA = dy > 0 ? RoomSide.Top : RoomSide.Bottom;
                sideB = dy > 0 ? RoomSide.Bottom : RoomSide.Top;
            }

            Vector2 startPoint = a.connectionPoints[sideA];
            Vector2 endPoint = b.connectionPoints[sideB];
            startPoint = DungeonGridUtility.SnapToCorridor(startPoint, _corridorSize);
            endPoint = DungeonGridUtility.SnapToCorridor(endPoint, _corridorSize);

            a.usedSides.Add(sideA);
            b.usedSides.Add(sideB);

            _ctx.ActiveConnections.Add(new ConnectionPoint { position = startPoint, side = sideA });
            _ctx.ActiveConnections.Add(new ConnectionPoint { position = endPoint, side = sideB });

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
                Vector2Int cell = DungeonGridUtility.WorldToCorridorCell(current, _corridorSize);
                if (!_ctx.CorridorGrid.ContainsKey(cell))
                    _ctx.CorridorGrid[cell] = new CorridorTile { cellPosition = cell };

                if (Mathf.Abs(direction.x) > 0.5f)
                    _ctx.CorridorGrid[cell].AddDirection(CorridorDirection.Horizontal);
                if (Mathf.Abs(direction.y) > 0.5f)
                    _ctx.CorridorGrid[cell].AddDirection(CorridorDirection.Vertical);

                current += direction * _corridorSize;
                if (Vector2.Distance(from, current) > 1000f) break;
            }
        }
    }
}