using UnityEngine;

namespace Components.ProceduralGeneration.BSP
{
    public static class DungeonGridUtility
    {
        public static int SnapToGrid(int value, int gridSize) =>
            Mathf.RoundToInt(value / (float)gridSize) * gridSize;

        public static Vector2 SnapToCorridor(Vector2 p, int corridorSize) =>
            new Vector2(
                SnapToGrid(Mathf.RoundToInt(p.x), corridorSize),
                SnapToGrid(Mathf.RoundToInt(p.y), corridorSize)
            );

        public static Vector2Int WorldToCorridorCell(Vector2 world, int corridorSize) =>
            new Vector2Int(
                Mathf.RoundToInt(world.x / corridorSize),
                Mathf.RoundToInt(world.y / corridorSize)
            );

        public static Vector3 CorridorCellToWorld(Vector2Int cell, int corridorSize) =>
            new Vector3(
                cell.x * corridorSize + corridorSize / 2f,
                cell.y * corridorSize + corridorSize / 2f,
                0);
    }
}