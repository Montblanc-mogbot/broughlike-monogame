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
                                Message: "The bed is still warm.",
                                VisualKind: WorldObjectVisualKind.Bed,
                                BlocksMovement: true),
                            new Point2(2, 1)),
                        new WorldObjectPlacement(
                            new WorldObjectDefinition(
                                WorldObjectDefinitionKind.ScriptedInteractable,
                                ItemId: "black-suit",
                                DisplayName: "Dresser",
                                Message: "Put on your black suit",
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
                    new ExitRoute(new PortalDestination("apartment-intro", 2, "Into the living room"), Label: "Into the living room")
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
                                Message: "The living room is too quiet.",
                                VisualKind: WorldObjectVisualKind.Bed,
                                BlocksMovement: true),
                            new Point2(2, 1)),
                        new WorldObjectPlacement(
                            new WorldObjectDefinition(
                                WorldObjectDefinitionKind.ScriptedInteractable,
                                DisplayName: "Side Table",
                                Message: "Nothing here but dust.",
                                VisualKind: WorldObjectVisualKind.Dresser,
                                BlocksMovement: true),
                            new Point2(5, 2))
                    ]),
                new SpawnProfile(999, 0, 0, [new WeightedEntry<MonsterKind>(MonsterKind.Bird, 1)]),
                new ExitDefinition(
                [
                    new ExitRoute(new PortalDestination("apartment-intro", 3, "Into the hallway"), Label: "Into the hallway")
                ])),
            new FloorDefinition(
                "apartment-hallway",
                "Hallway",
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
                    ]),
                new SpawnProfile(999, 0, 0, [new WeightedEntry<MonsterKind>(MonsterKind.Bird, 1)]),
                new ExitDefinition(
                [
                    new ExitRoute(new PortalDestination("apartment-intro", 4, "Into the meeting room"), Label: "Into the meeting room")
                ])),
            new FloorDefinition(
                "apartment-meeting-room",
                "Meeting Room",
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
                                DisplayName: "Inspector",
                                Message: "Feel free to go about your business for the time being",
                                VisualKind: WorldObjectVisualKind.Npc,
                                BlocksMovement: true),
                            new Point2(3, 2)),
                        new WorldObjectPlacement(
                            new WorldObjectDefinition(
                                WorldObjectDefinitionKind.ScriptedInteractable,
                                DisplayName: "Officer One",
                                Message: "He studies you in silence.",
                                VisualKind: WorldObjectVisualKind.Npc,
                                BlocksMovement: true),
                            new Point2(5, 2)),
                        new WorldObjectPlacement(
                            new WorldObjectDefinition(
                                WorldObjectDefinitionKind.ScriptedInteractable,
                                DisplayName: "Officer Two",
                                Message: "The room feels smaller every moment.",
                                VisualKind: WorldObjectVisualKind.Npc,
                                BlocksMovement: true),
                            new Point2(2, 4)),
                        new WorldObjectPlacement(
                            new WorldObjectDefinition(
                                WorldObjectDefinitionKind.ScriptedInteractable,
                                DisplayName: "Clerk",
                                Message: "Nobody offers an explanation.",
                                VisualKind: WorldObjectVisualKind.Npc,
                                BlocksMovement: true),
                            new Point2(6, 4))
                    ]),
                new SpawnProfile(999, 0, 0, [new WeightedEntry<MonsterKind>(MonsterKind.Bird, 1)]),
                new ExitDefinition(
                [
                    new ExitRoute(new PortalDestination("tutorial", 1, "Step into the city"), RequiredItemId: "black-suit", Label: "Step into the city"),
                    new ExitRoute(new PortalDestination("apartment-intro", 1, "You are sent back to get dressed"), Label: "You are sent back to get dressed")
                ]))
        ];
}
