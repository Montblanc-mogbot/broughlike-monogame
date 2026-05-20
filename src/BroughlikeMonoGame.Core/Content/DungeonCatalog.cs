using System.Collections.Generic;

namespace BroughlikeMonoGame.Core;

public static class DungeonCatalog
{
    public static DungeonDefinition CreateTutorialDungeon()
    {
        var spawnProfile = new SpawnProfile(
            GameConstants.InitialSpawnRate,
            initialMonsterCount: 2,
            initialTreasureCount: 3,
            monsterTable:
            [
                new WeightedEntry<MonsterKind>(MonsterKind.Bird, 3),
                new WeightedEntry<MonsterKind>(MonsterKind.Snake, 3),
                new WeightedEntry<MonsterKind>(MonsterKind.Tank, 2),
                new WeightedEntry<MonsterKind>(MonsterKind.Eater, 2),
                new WeightedEntry<MonsterKind>(MonsterKind.Jester, 1),
            ]);

        var floors = new List<FloorDefinition>();
        for (var level = 1; level <= GameConstants.NumberOfLevels; level++)
        {
            floors.Add(new FloorDefinition(
                $"tutorial-floor-{level}",
                $"Tutorial Floor {level}",
                new ProceduralLevelSource(),
                spawnProfile));
        }

        return new DungeonDefinition("tutorial", "Tutorial Dungeon", floors);
    }
}
