using UnityEngine;
using System.Collections.Generic;

public class RoomData
{
    public RectInt bounds;
    public Vector2Int floorCenter;
    public Dictionary<RoomEntries, ConnectionPoint> connections;

    public RoomData(RectInt bounds)
    {
        this.bounds = bounds;
        this.floorCenter = new Vector2Int(bounds.x + bounds.width / 2, bounds.y + bounds.height / 2);
        this.connections = new Dictionary<RoomEntries, ConnectionPoint>();
        CalculateConnectionPoints();
    }

    private void CalculateConnectionPoints()
    {
        connections.Clear();

        int centerX = bounds.x + bounds.width / 2;
        int centerY = bounds.y + bounds.height / 2;

        connections[RoomEntries.Left] = new ConnectionPoint(
            new Vector2Int(bounds.x, centerY), RoomEntries.Left);

        connections[RoomEntries.Right] = new ConnectionPoint(
            new Vector2Int(bounds.x + bounds.width, centerY), RoomEntries.Right);

        connections[RoomEntries.Top] = new ConnectionPoint(
            new Vector2Int(centerX, bounds.y + bounds.height), RoomEntries.Top);

        connections[RoomEntries.Bottom] = new ConnectionPoint(
            new Vector2Int(centerX, bounds.y), RoomEntries.Bottom);
    }

    public ConnectionPoint GetConnection(RoomEntries side)
    {
        return connections.ContainsKey(side) ? connections[side] : default;
    }
}