using System.Collections.Generic;

namespace BroughlikeMonoGame.Core;

public static class ToolkitSampleFloors
{
    public static IReadOnlyList<FloorDefinition> Create()
        =>
        [
            new FloorDefinition(
                "toolkit-sample-hub",
                "Sample Antechamber",
                new FixedLevelSource(
                    [
                        "#########",
                        "#.......#",
                        "#..#....#",
                        "#..#.@..#",
                        "#..#....#",
                        "#.......#",
                        "#.....>.#",
                        "#########",
                        "#########"
                    ],
                    worldObjects:
                    [
                        new WorldObjectPlacement(
                            new WorldObjectDefinition(
                                WorldObjectDefinitionKind.ScriptedInteractable,
                                ItemId: "power",
                                DisplayName: "Supply Crate",
                                Message: "Inside: one POWER charm and a note saying to test the content seams before adding drama.",
                                VisualKind: WorldObjectVisualKind.SideTable,
                                BlocksMovement: true,
                                SpawnItemOffset: new Point2(1, 0)),
                            new Point2(2, 3)),
                        new WorldObjectPlacement(
                            new WorldObjectDefinition(
                                WorldObjectDefinitionKind.Portal,
                                PortalDestination: new PortalDestination("tutorial", 1),
                                DisplayName: "Training Gate",
                                VisualKind: WorldObjectVisualKind.Npc),
                            new Point2(1, 5))
                    ]),
                new SpawnProfile(999, 0, 0, [new WeightedEntry<MonsterKind>(MonsterKind.Bird, 1)]),
                new ExitDefinition(
                [
                    new ExitRoute(new PortalDestination("toolkit-sample", 2, "Enter the sample dungeon"))
                ])),
            new FloorDefinition(
                "toolkit-sample-dungeon-1",
                "Sample Depth 1",
                new ProceduralLevelSource(),
                new SpawnProfile(
                    initialSpawnRate: 14,
                    initialMonsterCount: 3,
                    initialTreasureCount: 2,
                    monsterTable:
                    [
                        new WeightedEntry<MonsterKind>(MonsterKind.Bird, 2),
                        new WeightedEntry<MonsterKind>(MonsterKind.Jester, 1)
                    ],
                    initialFloorItemCount: 1,
                    initialEnemyItemDropCount: 1,
                    itemTable:
                    [
                        new WeightedEntry<string>("power", 2),
                        new WeightedEntry<string>("bolt", 1)
                    ]),
                new ExitDefinition(
                [
                    new ExitRoute(new PortalDestination("hub-success", 1, "Return to hub-success"), RequiredItemId: "power", Label: "Return carrying POWER", SetsCurrentStart: true),
                    new ExitRoute(new PortalDestination("hub-failure", 1, "Return to hub-failure"), Label: "Return empty-handed", SetsCurrentStart: true)
                ]))
        ];
}
