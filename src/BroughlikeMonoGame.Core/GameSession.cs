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

    public string LastInputDebug { get; private set; } = "none";

    public string LastRawInputDebug { get; private set; } = "curr=none prev=none";

    public string LastPlayerActionDebug { get; private set; } = "boot";

    public string LastEnemyActionDebug { get; private set; } = "none";

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
            LastPlayerActionDebug = "ignored: mode not running";
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

    public void RecordInputDebug(string message) => LastInputDebug = message;

    public void RecordRawInputDebug(string message) => LastRawInputDebug = message;

    public IReadOnlyList<string> GetDebugLines()
    {
        if (Player is null)
        {
            return [
                $"Input: {LastInputDebug}",
                $"Raw: {LastRawInputDebug}",
                $"PlayerAction: {LastPlayerActionDebug}",
                $"EnemyAction: {LastEnemyActionDebug}",
                "Player: not spawned"
            ];
        }

        var lines = new List<string>
        {
            $"Input: {LastInputDebug}",
            $"Raw: {LastRawInputDebug}",
            $"PlayerAction: {LastPlayerActionDebug}",
            $"EnemyAction: {LastEnemyActionDebug}",
            $"Player: ({Player.Tile.Position.X},{Player.Tile.Position.Y}) HP {MathF.Ceiling(Player.Hp)} L({Player.LastMove.X},{Player.LastMove.Y})"
        };

        foreach (var enemy in _monsters
                     .Where(monster => !monster.Dead)
                     .OrderBy(monster => monster.Tile.DistanceTo(Player.Tile))
                     .Take(3))
        {
            lines.Add($"{enemy.Archetype.Name}: ({enemy.Tile.Position.X},{enemy.Tile.Position.Y}) HP {MathF.Ceiling(enemy.Hp)} {(enemy.Stunned ? "stun" : "live")}");
        }

        return lines;
    }

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
        if (delta.X == 0 && delta.Y == 0)
        {
            if (actor.IsPlayer)
            {
                LastPlayerActionDebug = "rejected: zero delta";
            }
            return false;
        }

        var newTile = Grid.GetTile(actor.Tile.Position.X + delta.X, actor.Tile.Position.Y + delta.Y);
        if (!newTile.Passable)
        {
            if (actor.IsPlayer)
            {
                LastPlayerActionDebug = $"blocked: wall at ({newTile.Position.X},{newTile.Position.Y})";
            }
            return false;
        }

        actor.LastMove = delta;
        if (newTile.Occupant is null)
        {
            actor.MoveTo(newTile);
            ResolveStepOn(actor);
            if (actor.IsPlayer)
            {
                LastPlayerActionDebug = $"move -> ({newTile.Position.X},{newTile.Position.Y})";
            }
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

            if (actor.IsPlayer)
            {
                LastPlayerActionDebug = $"attack {defender.Archetype.Name} @({newTile.Position.X},{newTile.Position.Y}) hp {MathF.Max(0, defender.Hp):0.#}";
            }
            else if (defender.IsPlayer)
            {
                LastEnemyActionDebug = $"hit by {actor.Archetype.Name} from ({actor.Tile.Position.X},{actor.Tile.Position.Y})";
            }
        }
        else
        {
            if (actor.IsPlayer)
            {
                LastPlayerActionDebug = $"blocked: ally at ({newTile.Position.X},{newTile.Position.Y})";
            }
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

    private string DescribeAttack(MonsterActor attacker, MonsterActor defender)
    {
        var attackerName = attacker.IsPlayer ? "You" : attacker.Kind.ToString();
        var defenderName = defender.IsPlayer ? "you" : defender.Kind.ToString();
        var stunned = defender.Stunned && !defender.Dead ? " stunned" : string.Empty;
        var defeated = defender.Dead ? " down" : string.Empty;
        return $"{attackerName} hit {defenderName}{stunned}{defeated}";
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
