using UnityEngine;

[System.Serializable]
public class DungeonCorridor
{
    public Vector2Int Start { get; }
    public Vector2Int End { get; }
    public GameObject Instance { get; set; }

    public DungeonCorridor(Vector2Int start, Vector2Int end)
    {
        Start = start;
        End = end;
    }

    public Vector2Int GetDirection()
    {
        Vector2Int delta = End - Start;
        return new Vector2Int(
            delta.x == 0 ? 0 : (delta.x > 0 ? 1 : -1),
            delta.y == 0 ? 0 : (delta.y > 0 ? 1 : -1)
        );
    }

    public Vector3 GetWorldPosition(float z = 0)
    {
        return new Vector3(Start.x, Start.y, z);
    }
}
