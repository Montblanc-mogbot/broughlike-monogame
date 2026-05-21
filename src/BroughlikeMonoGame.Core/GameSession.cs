using System;
using System.Collections.Generic;
using System.Linq;

namespace BroughlikeMonoGame.Core;

public sealed class GameSession
{
    private readonly Random _random;
    private readonly AudioService _audio;
    private readonly ScoreboardService _scoreboard;
    private readonly Dictionary<string, ItemDefinition> _itemCatalog;
    private readonly DungeonRegistry _dungeons;
    private readonly string _startingDungeonId;
    private readonly List<MonsterActor> _monsters = [];
    private readonly HashSet<string> _progressFlags = new(StringComparer.OrdinalIgnoreCase);
    private string _titleStartDungeonId;
    private int _titleStartFloor = 1;
    private float _titleStartPlayerHp = GameConstants.StartingHp;
    private float _titleStartPlayerMaxHp = MonsterCatalog.Player.MaxHp;
    private IReadOnlyList<string?>? _titleStartInventoryItemIds;
    private IReadOnlyList<string?> _stashItemIds = [];
    private FloorDefinition? _currentFloor;

    public GameSession(
        Random random,
        AudioService audio,
        ScoreboardService scoreboard,
        IReadOnlyList<ItemDefinition> itemCatalog,
        DungeonRegistry? dungeons = null,
        string startingDungeonId = "tutorial")
    {
        _random = random;
        _audio = audio;
        _scoreboard = scoreboard;
        _itemCatalog = itemCatalog.ToDictionary(item => item.Id, StringComparer.OrdinalIgnoreCase);
        _dungeons = dungeons ?? DungeonCatalog.CreateDefaultRegistry();
        _startingDungeonId = startingDungeonId;
        _titleStartDungeonId = startingDungeonId;
        CurrentDungeonId = startingDungeonId;
        Scores = _scoreboard.Load();
        Mode = GameMode.Title;
        Grid = new LevelGrid(Layout.MapTiles);
    }

    public GameMode Mode { get; private set; }

    public LevelGrid Grid { get; private set; }

    public MonsterActor Player { get; private set; } = null!;

    public IReadOnlyList<MonsterActor> Monsters => _monsters;

    public List<ScoreEntry> Scores { get; private set; }

    public Inventory Inventory { get; private set; } = new();

    public string CurrentDungeonId { get; private set; }

    public string StartingDungeonId => _startingDungeonId;

    public string CurrentStartDungeonId => _titleStartDungeonId;

    public int CurrentStartFloorNumber => _titleStartFloor;

    public float CurrentStartPlayerHp => _titleStartPlayerHp;

    public float CurrentStartPlayerMaxHp => _titleStartPlayerMaxHp;

    public IReadOnlyList<string?> CurrentStartInventoryItemIds => _titleStartInventoryItemIds ?? WorldState.CreateDefault("slot-1", _startingDungeonId).Player.InventoryItemIds;

    public IReadOnlyList<string?> StashItemIds => _stashItemIds;

    public IReadOnlyCollection<string> ProgressFlags => _progressFlags;

    public DungeonDefinition CurrentDungeon => _dungeons.Get(CurrentDungeonId);

    public int Level { get; private set; } = 1;

    public string CurrentFloorDisplayName => _currentFloor?.DisplayName ?? $"Floor {Level}";

    public int Score { get; private set; }

    public int SpawnRate { get; private set; }

    public int SpawnCounter { get; private set; }

    public int InventoryCapacity { get; private set; }

    public int ShakeAmount { get; private set; }

    public Point2 ShakeOffset { get; private set; }

    public string? BannerMessage { get; private set; }

    public void ShowTitle()
    {
        Mode = GameMode.Title;
        BannerMessage = "Press any movement key to begin";
    }

    public void StartGame()
    {
        CurrentDungeonId = _titleStartDungeonId;
        Level = _titleStartFloor;
        Score = 0;
        InventoryCapacity = _titleStartInventoryItemIds?.Count ?? GameConstants.InitialSpellCount;
        StartLevel(_titleStartPlayerHp, _titleStartInventoryItemIds, _titleStartPlayerMaxHp);
        Mode = GameMode.Running;
        BannerMessage = null;
    }

