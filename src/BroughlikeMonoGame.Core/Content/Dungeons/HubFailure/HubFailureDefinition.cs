namespace BroughlikeMonoGame.Core;

public static class HubFailureDefinition
{
    public static DungeonDefinition Create()
        => new(
            "hub-failure",
            "Court Offices",
            [
                new FloorDefinition(
                    "hub-failure-floor-1",
                    "Waiting Gallery",
                    new FixedLevelSource(
                        [
                            "#########",
                            "#.......#",
                            "#...@...#",
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
                                    DisplayName: "Bailiff",
                                    Message: "Nothing was filed. This, too, will be held against you.",
                                    VisualKind: WorldObjectVisualKind.Npc,
                                    BlocksMovement: true),
                                new Point2(2, 2)),
                            new WorldObjectPlacement(
                                new WorldObjectDefinition(
                                    WorldObjectDefinitionKind.ScriptedInteractable,
                                    DisplayName: "Vacant Hook",
                                    Message: "A card beneath it says POWER in a handwriting that avoids blame.",
                                    VisualKind: WorldObjectVisualKind.Dresser,
                                    BlocksMovement: true),
                                new Point2(5, 2)),
                            new WorldObjectPlacement(
                                new WorldObjectDefinition(
                                    WorldObjectDefinitionKind.ScriptedInteractable,
                                    DisplayName: "Bench",
                                    Message: "The bench is warm, as if your absence had been sitting here already.",
                                    VisualKind: WorldObjectVisualKind.Armchair,
                                    BlocksMovement: true),
                                new Point2(3, 5))
                        ]),
                    new SpawnProfile(999, 0, 0, [new WeightedEntry<MonsterKind>(MonsterKind.Bird, 1)]),
                    new ExitDefinition(
                    [
                        new ExitRoute(new PortalDestination("tutorial", 1, "Try again"), Label: "Try again")
                    ]))
            ]);
}
