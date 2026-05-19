using BroughlikeMonoGame.Core;
using System.Reflection;

var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));

var checks = new List<(string name, Action run)>
{
    ("TryMoveActor rejects zero delta", CheckZeroDeltaRejected),
    ("GameApp processes one movement per update", CheckSingleMovementPerUpdate),
    ("Tank alternates movement turns", CheckTankAlternates)
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
        Console.WriteLine($"FAIL {name}: {ex.Message}");
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
    var grid = new LevelGrid(7);
    grid.Generate(new Random(0));
    foreach (var tile in grid.AllTiles())
    {
        if (grid.IsInBounds(tile.Position.X, tile.Position.Y))
        {
            grid.Replace(tile, TileKind.Floor);
        }
    }

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

static GameSession CreateSession()
{
    return new GameSession(new Random(0), new AudioService(), new ScoreboardService(new InMemoryScoreStorage()), SpellBook.Create());
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
