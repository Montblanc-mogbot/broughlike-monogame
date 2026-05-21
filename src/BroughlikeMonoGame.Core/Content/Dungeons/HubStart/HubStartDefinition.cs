namespace BroughlikeMonoGame.Core;

public static class HubStartDefinition
{
    public static DungeonDefinition Create()
        => new(
            "hub-start",
            "Court Offices",
            [
                new FloorDefinition(
                    "hub-start-floor-1",
                    "First Antechamber",
                    new FixedLevelSource(
                        [
                            "#########",
                            "#.......#",
                            "#.......#",
                            "#..@....#",
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
                                    DisplayName: "Usher",
                                    Message: "The inquiry is below. Or above. Continue until someone stops you.",
                                    VisualKind: WorldObjectVisualKind.Npc,
                                    BlocksMovement: true),
                                new Point2(2, 2)),
                            new WorldObjectPlacement(
                                new WorldObjectDefinition(
                                    WorldObjectDefinitionKind.ScriptedInteractable,
                                    DisplayName: "Bench",
                                    Message: null,
                                    VisualKind: WorldObjectVisualKind.Armchair,
                                    BlocksMovement: true),
                                new Point2(5, 2)),
                            new WorldObjectPlacement(
                                new WorldObjectDefinition(
                                    WorldObjectDefinitionKind.ScriptedInteractable,
                                    DisplayName: "Notice Board",
                                    Message: "No case numbers are listed. Only names, and not yours.",
                                    VisualKind: WorldObjectVisualKind.SideTable,
                                    BlocksMovement: true),
                                new Point2(2, 5))
                        ]),
                    new SpawnProfile(999, 0, 0, [new WeightedEntry<MonsterKind>(MonsterKind.Bird, 1)]),
                    new ExitDefinition(
                    [
                        new ExitRoute(new PortalDestination("tutorial", 1, "Descend into the offices"), Label: "Descend into the offices")
                    ]))
            ]);
}
