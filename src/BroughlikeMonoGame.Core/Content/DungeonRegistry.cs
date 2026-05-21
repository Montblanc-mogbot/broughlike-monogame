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
            if (string.IsNullOrWhiteSpace(dungeon.Id))
            {
                throw new ArgumentException("Dungeon ids must be non-empty.", nameof(dungeons));
            }

            if (!_dungeons.TryAdd(dungeon.Id, dungeon))
            {
                throw new ArgumentException($"Duplicate dungeon id '{dungeon.Id}'.", nameof(dungeons));
            }
        }

        if (_dungeons.Count == 0)
        {
            throw new ArgumentException("Dungeon registry requires at least one dungeon.", nameof(dungeons));
        }
    }

    public IEnumerable<DungeonDefinition> Dungeons => _dungeons.Values;

    public bool Contains(string dungeonId) => _dungeons.ContainsKey(dungeonId);

    public DungeonDefinition Get(string dungeonId)
    {
        if (!_dungeons.TryGetValue(dungeonId, out var dungeon))
        {
            throw new InvalidOperationException($"Unknown dungeon id '{dungeonId}'.");
        }

        return dungeon;
    }
}
