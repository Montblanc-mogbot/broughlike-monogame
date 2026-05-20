using System;
using System.Collections.Generic;

namespace BroughlikeMonoGame.Core;

public sealed class DungeonRegistry
{
    private readonly Dictionary<string, DungeonDefinition> _dungeons;

    public DungeonRegistry(IReadOnlyList<DungeonDefinition> dungeons)
    {
        _dungeons = new Dictionary<string, DungeonDefinition>(StringComparer.OrdinalIgnoreCase);
        foreach (var dungeon in dungeons)
        {
            _dungeons[dungeon.Id] = dungeon;
        }

        if (_dungeons.Count == 0)
        {
            throw new ArgumentException("Dungeon registry requires at least one dungeon.", nameof(dungeons));
        }
    }

    public DungeonDefinition Get(string dungeonId)
    {
        if (!_dungeons.TryGetValue(dungeonId, out var dungeon))
        {
            throw new InvalidOperationException($"Unknown dungeon id '{dungeonId}'.");
        }

        return dungeon;
    }
}
