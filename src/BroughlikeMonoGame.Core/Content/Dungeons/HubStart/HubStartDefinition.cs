namespace BroughlikeMonoGame.Core;

public static class HubStartDefinition
{
    public static DungeonDefinition Create()
        => new(
            "hub-start",
            "Hub Start",
            [
                new FloorDefinition(
                    "hub-start-floor-1",
                    "Hub Start",
                    new FixedLevelSource(
                        [
                            "#########",
                            "#@>.....#",
                            "#.......#",
                            "#.......#",
                            "#.......#",
                            "#.......#",
                            "#.......#",
                            "#.......#",
                            "#########"
                        ]),
                    new SpawnProfile(999, 0, 0, [new WeightedEntry<MonsterKind>(MonsterKind.Bird, 1)]),
                    new ExitDefinition(
                    [
                        new ExitRoute(new PortalDestination("tutorial", 1, "Enter the dungeon"), Label: "Enter the dungeon")
                    ]))
            ]);
}
