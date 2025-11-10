using UnityEngine;

[System.Flags]
public enum RoomEntries
{
    None = 0,
    Left = 1,
    Right = 2,
    Top = 4,
    Bottom = 8
}

public struct ConnectionPoint
{
    public Vector2Int position;
    public RoomEntries side;

    public ConnectionPoint(Vector2Int pos, RoomEntries side)
    {
        this.position = pos;
        this.side = side;
    }
}