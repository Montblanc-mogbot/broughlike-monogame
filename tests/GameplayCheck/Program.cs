using BroughlikeMonoGame.Core;
using System.Reflection;

var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));

var checks = new List<(string name, Action run)>
{
    ("TryMoveActor rejects zero delta", CheckZeroDeltaRejected),
    ("GameApp processes one movement per update", CheckSingleMovementPerUpdate),
    ("Tank alternates movement turns", CheckTankAlternates),
    ("Player can move into tile after killing eater", CheckMoveAfterKillingEater),
    ("Stunned enemy does not retaliate same turn", CheckStunnedEnemyDoesNotRetaliate),
    ("UseItem consumes slot and applies effect", CheckUseItemConsumesSlot),
    ("Treasure no longer spawns score-based item rewards", CheckTreasureNoLongerSpawnsScoreItems),
    ("Player stepping on item pickup stores item", CheckItemPickupStoresItem),
    ("Enemy death drops configured item pickups", CheckEnemyDeathDropsConfiguredItem),
    ("Default registry starts the game in the apartment intro", CheckDefaultRegistryStartsInApartmentIntro),
    ("Dresser interaction spawns the black suit pickup", CheckDresserSpawnsBlackSuit),
    ("Apartment intro sends you back if you skip the black suit", CheckApartmentIntroRequiresBlackSuit),
    ("Apartment intro advances to the next dungeon with the black suit", CheckApartmentIntroAdvancesWithBlackSuit),
    ("Fixed floor definitions load through the shared runtime", CheckFixedFloorDefinitionLoads),
    ("Portal world objects can transition between dungeon definitions", CheckPortalTransitionsBetweenDungeons),
    ("Progress-gated portals stay locked until flags are unlocked", CheckProgressGatedPortal),
    ("Dungeon exits can route to different hubs based on inventory outcomes", CheckConditionalExitRouting),
    ("SaveGame snapshots can restore run state across dungeons", CheckSaveGameRoundTrip),
    ("World-state persistence shows the title when no save exists", CheckRunStatePersistenceWithoutSave),
    ("World-state persistence resumes an active run on boot", CheckRunStatePersistenceLoadsSavedRun),
    ("World-state persistence starts from currentStart when no active run exists", CheckWorldStateStartsFromCurrentStart),
    ("World-state persistence keeps the file and clears only activeRun when the run is no longer active", CheckRunStatePersistenceClearsInactiveRuns),
    ("Apartment intro completion can update currentStart for future boots", CheckApartmentIntroUpdatesCurrentStart),
    ("Content validator accepts the default registry", CheckContentValidatorAcceptsDefaultRegistry),
    ("Content validator catches broken authoring references", CheckContentValidatorCatchesBrokenReferences)
};

