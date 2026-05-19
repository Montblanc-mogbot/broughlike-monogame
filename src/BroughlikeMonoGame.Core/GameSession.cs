using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BroughlikeMonoGame.Core;

public sealed class GameSession
{
    private readonly Random _random;
    private readonly AudioService _audio;
    private readonly ScoreboardService _scoreboard;
    private readonly List<SpellDefinition> _spellBook;
    private readonly List<MonsterActor> _monsters = [];

    public GameSession(Random random, AudioService audio, ScoreboardService scoreboard, IReadOnlyList<SpellDefinition> spellBook)
    {
        _random = random;
        _audio = audio;
        _scoreboard = scoreboard;
        _spellBook = spellBook.ToList();
        Scores = _scoreboard.Load();
        Mode = GameMode.Title;
        Grid = new LevelGrid(Layout.MapTiles);
    }

    public GameMode Mode { get; private set; }

    public LevelGrid Grid { get; private set; }

    public MonsterActor Player { get; private set; } = null!;

    public IReadOnlyList<MonsterActor> Monsters => _monsters;

    public List<ScoreEntry> Scores { get; private set; }

    public List<string?> PlayerSpells { get; private set; } = [];

    public int Level { get; private set; } = 1;

    public int Score { get; private set; }

    public int SpawnRate { get; private set; }

    public int SpawnCounter { get; private set; }

    public int NumSpells { get; private set; }

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
        Level = 1;
        Score = 0;
        NumSpells = GameConstants.InitialSpellCount;
        StartLevel(GameConstants.StartingHp, null);
        Mode = GameMode.Running;
    }

    public void StartLevel(float playerHp, List<string?>? spells)
    {
        SpawnRate = GameConstants.InitialSpawnRate;
        SpawnCounter = SpawnRate;
        Grid = new LevelGrid(Layout.MapTiles);
        GenerateLevel();
        Player = new MonsterActor(MonsterCatalog.Player, Grid.GetRandomPassableTile(_random), isPlayer: true);
        Player.Heal(playerHp - Player.Hp);
        PlayerSpells = spells is null ? DrawInitialSpells(NumSpells) : spells.ToList();
        PlaceExit();
    }

    public void AdvanceFrame()
    {
        Player.TickAnimation();

        foreach (var monster in _monsters)
        {
            monster.TickAnimation();
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

    public void CastSpell(int index)
    {
        if (Mode != GameMode.Running || index < 0 || index >= PlayerSpells.Count)
        {
            return;
        }

        var spellName = PlayerSpells[index];
        if (spellName is null)
        {
            return;
        }

        PlayerSpells[index] = null;
        var spell = _spellBook.First(definition => definition.Name == spellName);
        spell.Cast(this);
        _audio.Play("spell");
        Tick();
    }

    public void Tick()
    {
        foreach (var monster in _monsters.ToList())
        {
            if (monster.Dead)
            {
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
        if (Score % 3 == 0 && NumSpells < GameConstants.MaxSpells)
        {
            NumSpells++;
            AddSpell();
        }

        _audio.Play("treasure");
        SpawnMonster();
    }

    public void QueueShake(int amount) => ShakeAmount = Math.Max(ShakeAmount, amount);

    public void PlaceEffect(Tile tile, EffectKind kind) => tile.SetEffect(kind);

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
            tile.HasTreasure = true;
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
        var newTile = Grid.GetTile(actor.Tile.Position.X + delta.X, actor.Tile.Position.Y + delta.Y);
        if (!newTile.Passable)
        {
            return false;
        }

        actor.LastMove = delta;
        if (newTile.Occupant is null)
        {
            actor.MoveTo(newTile);
            ResolveStepOn(actor);
        }
        else if (actor.IsPlayer != newTile.Occupant.IsPlayer)
        {
            actor.AttackedThisTurn = true;
            newTile.Occupant.Stunned = true;
            DamageMonster(newTile.Occupant, 1 + actor.BonusAttack);
            actor.BonusAttack = 0;
            QueueShake(5);
        }

        return true;
    }

    public void DamageMonster(MonsterActor monster, float damage)
    {
        monster.Damage(damage);
        _audio.Play(monster.IsPlayer ? "hit1" : "hit2");
    }

    public void AddSpell()
    {
        var newSpell = _spellBook[_random.Next(_spellBook.Count)].Name;
        PlayerSpells.Add(newSpell);
    }

    private void GenerateLevel()
    {
        for (var attempts = 0; attempts < 1000; attempts++)
        {
            var passableTiles = Grid.Generate(_random);
            var candidate = Grid.GetRandomPassableTile(_random);
            if (passableTiles == Grid.CountPassableConnectedFrom(candidate))
            {
                GenerateMonsters();
                for (var i = 0; i < 3; i++)
                {
                    Grid.GetRandomPassableTile(_random).HasTreasure = true;
                }

                return;
            }
        }

        throw new InvalidOperationException("Timed out generating a connected map.");
    }

    private void GenerateMonsters()
    {
        _monsters.Clear();
        for (var i = 0; i < Level + 1; i++)
        {
            SpawnMonster();
        }
    }

    private void SpawnMonster()
    {
        var archetype = MonsterCatalog.Enemies[_random.Next(MonsterCatalog.Enemies.Count)];
        var monster = new MonsterActor(archetype, Grid.GetRandomPassableTile(_random));
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
        switch (tile.Kind)
        {
            case TileKind.Floor when actor.IsPlayer && tile.HasTreasure:
                tile.HasTreasure = false;
                GainTreasure();
                break;
            case TileKind.Exit when actor.IsPlayer:
                _audio.Play("newLevel");
                if (Level == GameConstants.NumberOfLevels)
                {
                    WinRun();
                }
                else
                {
                    Level++;
                    StartLevel(Math.Min(GameConstants.MaxHp, Player.Hp + 1), PlayerSpells);
                }
                break;
        }
    }

    private void PlaceExit()
    {
        var tile = Grid.GetRandomPassableTile(_random, tile => tile != Player.Tile);
        Grid.Replace(tile, TileKind.Exit);
    }

    private List<string?> DrawInitialSpells(int count)
    {
        var spells = _spellBook.Select(spell => spell.Name).OrderBy(_ => _random.Next()).Take(count).Cast<string?>().ToList();
        while (spells.Count < count)
        {
            spells.Add(null);
        }

        return spells;
    }

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
