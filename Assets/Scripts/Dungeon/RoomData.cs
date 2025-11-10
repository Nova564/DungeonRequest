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
        int totalSize = 80;
        int wallThickness = 8;
        int centerX = bounds.x + totalSize / 2;
        int centerY = bounds.y + totalSize / 2;

        connections[RoomEntries.Left] = new ConnectionPoint(
            new Vector2Int(bounds.x + wallThickness, centerY), RoomEntries.Left);
        connections[RoomEntries.Right] = new ConnectionPoint(
            new Vector2Int(bounds.x + totalSize - wallThickness, centerY), RoomEntries.Right);
        connections[RoomEntries.Top] = new ConnectionPoint(
            new Vector2Int(centerX, bounds.y + totalSize - wallThickness), RoomEntries.Top);
        connections[RoomEntries.Bottom] = new ConnectionPoint(
            new Vector2Int(centerX, bounds.y + wallThickness), RoomEntries.Bottom);
    }

    public ConnectionPoint GetConnection(RoomEntries side)
    {
        return connections.ContainsKey(side) ? connections[side] : default;
    }
}