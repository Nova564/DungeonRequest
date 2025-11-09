using System.Collections.Generic;

public class DungeonLayout
{
    public List<DungeonRoom> Rooms { get; } = new();
    public List<DungeonCorridor> Corridors { get; } = new();
}
