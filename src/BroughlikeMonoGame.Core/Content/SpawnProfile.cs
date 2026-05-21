using System;
using System.Collections.Generic;
using System.Linq;

namespace BroughlikeMonoGame.Core;

public sealed class SpawnProfile
{
    private readonly IReadOnlyList<WeightedEntry<MonsterKind>> _monsterTable;
    private readonly IReadOnlyList<WeightedEntry<string>> _itemTable;
    private readonly int _totalMonsterWeight;
    private readonly int _totalItemWeight;

    public SpawnProfile(
        int initialSpawnRate,
        int initialMonsterCount,
        int initialTreasureCount,
        IReadOnlyList<WeightedEntry<MonsterKind>> monsterTable,
        int initialFloorItemCount = 0,
        int initialEnemyItemDropCount = 0,
        IReadOnlyList<WeightedEntry<string>>? itemTable = null)
    {
        if (monsterTable.Count == 0)
        {
            throw new ArgumentException("Spawn tables must include at least one monster entry.", nameof(monsterTable));
        }

        InitialSpawnRate = initialSpawnRate;
        InitialMonsterCount = initialMonsterCount;
        InitialTreasureCount = initialTreasureCount;
        InitialFloorItemCount = initialFloorItemCount;
        InitialEnemyItemDropCount = initialEnemyItemDropCount;
        _monsterTable = monsterTable;
        _itemTable = itemTable ?? [];
        _totalMonsterWeight = monsterTable.Sum(entry => Math.Max(0, entry.Weight));
        _totalItemWeight = _itemTable.Sum(entry => Math.Max(0, entry.Weight));
        if (_totalMonsterWeight <= 0)
        {
            throw new ArgumentException("Spawn table weights must total more than zero.", nameof(monsterTable));
        }
    }

    public int InitialSpawnRate { get; }

    public int InitialMonsterCount { get; }

    public int InitialTreasureCount { get; }

    public int InitialFloorItemCount { get; }

    public int InitialEnemyItemDropCount { get; }

    public IReadOnlyList<WeightedEntry<MonsterKind>> MonsterTable => _monsterTable;

    public IReadOnlyList<WeightedEntry<string>> ItemTable => _itemTable;

    public MonsterKind PickRandomMonster(Random random)
    {
        var roll = random.Next(1, _totalMonsterWeight + 1);
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

    public string PickRandomItemId(Random random)
    {
        if (_itemTable.Count == 0 || _totalItemWeight <= 0)
        {
            throw new InvalidOperationException("No item table is configured for this spawn profile.");
        }

        var roll = random.Next(1, _totalItemWeight + 1);
        var running = 0;
        foreach (var entry in _itemTable)
        {
            running += Math.Max(0, entry.Weight);
            if (roll <= running)
            {
                return entry.Value;
            }
        }

        return _itemTable[^1].Value;
    }
}
