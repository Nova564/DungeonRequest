using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon/Dungeon Config")]
public class DungeonConfig : ScriptableObject
{
    [Header("Grid Settings")]
    public int width = 100;
    public int height = 100;

    [Header("Room Settings")]
    public int minRoomSize = 6;
    public int maxRoomSize = 14;
    public int maxRooms = 12;

    [Header("Prefabs")]
    public GameObject roomPrefab;
    public GameObject corridorPrefab;
}
