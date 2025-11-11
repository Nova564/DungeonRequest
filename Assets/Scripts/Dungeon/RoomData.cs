using System.Collections.Generic;
using UnityEngine;

public enum RoomSide
{
    None = 0,
    Left = 1,
    Right = 2,
    Top = 4,
    Bottom = 8
}

public class RoomData
{
    public RectInt bounds;
    public Vector2 floorCenter;
    public Dictionary<RoomSide, Vector2> connectionPoints = new Dictionary<RoomSide, Vector2>();
    public List<RoomSide> usedSides = new List<RoomSide>();
    public RoomSide activeEntry = RoomSide.None;
}

public struct ConnectionPoint
{
    public Vector2 position;
    public RoomSide side;
}
