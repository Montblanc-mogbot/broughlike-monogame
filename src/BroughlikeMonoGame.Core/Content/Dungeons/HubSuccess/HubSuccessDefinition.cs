namespace BroughlikeMonoGame.Core;

public static class HubSuccessDefinition
{
    public static DungeonDefinition Create()
        => new(
            "hub-success",
            "Hub Success",
            [
                new FloorDefinition(
                    "hub-success-floor-1",
                    "Hub Success",
                    new FixedLevelSource(
                        [
                            "#########",
                            "#.......#",
                            "#..@....#",
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
