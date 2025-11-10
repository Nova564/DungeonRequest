using UnityEngine;
using System.Collections.Generic;

public enum CorridorTileType
{
    Horizontal,
    Vertical,
    CornerBottomRight, 
    CornerBottomLeft,   
    CornerTopRight,     
    CornerTopLeft,      
}

public class CorridorTile
{
    public Vector2Int position;
    public CorridorTileType type;
    public HashSet<Vector2Int> directions;

    public CorridorTile(Vector2Int pos)
    {
        this.position = pos;
        this.directions = new HashSet<Vector2Int>();
        this.type = CorridorTileType.Horizontal;
    }

    public void AddDirection(Vector2Int dir)
    {
        directions.Add(dir);
        UpdateType();
    }

    private void UpdateType()
    {
        int count = directions.Count;

        if (count == 1)
        {
            if (directions.Contains(Vector2Int.right) || directions.Contains(Vector2Int.left))
                type = CorridorTileType.Horizontal;
            else
                type = CorridorTileType.Vertical;
        }
        else if (count == 2)
        {
            bool hasLeft = directions.Contains(Vector2Int.left);
            bool hasRight = directions.Contains(Vector2Int.right);
            bool hasUp = directions.Contains(Vector2Int.up);
            bool hasDown = directions.Contains(Vector2Int.down);

            if (hasLeft && hasRight)
                type = CorridorTileType.Horizontal;
            else if (hasUp && hasDown)
                type = CorridorTileType.Vertical;
            else if (hasRight && hasUp)
                type = CorridorTileType.CornerBottomLeft;
            else if (hasLeft && hasUp)
                type = CorridorTileType.CornerBottomRight;
            else if (hasRight && hasDown)
                type = CorridorTileType.CornerTopLeft;
            else if (hasLeft && hasDown)
                type = CorridorTileType.CornerTopRight;
        }
    }
}