    public SaveGame CreateSaveGame()
        => new(
            CurrentDungeonId,
            Level,
            Player.Hp,
            Player.MaxHp,
            Score,
            InventoryCapacity,
            Inventory.ToItemIds(),
            _progressFlags.OrderBy(flag => flag, StringComparer.OrdinalIgnoreCase).ToArray());

    public void LoadSaveGame(SaveGame saveGame)
    {
        _progressFlags.Clear();
        foreach (var flag in saveGame.ProgressFlags)
        {
            _progressFlags.Add(flag);
        }

        CurrentDungeonId = saveGame.DungeonId;
        Level = saveGame.FloorNumber;
        Score = saveGame.Score;
        InventoryCapacity = saveGame.InventoryCapacity;
        StartLevel(saveGame.PlayerHp, saveGame.InventoryItemIds, saveGame.PlayerMaxHp);
        Mode = GameMode.Running;
        BannerMessage = $"Resumed {CurrentDungeon.DisplayName}";
    }

    public void LoadWorldState(WorldState worldState)
    {
        _titleStartDungeonId = worldState.CurrentStart.DungeonId;
        _titleStartFloor = worldState.CurrentStart.FloorNumber;
        _titleStartPlayerHp = worldState.Player.CurrentHp;
        _titleStartPlayerMaxHp = worldState.Player.MaxHp;
        _titleStartInventoryItemIds = worldState.Player.InventoryItemIds.ToArray();
        _stashItemIds = worldState.StashItemIds.ToArray();

        _progressFlags.Clear();
        foreach (var flag in worldState.StoryFlags.Where(entry => entry.Value).Select(entry => entry.Key))
        {
            _progressFlags.Add(flag);
        }

        CurrentDungeonId = _titleStartDungeonId;
        Level = _titleStartFloor;
        InventoryCapacity = _titleStartInventoryItemIds.Count;
        Mode = GameMode.Title;
        BannerMessage = "Press any movement key to begin";
    }

    public void UpdateCurrentStart(string dungeonId, int floorNumber, float currentHp, float maxHp, IReadOnlyList<string?> inventoryItemIds)
    {
        _titleStartDungeonId = dungeonId;
        _titleStartFloor = floorNumber;
        _titleStartPlayerHp = currentHp;
        _titleStartPlayerMaxHp = maxHp;
        _titleStartInventoryItemIds = inventoryItemIds.ToArray();
        InventoryCapacity = _titleStartInventoryItemIds.Count;
    }

    public void EnterDungeon(string dungeonId, int floorNumber, float? playerHp = null, IReadOnlyList<string?>? inventoryItemIds = null)
    {
        CurrentDungeonId = dungeonId;
        Level = floorNumber;
        StartLevel(playerHp ?? Player.Hp, inventoryItemIds ?? Inventory.ToItemIds(), Player.MaxHp);
    }

    public bool EvaluateExitRoute(ExitRoute route)
    {
        if (!string.IsNullOrWhiteSpace(route.RequiredItemId))
        {
            var hasItem = Inventory.ContainsItem(route.RequiredItemId);
            if (route.RequireItem != hasItem)
            {
                return false;
            }
        }

        if (!string.IsNullOrWhiteSpace(route.RequiredProgressFlag))
        {
            var hasFlag = HasProgressFlag(route.RequiredProgressFlag);
            if (route.RequireProgressFlag != hasFlag)
            {
                return false;
            }
        }

        return true;
    }

    public bool HasProgressFlag(string flag)
        => _progressFlags.Contains(flag);

    public void UnlockProgressFlag(string flag)
    {
        if (!string.IsNullOrWhiteSpace(flag))
        {
            _progressFlags.Add(flag);
        }
    }

