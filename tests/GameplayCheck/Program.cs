using BroughlikeMonoGame.Core;
using System.Reflection;

var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));

var checks = new List<(string name, Action run)>
{
    ("TryMoveActor rejects zero delta", CheckZeroDeltaRejected),
    ("GameApp processes one movement per update", CheckSingleMovementPerUpdate),
    ("Tank alternates movement turns", CheckTankAlternates),
    ("Player can move into tile after killing eater", CheckMoveAfterKillingEater),
    ("Stunned enemy does not retaliate same turn", CheckStunnedEnemyDoesNotRetaliate)
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

static GameSession CreateSession()
{
    return new GameSession(new Random(0), new AudioService(), new ScoreboardService(new InMemoryScoreStorage()), SpellBook.Create());
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
