using System.Collections.Generic;
using UnityEngine;

namespace Components.ProceduralGeneration.BSP
{
    public class DungeonRuntimeContext
    {
        public BSPNode RootNode;
        public readonly List<RoomData> Rooms = new();
        public readonly Dictionary<Vector2Int, CorridorTile> CorridorGrid = new();
        public readonly List<GameObject> SpawnedObjects = new();
        public readonly List<ConnectionPoint> ActiveConnections = new();

        public void Clear()
        {
            foreach (var o in SpawnedObjects)
            {
                if (o) Object.Destroy(o);
            }
            SpawnedObjects.Clear();
            Rooms.Clear();
            CorridorGrid.Clear();
            ActiveConnections.Clear();
            RootNode = null;
        }
    }
}