    public void StartLevel(float playerHp, IReadOnlyList<string?>? inventoryItemIds, float playerMaxHp)
    {
        _currentFloor = CurrentDungeon.GetFloor(Level);
        SpawnRate = _currentFloor.SpawnProfile.InitialSpawnRate;
        SpawnCounter = SpawnRate;

        var plan = _currentFloor.LevelSource.Build(
            _random,
            new FloorBuildContext(Level, Layout.MapTiles, _currentFloor.SpawnProfile, ResolveItemDefinition));

        Grid = plan.Grid;
        _monsters.Clear();

        foreach (var placement in plan.Monsters)
        {
            var monsterTile = Grid.GetTile(placement.Position.X, placement.Position.Y);
            _monsters.Add(new MonsterActor(MonsterCatalog.Get(placement.Kind), monsterTile, deathDrop: placement.DeathDrop));
        }

        foreach (var placement in plan.WorldObjects)
        {
            var tile = Grid.GetTile(placement.Position.X, placement.Position.Y);
            tile.WorldObject = WorldObjectFactory.Create(placement.Definition, ResolveItemDefinition);
        }

        Player = new MonsterActor(MonsterCatalog.Player, Grid.GetTile(plan.PlayerStart.X, plan.PlayerStart.Y), isPlayer: true);
        Player.SetMaxHp(playerMaxHp);
        Player.Heal(playerHp - Player.Hp);
        Inventory = new Inventory();
        if (inventoryItemIds is null)
        {
            if (CurrentDungeon.SeedsRandomStartingInventory)
            {
                DrawInitialInventory(InventoryCapacity);
            }
            else
            {
                for (var i = 0; i < InventoryCapacity; i++)
                {
                    Inventory.AddSlot();
                }
            }
        }
        else
        {
            Inventory.LoadFromIds(inventoryItemIds, ResolveItemDefinition);
        }
    }

    public void AdvanceFrame()
    {
        if (Player is not null)
        {
            Player.TickAnimation();
            Player.ClearTurnFeedback();
        }

        foreach (var monster in _monsters)
        {
            monster.TickAnimation();
            monster.ClearTurnFeedback();
        }
    }

    public void UpdateEffects()
    {
        foreach (var tile in Grid.AllTiles())
        {
            tile.TickEffect();
        }
    }

    public void TryMovePlayer(Point2 delta)
    {
        if (Mode != GameMode.Running)
        {
            return;
        }

        if (TryMoveActor(Player, delta))
        {
            Tick();
        }
    }

    public void UseItem(int index)
    {
        if (Mode != GameMode.Running || !Inventory.TryConsume(index, out var item) || item is null)
        {
            return;
        }

        item.Use(this);
        _audio.Play("spell");
        Tick();
    }

    public void Tick()
    {
        foreach (var monster in _monsters.ToList())
        {
            if (monster.Dead)
            {
                ResolveDeadMonster(monster);
                _monsters.Remove(monster);
                continue;
            }

            UpdateMonster(monster);
        }

        Player.Shield = Math.Max(0, Player.Shield - 1);
        if (Player.Dead)
        {
            Scores = _scoreboard.AddScore(Scores, Score, false);
            Mode = GameMode.Dead;
            BannerMessage = "You died. Press a movement key to return.";
            return;
        }

        SpawnCounter--;
        if (SpawnCounter <= 0)
        {
            SpawnMonster();
            SpawnCounter = SpawnRate;
            SpawnRate--;
        }

        UpdateEffects();
        ScreenShake();
    }

    public void StartNextTurnMessage(string? message) => BannerMessage = message;

    public void WinRun()
    {
        Scores = _scoreboard.AddScore(Scores, Score, true);
        ShowTitle();
        BannerMessage = "Victory! Press any movement key to play again.";
    }

    public void GainTreasure()
    {
        Score++;
        _audio.Play("treasure");
        SpawnMonster();
    }

    public void QueueShake(int amount) => ShakeAmount = Math.Max(ShakeAmount, amount);

    public void PlaceEffect(Tile tile, EffectKind kind) => tile.SetEffect(kind);

    public void PlaceWorldObject(Tile tile, WorldObject worldObject) => tile.WorldObject = worldObject;

    public bool TryStoreInventoryItem(ItemDefinition item) => Inventory.TryStore(item, InventoryCapacity);

    public void DigAllWalls()
    {
        foreach (var tile in Grid.AllTiles().Where(tile => Grid.IsInBounds(tile.Position.X, tile.Position.Y) && tile.Kind == TileKind.Wall))
        {
            Grid.Replace(tile, TileKind.Floor);
        }
    }

    public void TransformAdjacentWallsToTreasure(Tile origin)
    {
        foreach (var tile in origin.GetAdjacentNeighbors(Grid).Where(tile => Grid.IsInBounds(tile.Position.X, tile.Position.Y) && tile.Kind == TileKind.Wall))
        {
            Grid.Replace(tile, TileKind.Floor);
            tile.WorldObject = new TreasurePickup();
        }
    }

