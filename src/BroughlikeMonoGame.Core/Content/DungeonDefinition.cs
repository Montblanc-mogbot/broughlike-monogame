using System;
using System.Collections.Generic;

namespace BroughlikeMonoGame.Core;

public sealed class DungeonDefinition
{
    private readonly IReadOnlyList<FloorDefinition> _floors;

    public DungeonDefinition(string id, string displayName, IReadOnlyList<FloorDefinition> floors, bool seedsRandomStartingInventory = true)
    {
        if (floors.Count == 0)
        {
            throw new ArgumentException("Dungeon definitions require at least one floor.", nameof(floors));
        }

        Id = id;
        DisplayName = displayName;
        SeedsRandomStartingInventory = seedsRandomStartingInventory;
        _floors = floors;
    }

    public string Id { get; }

    public string DisplayName { get; }

    public bool SeedsRandomStartingInventory { get; }

    public int FloorCount => _floors.Count;

    public FloorDefinition GetFloor(int levelNumber)
    {
        if (levelNumber < 1 || levelNumber > _floors.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(levelNumber), $"Dungeon '{Id}' does not define floor {levelNumber}.");
        }

        return _floors[levelNumber - 1];
    }
}
