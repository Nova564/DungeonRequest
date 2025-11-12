using UnityEngine;

namespace Components.ProceduralGeneration.BSP
{
    [CreateAssetMenu(menuName = "Procedural Generation Config/BSP Dungeon Parameters")]
    public class BspDungeonParameters : ScriptableObject
    {
        [Header("Dungeon Bounds")]
        public int dungeonWidth = 100;
        public int dungeonHeight = 100;

        [Header("BSP")]
        public int minRoomSize = 15;
        public int maxIterations = 4;

        [Header("Rooms")]
        public int roomSize = 5;

        [Header("Corridors")]
        public int corridorSize = 1;

        [Header("Debug")]
        public bool useDebugCubes = true;
        public bool showPivotDebug = false;
        public Color roomDebugColor = new Color(0.2f, 0.9f, 0.2f, 0.6f);
        public Color corridorDebugColor = new Color(0.2f, 0.6f, 1f, 0.6f);
    }
}