    public void BoltTravel(Point2 direction, EffectKind effect, int damage)
    {
        var current = Player.Tile;
        while (true)
        {
            var next = Grid.GetTile(current.Position.X + direction.X, current.Position.Y + direction.Y);
            if (!next.Passable)
            {
                break;
            }

            current = next;
            if (current.Occupant is { IsPlayer: false } monster)
            {
                DamageMonster(monster, damage);
            }

            current.SetEffect(effect);
        }
    }

    public IReadOnlyList<MonsterActor> GetEnemies() => _monsters;

    public Tile GetRandomPassableTile(Func<Tile, bool>? predicate = null) => Grid.GetRandomPassableTile(_random, predicate);

    public void TeleportActor(MonsterActor actor, Tile tile, int teleportCounter = 2)
    {
        actor.MoveTo(tile);
        actor.TeleportCounter = teleportCounter;
        ResolveStepOn(actor);
    }

    public bool TryMoveActor(MonsterActor actor, Point2 delta)
    {
        if (delta.X == 0 && delta.Y == 0)
        {
            return false;
        }

        var newTile = Grid.GetTile(actor.Tile.Position.X + delta.X, actor.Tile.Position.Y + delta.Y);
        if (!newTile.Passable)
        {
            return false;
        }

        actor.LastMove = delta;
        if (newTile.Occupant is null && newTile.WorldObject?.BlocksMovement == true)
        {
            newTile.WorldObject.Interact(this, actor, newTile);
            return actor.IsPlayer;
        }

        if (newTile.Occupant is null)
        {
            actor.MoveTo(newTile);
            ResolveStepOn(actor);
        }
        else if (actor.IsPlayer != newTile.Occupant.IsPlayer)
        {
            var defender = newTile.Occupant;
            actor.AttackedThisTurn = true;
            actor.StartAttackLunge(delta);
            defender.SetStunned(true);
            DamageMonster(defender, 1 + actor.BonusAttack, delta);
            BannerMessage = DescribeAttack(actor, defender);
            actor.BonusAttack = 0;
            QueueShake(5);

        }
        else
        {
            return false;
        }

        return true;
    }

    public void DamageMonster(MonsterActor monster, float damage)
    {
        DamageMonster(monster, damage, default);
    }

    public void DamageMonster(MonsterActor monster, float damage, Point2 sourceDirection)
    {
        monster.Damage(damage, sourceDirection);
        _audio.Play(monster.IsPlayer ? "hit1" : "hit2");
    }

    private void SpawnMonster()
    {
        var spawnProfile = _currentFloor?.SpawnProfile ?? CurrentDungeon.GetFloor(Math.Clamp(Level, 1, CurrentDungeon.FloorCount)).SpawnProfile;
        var archetype = MonsterCatalog.Get(spawnProfile.PickRandomMonster(_random));
        var monster = new MonsterActor(archetype, Grid.GetRandomPassableTile(_random, tile => tile.WorldObject is null && tile != Player.Tile));
        _monsters.Add(monster);
    }

    private void UpdateMonster(MonsterActor monster)
    {
        monster.TeleportCounter--;
        if (monster.Stunned || monster.TeleportCounter > 0)
        {
            monster.Stunned = false;
            return;
        }

        switch (monster.Kind)
        {
            case MonsterKind.Snake:
                monster.AttackedThisTurn = false;
                MoveMonsterTowardPlayer(monster);
                if (!monster.AttackedThisTurn)
                {
                    MoveMonsterTowardPlayer(monster);
                }
                break;
            case MonsterKind.Tank:
                var startedStunned = monster.Stunned;
                MoveMonsterTowardPlayer(monster);
                if (!startedStunned)
                {
                    monster.Stunned = true;
                }
                break;
            case MonsterKind.Eater:
                var wall = monster.Tile.GetAdjacentNeighbors(Grid).FirstOrDefault(tile => !tile.Passable && Grid.IsInBounds(tile.Position.X, tile.Position.Y));
                if (wall is not null)
                {
                    Grid.Replace(wall, TileKind.Floor);
                    monster.Heal(0.5f);
                }
                else
                {
                    MoveMonsterTowardPlayer(monster);
                }
                break;
            case MonsterKind.Jester:
                var options = monster.Tile.GetAdjacentPassableNeighbors(Grid).Where(tile => tile.Occupant is null || tile.Occupant.IsPlayer).ToList();
                if (options.Count > 0)
                {
                    var target = options[_random.Next(options.Count)];
                    TryMoveActor(monster, new Point2(target.Position.X - monster.Tile.Position.X, target.Position.Y - monster.Tile.Position.Y));
                }
                break;
            default:
                MoveMonsterTowardPlayer(monster);
                break;
        }
    }

