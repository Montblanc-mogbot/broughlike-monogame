using System.Collections.Generic;

namespace BroughlikeMonoGame.Core;

public static class TutorialDungeonSpawnProfiles
{
    public static SpawnProfile CreateMainProfile()
        => new(
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
            ],
            initialFloorItemCount: 2,
            initialEnemyItemDropCount: 1,
            itemTable:
            [
                new WeightedEntry<string>("woop", 2),
                new WeightedEntry<string>("power", 2),
                new WeightedEntry<string>("aura", 2),
                new WeightedEntry<string>("dig", 1),
                new WeightedEntry<string>("dash", 1),
                new WeightedEntry<string>("bubble", 1),
            ]);
}
