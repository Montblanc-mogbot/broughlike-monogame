using System.Collections.Generic;

namespace BroughlikeMonoGame.Core;

public static class ApartmentIntroDefinition
{
    public static DungeonDefinition Create()
        => new(
            "apartment-intro",
            "Apartment Building",
            CreateFloors(),
            seedsRandomStartingInventory: false);

    private static IReadOnlyList<FloorDefinition> CreateFloors()
        =>
        [
            new FloorDefinition(
                "apartment-bedroom",
                "Bedroom",
                new FixedLevelSource(
                    [
                        "#########",
                        "#.......#",
                        "#.>.....#",
                        "#.......#",
                        "#...@...#",
                        "#.......#",
                        "#.......#",
                        "#########",
                        "#########"
                    ],
                    worldObjects:
                    [
                        new WorldObjectPlacement(
                            new WorldObjectDefinition(
                                WorldObjectDefinitionKind.ScriptedInteractable,
                                DisplayName: "Bed",
                                Message: null,
                                VisualKind: WorldObjectVisualKind.Bed,
                                BlocksMovement: true),
                            new Point2(2, 1)),
                        new WorldObjectPlacement(
                            new WorldObjectDefinition(
                                WorldObjectDefinitionKind.ScriptedInteractable,
                                ItemId: "black-suit",
                                DisplayName: "Dresser",
                                Message: "Inside the dresser: a black suit, waiting as if it had expected you.",
                                VisualKind: WorldObjectVisualKind.Dresser,
                                BlocksMovement: true,
                                SpawnItemOffset: new Point2(1, 0)),
                            new Point2(5, 1)),
                        new WorldObjectPlacement(
                            new WorldObjectDefinition(
                                WorldObjectDefinitionKind.ScriptedInteractable,
                                DisplayName: "Stranger",
                                Message: "You have been arrested",
                                VisualKind: WorldObjectVisualKind.Npc,
                                BlocksMovement: true),
                            new Point2(3, 4))
                    ]),
                new SpawnProfile(999, 0, 0, [new WeightedEntry<MonsterKind>(MonsterKind.Bird, 1)]),
                new ExitDefinition(
                [
                    new ExitRoute(new PortalDestination("apartment-intro", 2, null))
                ])),
            new FloorDefinition(
                "apartment-living-room",
                "Living Room",
                new FixedLevelSource(
                    [
                        "#########",
                        "#.......#",
                        "#.......#",
                        "#..@....#",
                        "#.......#",
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
                                DisplayName: "Armchair",
                                Message: null,
                                VisualKind: WorldObjectVisualKind.Bed,
                                BlocksMovement: true),
                            new Point2(2, 1)),
                        new WorldObjectPlacement(
                            new WorldObjectDefinition(
                                WorldObjectDefinitionKind.ScriptedInteractable,
                                DisplayName: "Side Table",
                                Message: null,
                                VisualKind: WorldObjectVisualKind.Dresser,
                                BlocksMovement: true),
                            new Point2(5, 2))
                    ]),
                new SpawnProfile(999, 0, 0, [new WeightedEntry<MonsterKind>(MonsterKind.Bird, 1)]),
                new ExitDefinition(
                [
                    new ExitRoute(new PortalDestination("apartment-intro", 3, null))
                ])),
            new FloorDefinition(
                "apartment-hallway",
                "Hallway",
                new FixedLevelSource(
                    [
                        "#########",
                        "#########",
                        "#@.....>#",
                        "#########",
                        "#########",
                        "#########",
                        "#########",
                        "#########",
                        "#########"
                    ]),
                new SpawnProfile(999, 0, 0, [new WeightedEntry<MonsterKind>(MonsterKind.Bird, 1)]),
                new ExitDefinition(
                [
                    new ExitRoute(new PortalDestination("apartment-intro", 4, null))
                ])),
            new FloorDefinition(
                "apartment-meeting-room",
                "Fraulein Burstner's Room",
                new FixedLevelSource(
                    [
                        "#########",
                        "#.......#",
                        "#.......#",
                        "#...@...#",
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
                                DisplayName: "Bed",
                                Message: null,
                                VisualKind: WorldObjectVisualKind.Bed,
                                BlocksMovement: true),
                            new Point2(1, 1)),
                        new WorldObjectPlacement(
                            new WorldObjectDefinition(
                                WorldObjectDefinitionKind.ScriptedInteractable,
                                DisplayName: "Dresser",
                                Message: null,
                                VisualKind: WorldObjectVisualKind.Dresser,
                                BlocksMovement: true),
                            new Point2(6, 1)),
                        new WorldObjectPlacement(
                            new WorldObjectDefinition(
                                WorldObjectDefinitionKind.ScriptedInteractable,
                                DisplayName: "Inspector",
                                Message: "Feel free to go about your business for the time being",
                                VisualKind: WorldObjectVisualKind.Npc,
                                BlocksMovement: true),
                            new Point2(3, 2)),
                        new WorldObjectPlacement(
                            new WorldObjectDefinition(
                                WorldObjectDefinitionKind.ScriptedInteractable,
                                DisplayName: "Warden One",
                                Message: "...",
                                VisualKind: WorldObjectVisualKind.Npc,
                                BlocksMovement: true),
                            new Point2(5, 2)),
                        new WorldObjectPlacement(
                            new WorldObjectDefinition(
                                WorldObjectDefinitionKind.ScriptedInteractable,
                                DisplayName: "Warden Two",
                                Message: "...",
                                VisualKind: WorldObjectVisualKind.Npc,
                                BlocksMovement: true),
                            new Point2(2, 4)),
                        new WorldObjectPlacement(
                            new WorldObjectDefinition(
                                WorldObjectDefinitionKind.ScriptedInteractable,
                                DisplayName: "Warden Three",
                                Message: "...",
                                VisualKind: WorldObjectVisualKind.Npc,
                                BlocksMovement: true),
                            new Point2(6, 4))
                    ]),
                new SpawnProfile(999, 0, 0, [new WeightedEntry<MonsterKind>(MonsterKind.Bird, 1)]),
                new ExitDefinition(
                [
                    new ExitRoute(new PortalDestination("tutorial", 1, null), RequiredItemId: "black-suit"),
                    new ExitRoute(new PortalDestination("apartment-intro", 1, null), Label: "Something about you is not yet in order.")
                ]))
        ];
}
