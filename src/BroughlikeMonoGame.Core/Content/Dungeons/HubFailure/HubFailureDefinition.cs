namespace BroughlikeMonoGame.Core;

public static class HubFailureDefinition
{
    public static DungeonDefinition Create()
        => new(
            "hub-failure",
            "Hub Failure",
            [
                new FloorDefinition(
                    "hub-failure-floor-1",
                    "Hub Failure",
                    new FixedLevelSource(
                        [
                            "#########",
                            "#.......#",
                            "#...@...#",
                            "#.......#",
                            "#.......#",
                            "#.......#",
                            "#.......#",
                            "#......>#",
                            "#########"
                        ]),
                    new SpawnProfile(999, 0, 0, [new WeightedEntry<MonsterKind>(MonsterKind.Bird, 1)]))
            ]);
}
