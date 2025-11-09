using UnityEngine;

[System.Serializable]
public class DungeonRoom
{
    public RectInt Bounds { get; private set; }
    public Vector2Int Center => Vector2Int.RoundToInt(Bounds.center);
    public GameObject Instance { get; set; }

    public DungeonRoom(RectInt bounds)
    {
        Bounds = bounds;
    }
}
