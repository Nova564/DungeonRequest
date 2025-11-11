using UnityEngine;

[System.Flags]
public enum CorridorDirection
{
    None = 0,
    Horizontal = 1,
    Vertical = 2
}

public class CorridorTile
{
    public Vector2Int cellPosition;
    public CorridorDirection directions = CorridorDirection.None;

    public void AddDirection(CorridorDirection dir)
    {
        directions |= dir;
    }

    public CorridorTileType GetTileType()
    {
        if (directions == CorridorDirection.Horizontal)
            return CorridorTileType.Horizontal;

        if (directions == CorridorDirection.Vertical)
            return CorridorTileType.Vertical;

        //detection de corner simplifier pour l'instant
        if ((directions & CorridorDirection.Horizontal) != 0 &&
            (directions & CorridorDirection.Vertical) != 0)
        {
            // logique pour les corner bas droit à améliorer
            return CorridorTileType.CornerBottomRight;
        }

        return CorridorTileType.Horizontal;
    }
}

public enum CorridorTileType
{
    Horizontal,
    Vertical,
    CornerBottomRight,
    CornerBottomLeft,
    CornerTopRight,
    CornerTopLeft
}