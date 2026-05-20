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
    ("Treasure threshold spawns pickup instead of granting item directly", CheckTreasureThresholdSpawnsPickup),
    ("Player stepping on item pickup stores item", CheckItemPickupStoresItem),
    ("Fixed floor definitions load through the shared runtime", CheckFixedFloorDefinitionLoads),
    ("Portal world objects can transition between dungeon definitions", CheckPortalTransitionsBetweenDungeons),
    ("Progress-gated portals stay locked until flags are unlocked", CheckProgressGatedPortal),
    ("SaveGame snapshots can restore run state across dungeons", CheckSaveGameRoundTrip)
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

static void CheckTreasureThresholdSpawnsPickup()
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
        throw new Exception("treasure threshold granted an item directly instead of spawning a pickup");
    }

    if (inventory.SlotCount != 2)
    {
        throw new Exception($"inventory capacity slot was not added: {inventory.SlotCount}");
    }

    var spawnedPickupCount = grid.AllTiles().Count(tile => tile.WorldObject is ItemPickup);
    if (spawnedPickupCount != 1)
    {
        throw new Exception($"expected exactly one spawned item pickup, found {spawnedPickupCount}");
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

static void SetProperty<T>(object target, string name, T value)
{
    var prop = target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;
    prop.SetValue(target, value);
}

sealed class InMemoryScoreStorage : IScoreStorage
{
    private string? _content;
    public bool Exists() => _content is not null;
    public string? ReadAllText() => _content;
    public void WriteAllText(string content) => _content = content;
}