    private void MoveMonsterTowardPlayer(MonsterActor monster)
    {
        var options = monster.Tile.GetAdjacentPassableNeighbors(Grid)
            .Where(tile => tile.Occupant is null || tile.Occupant.IsPlayer)
            .OrderBy(tile => tile.DistanceTo(Player.Tile))
            .ToList();

        if (options.Count == 0)
        {
            return;
        }

        var target = options[0];
        TryMoveActor(monster, new Point2(target.Position.X - monster.Tile.Position.X, target.Position.Y - monster.Tile.Position.Y));
    }

    private void ResolveStepOn(MonsterActor actor)
    {
        var tile = actor.Tile;
        tile.WorldObject?.Interact(this, actor, tile);

        if (!ReferenceEquals(actor, Player) || !ReferenceEquals(Player.Tile, tile))
        {
            return;
        }

        switch (tile.Kind)
        {
            case TileKind.Exit when actor.IsPlayer:
                _audio.Play("newLevel");
                if (TryResolveCustomExit())
                {
                    break;
                }

                if (Level == CurrentDungeon.FloorCount)
                {
                    WinRun();
                }
                else
                {
                    Level++;
                    StartLevel(Math.Min(Player.MaxHp, Player.Hp + 1), Inventory.ToItemIds(), Player.MaxHp);
                }
                break;
        }
    }

    private bool TryResolveCustomExit()
    {
        var exitDefinition = _currentFloor?.Exit;
        if (exitDefinition is null)
        {
            return false;
        }

        foreach (var route in exitDefinition.Routes)
        {
            if (!EvaluateExitRoute(route))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(route.GrantsProgressFlag))
            {
                UnlockProgressFlag(route.GrantsProgressFlag);
            }

            if (route.SetsCurrentStart)
            {
                UpdateCurrentStart(route.Destination.DungeonId, route.Destination.FloorNumber, Player.Hp, Player.MaxHp, Inventory.ToItemIds());
            }

            EnterDungeon(route.Destination.DungeonId, route.Destination.FloorNumber);
            BannerMessage = route.Label ?? route.Destination.Label;
            return true;
        }

        return false;
    }

    private string DescribeAttack(MonsterActor attacker, MonsterActor defender)
    {
        var attackerName = attacker.IsPlayer ? "You" : attacker.Kind.ToString();
        var defenderName = defender.IsPlayer ? "you" : defender.Kind.ToString();
        var stunned = defender.Stunned && !defender.Dead ? " stunned" : string.Empty;
        var defeated = defender.Dead ? " down" : string.Empty;
        return $"{attackerName} hit {defenderName}{stunned}{defeated}";
    }

    private void DrawInitialInventory(int count)
    {
        while (Inventory.SlotCount < count)
        {
            Inventory.AddSlot();
        }
    }

    private void ResolveDeadMonster(MonsterActor monster)
    {
        if (monster.DeathDrop is not null && monster.Tile.WorldObject is null)
        {
            monster.Tile.WorldObject = WorldObjectFactory.Create(monster.DeathDrop, ResolveItemDefinition);
        }
    }

    private void SpawnTreasurePickup()
    {
        var tile = Grid.GetRandomPassableTile(_random, tile => tile.WorldObject is null);
        tile.WorldObject = new TreasurePickup();
    }

    private ItemDefinition ResolveItemDefinition(string itemId) => _itemCatalog[itemId];

    private void ScreenShake()
    {
        if (ShakeAmount > 0)
        {
            ShakeAmount--;
        }

        var angle = _random.NextDouble() * Math.PI * 2;
        ShakeOffset = new Point2(
            (int)Math.Round(Math.Cos(angle) * ShakeAmount),
            (int)Math.Round(Math.Sin(angle) * ShakeAmount));
    }
}