foreach (var (name, run) in checks)
{
    try
    {
        run();
        Console.WriteLine($"PASS {name}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"FAIL {name}: {ex}");
        Environment.ExitCode = 1;
    }
}

static void CheckZeroDeltaRejected()
{
    var session = CreateSession();
    session.StartGame();
    var player = session.Player;
    var before = player.Tile;
    if (session.TryMoveActor(player, new Point2(0, 0)))
    {
        throw new Exception("zero delta reported success");
    }
    if (!ReferenceEquals(before, player.Tile))
    {
        throw new Exception("player tile changed on zero delta");
    }
}

void CheckSingleMovementPerUpdate()
{
    var source = File.ReadAllText(Path.Combine(repoRoot, "src/BroughlikeMonoGame.Core/GameApp.cs"));
    var moveCalls = System.Text.RegularExpressions.Regex.Matches(source, "_session\\.TryMovePlayer\\(").Count;
    if (moveCalls != 1)
    {
        throw new Exception($"expected exactly one TryMovePlayer dispatch site, found {moveCalls}");
    }
}

static void CheckTankAlternates()
{
    var session = CreateSession();
    var grid = CreateOpenFloorGrid(7);

    SetProperty(session, nameof(GameSession.Grid), grid);
    var player = new MonsterActor(MonsterCatalog.Player, grid.GetTile(4, 3), isPlayer: true);
    SetProperty(session, nameof(GameSession.Player), player);
    var tank = new MonsterActor(MonsterCatalog.Tank, grid.GetTile(2, 3));
    tank.TeleportCounter = 0;
    var monsters = GetMonsters(session);
    monsters.Clear();
    monsters.Add(tank);
    SetProperty(session, nameof(GameSession.Mode), GameMode.Running);

    session.Tick();
    if (tank.Tile.Position != new Point2(3, 3))
    {
        throw new Exception($"tank failed first move: {tank.Tile.Position}");
    }

    session.Tick();
    if (tank.Tile.Position != new Point2(3, 3))
    {
        throw new Exception($"tank should pause on stunned turn but moved to {tank.Tile.Position}");
    }
}

static void CheckMoveAfterKillingEater()
{
    var session = CreateSession();
    var grid = CreateOpenFloorGrid(7);
    SetProperty(session, nameof(GameSession.Grid), grid);

    var player = new MonsterActor(MonsterCatalog.Player, grid.GetTile(3, 3), isPlayer: true);
    SetProperty(session, nameof(GameSession.Player), player);

    var eater = new MonsterActor(MonsterCatalog.Eater, grid.GetTile(3, 2));
    eater.TeleportCounter = 0;
    var monsters = GetMonsters(session);
    monsters.Clear();
    monsters.Add(eater);
    SetProperty(session, nameof(GameSession.Mode), GameMode.Running);

    session.TryMovePlayer(new Point2(0, -1));
    if (!eater.Dead)
    {
        throw new Exception("eater should be dead after player attack");
    }
    if (player.Tile.Position != new Point2(3, 3))
    {
        throw new Exception($"player should stay in place after kill, got {player.Tile.Position}");
    }

    session.TryMovePlayer(new Point2(0, -1));
    if (player.Tile.Position != new Point2(3, 2))
    {
        throw new Exception($"player failed to move into cleared tile after kill: {player.Tile.Position}");
    }
}

static void CheckStunnedEnemyDoesNotRetaliate()
{
    var session = CreateSession();
    var grid = CreateOpenFloorGrid(7);
    SetProperty(session, nameof(GameSession.Grid), grid);

    var player = new MonsterActor(MonsterCatalog.Player, grid.GetTile(3, 3), isPlayer: true);
    SetProperty(session, nameof(GameSession.Player), player);

    var eater = new MonsterActor(MonsterCatalog.Eater, grid.GetTile(3, 2));
    eater.TeleportCounter = 0;
    eater.Heal(1); // no-op but keeps intent explicit
    var monsters = GetMonsters(session);
    monsters.Clear();
    monsters.Add(eater);
    SetProperty(session, nameof(GameSession.Mode), GameMode.Running);

    // Give the eater enough hp to survive one hit.
    typeof(MonsterActor).GetProperty(nameof(MonsterActor.Hp), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!
        .SetValue(eater, 2f);

    var playerHpBefore = player.Hp;
    session.TryMovePlayer(new Point2(0, -1));

    if (player.Hp != playerHpBefore)
    {
        throw new Exception($"stunned eater retaliated unexpectedly; player hp {player.Hp}");
    }

    if (eater.Tile.Position != new Point2(3, 2))
    {
        throw new Exception($"eater moved despite being hit first: {eater.Tile.Position}");
    }
}

static void CheckUseItemConsumesSlot()
{
    var session = CreateSession();
    var grid = CreateOpenFloorGrid(7);
    SetProperty(session, nameof(GameSession.Grid), grid);

    var player = new MonsterActor(MonsterCatalog.Player, grid.GetTile(3, 3), isPlayer: true);
    SetProperty(session, nameof(GameSession.Player), player);
    SetProperty(session, nameof(GameSession.Mode), GameMode.Running);

    var inventory = new Inventory();
    inventory.AddSlot(ItemCatalog.CreateTutorialItems().First(item => item.Id == "power"));
    typeof(GameSession).GetProperty(nameof(GameSession.Inventory), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!
        .SetValue(session, inventory);

    session.UseItem(0);

    if (player.BonusAttack != 5)
    {
        throw new Exception($"power item did not apply bonus attack: {player.BonusAttack}");
    }

    if (inventory.GetItem(0) is not null)
    {
        throw new Exception("used item slot was not cleared");
    }
}

static void CheckTreasureNoLongerSpawnsScoreItems()
{
    var session = CreateSession();
    var grid = CreateOpenFloorGrid(7);
    SetProperty(session, nameof(GameSession.Grid), grid);

    var player = new MonsterActor(MonsterCatalog.Player, grid.GetTile(3, 3), isPlayer: true);
    SetProperty(session, nameof(GameSession.Player), player);
    SetProperty(session, nameof(GameSession.Mode), GameMode.Running);
    SetProperty(session, nameof(GameSession.Score), 2);
    SetProperty(session, nameof(GameSession.InventoryCapacity), 1);

    var inventory = new Inventory();
    inventory.AddSlot();
    typeof(GameSession).GetProperty(nameof(GameSession.Inventory), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!
        .SetValue(session, inventory);

    session.GainTreasure();

    if (inventory.GetItem(0) is not null)
    {
        throw new Exception("treasure should not inject item rewards into inventory");
    }

    if (inventory.SlotCount != 1)
    {
        throw new Exception($"inventory capacity changed unexpectedly: {inventory.SlotCount}");
    }

    var spawnedPickupCount = grid.AllTiles().Count(tile => tile.WorldObject is ItemPickup);
    if (spawnedPickupCount != 0)
    {
        throw new Exception($"score-based treasure should not spawn item pickups, found {spawnedPickupCount}");
    }
}

static void CheckItemPickupStoresItem()
{
    var session = CreateSession();
    var grid = CreateOpenFloorGrid(7);
    SetProperty(session, nameof(GameSession.Grid), grid);

    var player = new MonsterActor(MonsterCatalog.Player, grid.GetTile(3, 3), isPlayer: true);
    SetProperty(session, nameof(GameSession.Player), player);
    SetProperty(session, nameof(GameSession.Mode), GameMode.Running);
    SetProperty(session, nameof(GameSession.InventoryCapacity), 1);

    var inventory = new Inventory();
    inventory.AddSlot();
    typeof(GameSession).GetProperty(nameof(GameSession.Inventory), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!
        .SetValue(session, inventory);

    var item = ItemCatalog.CreateTutorialItems().First(definition => definition.Id == "power");
    grid.GetTile(3, 2).WorldObject = new ItemPickup(item);

    session.TryMovePlayer(new Point2(0, -1));

    if (player.Tile.Position != new Point2(3, 2))
    {
        throw new Exception($"player did not move onto item tile: {player.Tile.Position}");
    }

    if (inventory.GetItem(0)?.Id != "power")
    {
        throw new Exception("item pickup was not stored in inventory");
    }

    if (grid.GetTile(3, 2).WorldObject is not null)
    {
        throw new Exception("item pickup was not consumed from the floor");
    }
}

static void CheckEnemyDeathDropsConfiguredItem()
{
    var session = CreateSession();
    var grid = CreateOpenFloorGrid(7);
    SetProperty(session, nameof(GameSession.Grid), grid);

    var player = new MonsterActor(MonsterCatalog.Player, grid.GetTile(3, 3), isPlayer: true);
    SetProperty(session, nameof(GameSession.Player), player);
    SetProperty(session, nameof(GameSession.Mode), GameMode.Running);

    var eater = new MonsterActor(
        MonsterCatalog.Eater,
        grid.GetTile(3, 2),
        deathDrop: new WorldObjectDefinition(WorldObjectDefinitionKind.ItemPickup, ItemId: "power"));
    eater.TeleportCounter = 0;
    var monsters = GetMonsters(session);
    monsters.Clear();
    monsters.Add(eater);

    session.TryMovePlayer(new Point2(0, -1));
    session.Tick();

    if (grid.GetTile(3, 2).WorldObject is not ItemPickup pickup || pickup.Item.Id != "power")
    {
        throw new Exception("enemy death did not drop the configured item pickup");
    }
}

static void CheckDefaultRegistryStartsInApartmentIntro()
{
    var session = new GameSession(
        new Random(0),
        new AudioService(),
        new ScoreboardService(new InMemoryScoreStorage()),
        ItemCatalog.CreateTutorialItems(),
        DungeonCatalog.CreateDefaultRegistry(),
        DungeonCatalog.DefaultStartingDungeonId);

    session.StartGame();

    if (session.CurrentDungeonId != "apartment-intro")
    {
        throw new Exception($"default starting dungeon mismatch: {session.CurrentDungeonId}");
    }

    if (session.CurrentFloorDisplayName != "Bedroom")
    {
        throw new Exception($"default starting floor mismatch: {session.CurrentFloorDisplayName}");
    }

    if (session.Grid.GetTile(2, 2).Kind != TileKind.Exit)
    {
        throw new Exception("apartment intro does not place the bedroom door next to the player");
    }
}

static void CheckDresserSpawnsBlackSuit()
{
    var session = new GameSession(
        new Random(0),
        new AudioService(),
        new ScoreboardService(new InMemoryScoreStorage()),
        ItemCatalog.CreateTutorialItems(),
        DungeonCatalog.CreateDefaultRegistry(),
        DungeonCatalog.DefaultStartingDungeonId);

    session.StartGame();
    session.TryMovePlayer(new Point2(0, -1));
    session.TryMovePlayer(new Point2(0, -1));
    session.TryMovePlayer(new Point2(1, 0));
    session.TryMovePlayer(new Point2(0, -1));

    if (session.BannerMessage != "Inside the dresser: a black suit, waiting as if it had expected you.")
    {
        throw new Exception($"dresser interaction banner mismatch: {session.BannerMessage}");
    }

    if (session.Grid.GetTile(6, 1).WorldObject is not ItemPickup pickup || pickup.Item.Id != "black-suit")
    {
        throw new Exception("dresser did not spawn the black suit pickup");
    }
}

static void CheckApartmentIntroRequiresBlackSuit()
{
    var session = CreateSession(DungeonCatalog.CreateDefaultRegistry(), DungeonCatalog.DefaultStartingDungeonId);
    session.StartGame();

    Walk(session, (0, -1), (0, -1), (-1, 0), (-1, 0));
    Walk(session, (1, 0), (1, 0), (1, 0), (0, 1), (0, 1), (0, 1));
    Walk(session, (1, 0), (1, 0), (1, 0), (1, 0), (1, 0), (1, 0));
    Walk(session, (1, 0), (1, 0), (1, 0), (0, 1), (0, 1), (0, 1));

    if (session.CurrentDungeonId != "apartment-intro" || session.CurrentFloorDisplayName != "Bedroom")
    {
        throw new Exception($"expected missing suit route to return to bedroom, got {session.CurrentDungeonId} / {session.CurrentFloorDisplayName}");
    }
}

static void CheckApartmentIntroAdvancesWithBlackSuit()
{
    var session = CreateSession(DungeonCatalog.CreateDefaultRegistry(), DungeonCatalog.DefaultStartingDungeonId);
    session.StartGame();

    Walk(session, (0, -1), (0, -1), (1, 0), (0, -1));
    Walk(session, (1, 0), (0, -1));
    Walk(session, (0, 1), (-1, 0), (-1, 0), (-1, 0), (-1, 0));
    Walk(session, (1, 0), (1, 0), (1, 0), (0, 1), (0, 1), (0, 1));
    Walk(session, (1, 0), (1, 0), (1, 0), (1, 0), (1, 0), (1, 0));
    Walk(session, (1, 0), (1, 0), (1, 0), (0, 1), (0, 1), (0, 1));

    if (session.CurrentDungeonId != "tutorial")
    {
        throw new Exception($"expected black suit route to reach tutorial dungeon, got {session.CurrentDungeonId}");
    }
}

static void CheckFixedFloorDefinitionLoads()
{
    var dungeon = new DungeonDefinition(
        "test-dungeon",
        "Test Dungeon",
        [
            new FloorDefinition(
                "fixed-floor",
                "Fixed Floor",
                new FixedLevelSource(
                    [
                        "#########",
                        "#@......#",
                        "#.###...#",
                        "#...#...#",
                        "#...#...#",
                        "#...###.#",
                        "#.......#",
                        "#......>#",
                        "#########"
                    ],
                    monsters:
                    [
                        new MonsterPlacement(MonsterKind.Bird, new Point2(2, 1))
                    ],
                    worldObjects:
                    [
                        new WorldObjectPlacement(new WorldObjectDefinition(WorldObjectDefinitionKind.ItemPickup, "power"), new Point2(4, 6))
                    ]),
                new SpawnProfile(10, 1, 0, [new WeightedEntry<MonsterKind>(MonsterKind.Bird, 1)]))
        ]);

    var session = CreateSession(new DungeonRegistry([dungeon]), "test-dungeon");
    session.StartGame();

    if (session.Player.Tile.Position != new Point2(1, 1))
    {
        throw new Exception($"fixed floor player start mismatch: {session.Player.Tile.Position}");
    }

    if (session.Grid.GetTile(7, 7).Kind != TileKind.Exit)
    {
        throw new Exception("fixed floor exit was not loaded");
    }

    if (session.Monsters.Count != 1 || session.Monsters[0].Kind != MonsterKind.Bird)
    {
        throw new Exception("fixed floor monster placement was not loaded");
    }

    if (session.Grid.GetTile(4, 6).WorldObject is not ItemPickup pickup || pickup.Item.Id != "power")
    {
        throw new Exception("fixed floor item pickup was not loaded");
    }
}

static void CheckPortalTransitionsBetweenDungeons()
{
    var hub = new DungeonDefinition(
        "hub",
        "Hub",
        [
            new FloorDefinition(
                "hub-floor",
                "Hub Floor",
                new FixedLevelSource(
                    [
                        "#########",
                        "#@......#",
                        "#.......#",
                        "#.......#",
                        "#.......#",
                        "#.......#",
                        "#.......#",
                        "#......>#",
                        "#########"
                    ],
                    worldObjects:
                    [
                        new WorldObjectPlacement(
                            new WorldObjectDefinition(
                                WorldObjectDefinitionKind.Portal,
                                PortalDestination: new PortalDestination("crypt", 1, "Enter the crypt")),
                            new Point2(2, 1))
                    ]),
                new SpawnProfile(999, 0, 0, [new WeightedEntry<MonsterKind>(MonsterKind.Bird, 1)]))
        ]);

    var crypt = new DungeonDefinition(
        "crypt",
        "Crypt",
        [
            new FloorDefinition(
                "crypt-floor-1",
                "Crypt Floor 1",
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
                new SpawnProfile(12, 0, 0, [new WeightedEntry<MonsterKind>(MonsterKind.Bird, 1)]))
        ]);

    var session = CreateSession(new DungeonRegistry([hub, crypt]), "hub");
    session.StartGame();
    session.TryMovePlayer(new Point2(1, 0));

    if (session.CurrentDungeonId != "crypt")
    {
        throw new Exception($"portal did not change current dungeon: {session.CurrentDungeonId}");
    }

    if (session.Level != 1)
    {
        throw new Exception($"portal did not set target floor: {session.Level}");
    }

    if (session.Player.Tile.Position != new Point2(3, 2))
    {
        throw new Exception($"portal destination player start mismatch: {session.Player.Tile.Position}");
    }

    if (session.BannerMessage != "Enter the crypt")
    {
        throw new Exception($"portal did not set entry banner: {session.BannerMessage}");
    }
}

static void CheckProgressGatedPortal()
{
    var hub = new DungeonDefinition(
        "hub",
        "Hub",
        [
            new FloorDefinition(
                "hub-floor",
                "Hub Floor",
                new FixedLevelSource(
                    [
                        "#########",
                        "#@......#",
                        "#.......#",
                        "#.......#",
                        "#.......#",
                        "#.......#",
                        "#.......#",
                        "#......>#",
                        "#########"
                    ],
                    worldObjects:
                    [
                        new WorldObjectPlacement(
                            new WorldObjectDefinition(
                                WorldObjectDefinitionKind.Portal,
                                PortalDestination: new PortalDestination("crypt", 1, "Enter the sealed crypt"),
                                RequiredProgressFlag: "hub.crypt.unsealed"),
                            new Point2(2, 1))
                    ]),
                new SpawnProfile(999, 0, 0, [new WeightedEntry<MonsterKind>(MonsterKind.Bird, 1)]))
        ]);

    var crypt = new DungeonDefinition(
        "crypt",
        "Crypt",
        [
            new FloorDefinition(
                "crypt-floor-1",
                "Crypt Floor 1",
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
                new SpawnProfile(12, 0, 0, [new WeightedEntry<MonsterKind>(MonsterKind.Bird, 1)]))
        ]);

    var session = CreateSession(new DungeonRegistry([hub, crypt]), "hub");
    session.StartGame();
    session.TryMovePlayer(new Point2(1, 0));

    if (session.CurrentDungeonId != "hub")
    {
        throw new Exception("locked portal should not transition dungeons");
    }

    if (session.BannerMessage != "Enter the sealed crypt is sealed.")
    {
        throw new Exception($"locked portal banner mismatch: {session.BannerMessage}");
    }

    session.UnlockProgressFlag("hub.crypt.unsealed");
    session.TryMovePlayer(new Point2(-1, 0));
    session.TryMovePlayer(new Point2(1, 0));

    if (session.CurrentDungeonId != "crypt")
    {
        throw new Exception("unlocked portal did not transition dungeons");
    }
}

static void CheckConditionalExitRouting()
{
    var hub1 = new DungeonDefinition(
        "hub-1",
        "Hub 1",
        [
            new FloorDefinition(
                "hub-1-floor",
                "Hub 1 Floor",
                new FixedLevelSource(
                    [
                        "#########",
                        "#@......#",
                        "#.......#",
                        "#.......#",
                        "#.......#",
                        "#.......#",
                        "#.......#",
                        "#......>#",
                        "#########"
                    ]),
                new SpawnProfile(999, 0, 0, [new WeightedEntry<MonsterKind>(MonsterKind.Bird, 1)]))
        ]);

    var hub2 = new DungeonDefinition(
        "hub-2",
        "Hub 2",
        [
            new FloorDefinition(
                "hub-2-floor",
                "Hub 2 Floor",
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

    var hub3 = new DungeonDefinition(
        "hub-3",
        "Hub 3",
        [
            new FloorDefinition(
                "hub-3-floor",
                "Hub 3 Floor",
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

    var dungeon = new DungeonDefinition(
        "key-dungeon",
        "Key Dungeon",
        [
            new FloorDefinition(
                "key-dungeon-floor-1",
                "Key Dungeon Floor 1",
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
                        new ExitRoute(new PortalDestination("hub-2", 1, "Returned with the key"), RequiredItemId: "power", Label: "Returned with the key"),
                        new ExitRoute(new PortalDestination("hub-3", 1, "Returned empty-handed"), Label: "Returned empty-handed")
                    ]))
        ]);

    var registry = new DungeonRegistry([hub1, hub2, hub3, dungeon]);

    var successSession = CreateSession(registry, "key-dungeon");
    successSession.StartGame();
    var successInventory = new Inventory();
    successInventory.AddSlot(ItemCatalog.CreateTutorialItems().First(item => item.Id == "power"));
    typeof(GameSession).GetProperty(nameof(GameSession.Inventory), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!
        .SetValue(successSession, successInventory);
    successSession.TryMovePlayer(new Point2(1, 0));

    if (successSession.CurrentDungeonId != "hub-2")
    {
        throw new Exception($"successful exit did not route to hub-2: {successSession.CurrentDungeonId}");
    }

    var failureSession = CreateSession(registry, "key-dungeon");
    failureSession.StartGame();
    var failureInventory = new Inventory();
    failureInventory.AddSlot();
    typeof(GameSession).GetProperty(nameof(GameSession.Inventory), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!
        .SetValue(failureSession, failureInventory);
    failureSession.TryMovePlayer(new Point2(1, 0));

    if (failureSession.CurrentDungeonId != "hub-3")
    {
        throw new Exception($"failed exit did not route to hub-3: {failureSession.CurrentDungeonId}");
    }
}

static void CheckRunStatePersistenceWithoutSave()
{
    var storage = new InMemorySaveStorage();
    var persistence = new RunStatePersistence(new WorldStateService(storage));
    var session = CreateSession();

    persistence.Initialize(session);

    if (session.Mode != GameMode.Title)
    {
        throw new Exception($"expected title mode without a save, got {session.Mode}");
    }
}

static void CheckRunStatePersistenceLoadsSavedRun()
{
    var storage = new InMemorySaveStorage();
    var service = new WorldStateService(storage);
    var persistence = new RunStatePersistence(service);
    var worldState = WorldState.CreateDefault("slot-1", "apartment-intro") with
    {
        ActiveRun = new SaveGame("tutorial", 1, 2f, 5f, 0, 3, ["power", null, null], ["apartment.intro.complete"])
    };
    service.Save(worldState);

    var resumed = CreateSession();
    persistence.Initialize(resumed);

    if (resumed.Mode != GameMode.Running)
    {
        throw new Exception($"expected resumed session to be running, got {resumed.Mode}");
    }

    if (resumed.CurrentDungeonId != "tutorial")
    {
        throw new Exception($"expected tutorial resume, got {resumed.CurrentDungeonId}");
    }

    if (!resumed.HasProgressFlag("apartment.intro.complete"))
    {
        throw new Exception("expected progress flag to survive boot resume");
    }
}

static void CheckWorldStateStartsFromCurrentStart()
{
    var storage = new InMemorySaveStorage();
    var service = new WorldStateService(storage);
    var persistence = new RunStatePersistence(service);
    var worldState = new WorldState(
        "slot-1",
        new WorldStartState("tutorial", 1),
        new WorldPlayerState(4f, 5f, ["power", null, null]),
        new Dictionary<string, bool> { ["met_painter"] = false, ["apartment.intro.complete"] = true },
        null,
        ["black-suit"],
        ["apartment-intro", "tutorial"],
        "apartment-intro");
    service.Save(worldState);

    var session = CreateSession();
    persistence.Initialize(session);

    if (session.Mode != GameMode.Title)
    {
        throw new Exception($"expected title mode before starting, got {session.Mode}");
    }

    session.StartGame();

    if (session.CurrentDungeonId != "tutorial")
    {
        throw new Exception($"expected StartGame to use world-state currentStart, got {session.CurrentDungeonId}");
    }

    if (session.Player.Hp != 4f)
    {
        throw new Exception($"expected world-state hp 4, got {session.Player.Hp}");
    }

    if (session.Player.MaxHp != 5f)
    {
        throw new Exception($"expected world-state max hp 5, got {session.Player.MaxHp}");
    }

    if (session.Inventory.GetItem(0)?.Id != "power")
    {
        throw new Exception("expected world-state inventory to seed the start state");
    }

    if (!session.HasProgressFlag("apartment.intro.complete"))
    {
        throw new Exception("expected true story flags to load into session state");
    }

    if (session.HasProgressFlag("met_painter"))
    {
        throw new Exception("false story flags should not load as unlocked progress flags");
    }
}

static void CheckContentValidatorAcceptsDefaultRegistry()
{
    var errors = ContentValidator.Validate(DungeonCatalog.CreateDefaultRegistry(), ItemCatalog.CreateTutorialItems(), DungeonCatalog.DefaultStartingDungeonId);
    if (errors.Count > 0)
    {
        throw new Exception("expected default registry to validate cleanly:\n" + string.Join("\n", errors));
    }
}

static void CheckContentValidatorCatchesBrokenReferences()
{
    var invalid = new DungeonDefinition(
        "broken",
        "Broken",
        [
            new FloorDefinition(
                "broken-1",
                "Broken Floor",
                new FixedLevelSource(
                    [
                        "#########",
                        "#@......#",
                        "#.......#",
                        "#.......#",
                        "#.......#",
                        "#.......#",
                        "#.......#",
                        "#......>#",
                        "#########",
                    ],
                    monsters:
                    [
                        new MonsterPlacement(
                            MonsterKind.Bird,
                            new Point2(1, 1),
                            new WorldObjectDefinition(WorldObjectDefinitionKind.ItemPickup, ItemId: "missing-drop"))
                    ],
                    worldObjects:
                    [
                        new WorldObjectPlacement(
                            new WorldObjectDefinition(WorldObjectDefinitionKind.Portal, PortalDestination: new PortalDestination("missing-dungeon", 1)),
                            new Point2(2, 1)),
                        new WorldObjectPlacement(
                            new WorldObjectDefinition(WorldObjectDefinitionKind.ItemPickup, ItemId: "missing-item"),
                            new Point2(3, 1))
                    ]),
                new SpawnProfile(
                    999,
                    0,
                    0,
                    [new WeightedEntry<MonsterKind>(MonsterKind.Bird, 1)],
                    initialFloorItemCount: 1,
                    itemTable: [new WeightedEntry<string>("missing-table-item", 1)]),
                new ExitDefinition(
                    [
                        new ExitRoute(new PortalDestination("tutorial", 99), RequiredItemId: "missing-exit-item")
                    ]))
        ]);

    var errors = ContentValidator.Validate(new DungeonRegistry([invalid, TutorialDungeonDefinition.Create()]), ItemCatalog.CreateTutorialItems(), "missing-start");
    var joined = string.Join("\n", errors);

    ExpectContains(joined, "Starting dungeon 'missing-start' does not exist in the registry.");
    ExpectContains(joined, "exit route points to impossible floor 99 in dungeon 'tutorial'.");
    ExpectContains(joined, "exit route requires unknown item 'missing-exit-item'.");
    ExpectContains(joined, "item table references unknown item 'missing-table-item'.");
    ExpectContains(joined, "world object at 2,1 points to unknown dungeon 'missing-dungeon'.");
    ExpectContains(joined, "world object at 3,1 references unknown item 'missing-item'.");
    ExpectContains(joined, "death drop for 'Bird' references unknown item 'missing-drop'.");
}

static void CheckApartmentIntroUpdatesCurrentStart()
{
    var storage = new InMemorySaveStorage();
    var service = new WorldStateService(storage);
    var persistence = new RunStatePersistence(service);
    var session = CreateSession();
    persistence.Initialize(session);
    session.StartGame();

    // walk through the apartment intro, get the suit, and take the final exit.
    Walk(session, (0, -1), (0, -1), (1, 0), (0, -1));
    Walk(session, (1, 0), (0, -1));
    Walk(session, (0, 1), (-1, 0), (-1, 0), (-1, 0), (-1, 0));
    Walk(session, (1, 0), (1, 0), (1, 0), (0, 1), (0, 1), (0, 1));
    Walk(session, (1, 0), (1, 0), (1, 0), (1, 0), (1, 0), (1, 0));
    Walk(session, (1, 0), (1, 0), (1, 0), (0, 1), (0, 1), (0, 1));

    if (session.CurrentDungeonId != "tutorial")
    {
        throw new Exception($"expected apartment success route to enter tutorial, got {session.CurrentDungeonId}");
    }

    persistence.Sync(session);
    session.ShowTitle();
    persistence.Sync(session);

    var worldState = service.Load() ?? throw new Exception("expected world state after apartment progression");
    if (worldState.CurrentStart.DungeonId != "tutorial")
    {
        throw new Exception($"expected apartment completion to set currentStart to tutorial, got {worldState.CurrentStart.DungeonId}");
    }
}

static void CheckRunStatePersistenceClearsInactiveRuns()
{
    var storage = new InMemorySaveStorage();
    var service = new WorldStateService(storage);
    var persistence = new RunStatePersistence(service);
    var session = CreateSession();
    persistence.Initialize(session);
    session.StartGame();
    persistence.Sync(session);

    var runningState = service.Load() ?? throw new Exception("expected world state after running sync");
    if (runningState.ActiveRun is null)
    {
        throw new Exception("expected running session to persist an activeRun");
    }

    session.ShowTitle();
    persistence.Sync(session);

    var titleState = service.Load() ?? throw new Exception("expected world state file to remain after title sync");
    if (titleState.ActiveRun is not null)
    {
        throw new Exception("expected title-state sync to clear only the activeRun");
    }
}

static void CheckSaveGameRoundTrip()
{
    var hub = new DungeonDefinition(
        "hub",
        "Hub",
        [
            new FloorDefinition(
                "hub-floor",
                "Hub Floor",
                new FixedLevelSource(
                    [
                        "#########",
                        "#@......#",
                        "#.......#",
                        "#.......#",
                        "#.......#",
                        "#.......#",
                        "#.......#",
                        "#......>#",
                        "#########"
                    ]),
                new SpawnProfile(999, 0, 0, [new WeightedEntry<MonsterKind>(MonsterKind.Bird, 1)]))
        ]);

    var crypt = new DungeonDefinition(
        "crypt",
        "Crypt",
        [
            new FloorDefinition(
                "crypt-floor-1",
                "Crypt Floor 1",
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
                new SpawnProfile(12, 0, 0, [new WeightedEntry<MonsterKind>(MonsterKind.Bird, 1)]))
        ]);

    var session = CreateSession(new DungeonRegistry([hub, crypt]), "hub");
    session.StartGame();
    SetProperty(session, nameof(GameSession.InventoryCapacity), 2);
    var inventory = new Inventory();
    inventory.AddSlot(ItemCatalog.CreateTutorialItems().First(item => item.Id == "power"));
    inventory.AddSlot();
    typeof(GameSession).GetProperty(nameof(GameSession.Inventory), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!
        .SetValue(session, inventory);
    SetProperty(session, nameof(GameSession.Score), 7);
    session.EnterDungeon("crypt", 1, playerHp: 2f, inventoryItemIds: inventory.ToItemIds());

    session.UnlockProgressFlag("hub.crypt.unsealed");

    var save = session.CreateSaveGame();

    var restored = CreateSession(new DungeonRegistry([hub, crypt]), "hub");
    restored.LoadSaveGame(save);

    if (restored.CurrentDungeonId != "crypt")
    {
        throw new Exception($"restored dungeon mismatch: {restored.CurrentDungeonId}");
    }

    if (restored.Level != 1)
    {
        throw new Exception($"restored level mismatch: {restored.Level}");
    }

    if (restored.Score != 7)
    {
        throw new Exception($"restored score mismatch: {restored.Score}");
    }

    if (restored.InventoryCapacity != 2)
    {
        throw new Exception($"restored inventory capacity mismatch: {restored.InventoryCapacity}");
    }

    if (restored.Inventory.GetItem(0)?.Id != "power")
    {
        throw new Exception("restored inventory item mismatch");
    }

    if (restored.Player.Hp != 2f)
    {
        throw new Exception($"restored player hp mismatch: {restored.Player.Hp}");
    }

    if (!restored.HasProgressFlag("hub.crypt.unsealed"))
    {
        throw new Exception("restored progression flag missing");
    }
}

static GameSession CreateSession(DungeonRegistry? dungeons = null, string startingDungeonId = "tutorial")
{
    return new GameSession(
        new Random(0),
        new AudioService(),
        new ScoreboardService(new InMemoryScoreStorage()),
        ItemCatalog.CreateTutorialItems(),
        dungeons,
        startingDungeonId);
}

static void Walk(GameSession session, params (int x, int y)[] steps)
{
    foreach (var (x, y) in steps)
    {
        session.TryMovePlayer(new Point2(x, y));
    }
}

static LevelGrid CreateOpenFloorGrid(int size)
{
    var grid = new LevelGrid(size);
    grid.Generate(new Random(0));
    foreach (var tile in grid.AllTiles())
    {
        if (grid.IsInBounds(tile.Position.X, tile.Position.Y))
        {
            grid.Replace(tile, TileKind.Floor);
        }
    }

    return grid;
}

static List<MonsterActor> GetMonsters(GameSession session)
{
    var field = typeof(GameSession).GetField("_monsters", BindingFlags.NonPublic | BindingFlags.Instance)!;
    return (List<MonsterActor>)field.GetValue(session)!;
}

static void ExpectContains(string text, string expected)
{
    if (!text.Contains(expected, StringComparison.Ordinal))
    {
        throw new Exception($"expected to find '{expected}' in validation output:\n{text}");
    }
}

static void SetProperty<T>(object target, string name, T value)
{
    var prop = target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
    prop.SetValue(target, value);
}

sealed class InMemorySaveStorage : ISaveStorage
{
    private string? _content;

    public bool Exists() => !string.IsNullOrWhiteSpace(_content);

    public string? ReadAllText() => _content;

    public void WriteAllText(string content) => _content = content;

    public void Delete() => _content = null;
}

sealed class InMemoryScoreStorage : IScoreStorage
{
    private string? _content;
    public bool Exists() => _content is not null;
    public string? ReadAllText() => _content;
    public void WriteAllText(string content) => _content = content;
}
