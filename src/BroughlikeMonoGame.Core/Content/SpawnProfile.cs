using System;
using System.Collections.Generic;
using System.Linq;

namespace BroughlikeMonoGame.Core;

public sealed class SpawnProfile
{
    private readonly IReadOnlyList<WeightedEntry<MonsterKind>> _monsterTable;
    private readonly int _totalWeight;

    public SpawnProfile(int initialSpawnRate, int initialMonsterCount, int initialTreasureCount, IReadOnlyList<WeightedEntry<MonsterKind>> monsterTable)
    {
        if (monsterTable.Count == 0)
        {
            throw new ArgumentException("Spawn tables must include at least one monster entry.", nameof(monsterTable));
        }

        InitialSpawnRate = initialSpawnRate;
        InitialMonsterCount = initialMonsterCount;
        InitialTreasureCount = initialTreasureCount;
        _monsterTable = monsterTable;
        _totalWeight = monsterTable.Sum(entry => Math.Max(0, entry.Weight));
        if (_totalWeight <= 0)
        {
            throw new ArgumentException("Spawn table weights must total more than zero.", nameof(monsterTable));
        }
    }

    public int InitialSpawnRate { get; }

    public int InitialMonsterCount { get; }

    public int InitialTreasureCount { get; }

    public MonsterKind PickRandomMonster(Random random)
    {
        var roll = random.Next(1, _totalWeight + 1);
        var running = 0;
        foreach (var entry in _monsterTable)
        {
            running += Math.Max(0, entry.Weight);
            if (roll <= running)
            {
                return entry.Value;
            }
        }

        return _monsterTable[^1].Value;
    }
}
