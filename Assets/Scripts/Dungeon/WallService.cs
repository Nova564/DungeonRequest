using System.Collections.Generic;
using UnityEngine;
using VTools.Grid;

namespace Components.ProceduralGeneration.BSP
{
    public class WallGenerationService
    {
        private readonly DungeonRuntimeContext _ctx;
        private readonly VTools.Grid.Grid _grid;
        private readonly int _roomSize;
        private readonly GameObject _wallPrefab;
        private readonly int _corridorSize;
        private readonly ProceduralGridGenerator _gridGenerator;

        private HashSet<Vector2Int> _floorCells;
        private HashSet<Vector2Int> _wallCells;

        public WallGenerationService(
            DungeonRuntimeContext ctx,
            VTools.Grid.Grid grid,
            int roomSize,
            int corridorSize,
            ProceduralGridGenerator gridGenerator,
            GameObject wallPrefab)
        {
            _ctx = ctx;
            _grid = grid;
            _roomSize = roomSize;
            _corridorSize = corridorSize;
            _gridGenerator = gridGenerator;
            _wallPrefab = wallPrefab;

            _floorCells = new HashSet<Vector2Int>();
            _wallCells = new HashSet<Vector2Int>();
        }

        public void GenerateWalls()
        {
            CollectFloorCells();
            DetermineWallPositions();
            PlaceWalls();

            Debug.Log($"[WallGeneration] Generated {_wallCells.Count} wall tiles");
        }

        private void CollectFloorCells()
        {
            _floorCells.Clear();

            foreach (var room in _ctx.Rooms)
            {
                Vector2Int roomGridPos = WorldToGridCell(room.floorCenter, _roomSize);

                int cellsPerRoom = Mathf.CeilToInt(_roomSize / (float)_grid.CellSize);
                int halfCells = cellsPerRoom / 2;

                for (int x = -halfCells; x <= halfCells; x++)
                {
                    for (int y = -halfCells; y <= halfCells; y++)
                    {
                        Vector2Int cell = new Vector2Int(roomGridPos.x + x, roomGridPos.y + y);
                        if (IsWithinGridBounds(cell))
                        {
                            _floorCells.Add(cell);
                        }
                    }
                }
            }

            foreach (var kvp in _ctx.CorridorGrid)
            {
                Vector2 worldPos = DungeonGridUtility.CorridorCellToWorld(kvp.Key, _corridorSize);
                Vector2Int gridCell = WorldToGridCell(worldPos, _corridorSize);

                if (IsWithinGridBounds(gridCell))
                {
                    _floorCells.Add(gridCell);
                }
            }
        }

        private void DetermineWallPositions()
        {
            _wallCells.Clear();

            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(0, 1),
                new Vector2Int(1, 1),
                new Vector2Int(1, 0),
                new Vector2Int(1, -1),
                new Vector2Int(0, -1),
                new Vector2Int(-1, -1),
                new Vector2Int(-1, 0),
                new Vector2Int(-1, 1)
            };

            foreach (var floorCell in _floorCells)
            {
                foreach (var dir in directions)
                {
                    Vector2Int neighborCell = floorCell + dir;
                    if (!_floorCells.Contains(neighborCell) && IsWithinGridBounds(neighborCell))
                    {
                        _wallCells.Add(neighborCell);
                    }
                }
            }
        }

        private void PlaceWalls()
        {
            for (int x = 0; x < _grid.Width; x++)
            {
                for (int y = 0; y < _grid.Lenght; y++)
                {
                    var cellPos = new Vector2Int(x, y);
                    if (_floorCells.Contains(cellPos)) continue;

                    if (_grid.TryGetCellByCoordinates(x, y, out var cell))
                    {
                        AddWallToCell(cell);
                    }
                }
            }
        }

        private void AddWallToCell(Cell cell)
        {
            if (_wallPrefab == null)
            {
                Debug.LogWarning("[WallGeneration] Wall prefab not assigned!");
                return;
            }
            var c = cell.Coordinates;
            float half = _grid.CellSize * 0.5f;
            var pos = new Vector3(
                _grid.OriginPosition.x + c.x * _grid.CellSize + half,
                _grid.OriginPosition.y + c.y * _grid.CellSize + half,
                0f);

            var go = Object.Instantiate(_wallPrefab, pos, Quaternion.identity);
            go.transform.SetParent(_gridGenerator.transform, true);
        }

        private Vector2Int WorldToGridCell(Vector2 worldPos, float size)
        {
            int x = Mathf.FloorToInt((worldPos.x - _grid.OriginPosition.x) / _grid.CellSize);
            int y = Mathf.FloorToInt((worldPos.y - _grid.OriginPosition.y) / _grid.CellSize);
            return new Vector2Int(x, y);
        }

        private bool IsWithinGridBounds(Vector2Int cell)
        {
            return cell.x >= 0 && cell.x < _grid.Width &&
                   cell.y >= 0 && cell.y < _grid.Lenght;
        }

        public void Clear()
        {
            _floorCells?.Clear();
            _wallCells?.Clear();
        }
    }
}