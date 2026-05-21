namespace BroughlikeMonoGame.Core;

public static class HubSuccessDefinition
{
    public static DungeonDefinition Create()
        => new(
            "hub-success",
            "Court Offices",
            [
                new FloorDefinition(
                    "hub-success-floor-1",
                    "Records Stair",
                    new FixedLevelSource(
                        [
                            "#########",
                            "#.......#",
                            "#..@....#",
                            "#.......#",
                            "#.......#",
                            "#.......#",
                            "#......>#",
                            "#########",
                            "#########"
                        ],
                        worldObjects:
                        [
                            new WorldObjectPlacement(
                                new WorldObjectDefinition(
                                    WorldObjectDefinitionKind.ScriptedInteractable,
                                    DisplayName: "Clerk",
                                    Message: "Yes, that's the right stamp. No, that does not help you.",
                                    VisualKind: WorldObjectVisualKind.Npc,
                                    BlocksMovement: true),
                                new Point2(2, 2)),
                            new WorldObjectPlacement(
                                new WorldObjectDefinition(
                                    WorldObjectDefinitionKind.ScriptedInteractable,
                                    DisplayName: "Key Rack",
                                    Message: "Most hooks are empty. One label reads: ACCESS GRANTED RETROACTIVELY.",
                                    VisualKind: WorldObjectVisualKind.Dresser,
                                    BlocksMovement: true),
                                new Point2(5, 2)),
                            new WorldObjectPlacement(
                                new WorldObjectDefinition(
                                    WorldObjectDefinitionKind.ScriptedInteractable,
                                    DisplayName: "Stamped Notice",
                                    Message: "You may proceed because you already did.",
                                    VisualKind: WorldObjectVisualKind.SideTable,
                                    BlocksMovement: true),
                                new Point2(3, 5))
                        ]),
                    new SpawnProfile(999, 0, 0, [new WeightedEntry<MonsterKind>(MonsterKind.Bird, 1)]),
                    new ExitDefinition(
                    [
                        new ExitRoute(new PortalDestination("tutorial", 1, "Re-enter the offices"), Label: "Re-enter the offices")
                    ]))
            ]);
}
