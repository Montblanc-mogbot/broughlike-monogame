using System.Collections.Generic;
using System.Linq;

namespace BroughlikeMonoGame.Desktop.Core;

public sealed class Tile
{
    public Tile(int x, int y, TileKind kind)
    {
        Position = new Point2(x, y);
        Kind = kind;
    }

    public Point2 Position { get; }

    public TileKind Kind { get; private set; }

    public bool Passable => Kind != TileKind.Wall;

    public bool HasTreasure { get; set; }

    public TileEffect? Effect { get; private set; }

    public MonsterActor? Occupant { get; set; }

    public void SetKind(TileKind kind) => Kind = kind;

    public int DistanceTo(Tile other) => System.Math.Abs(Position.X - other.Position.X) + System.Math.Abs(Position.Y - other.Position.Y);

    public IEnumerable<Tile> GetAdjacentNeighbors(LevelGrid grid)
    {
        yield return grid.GetTile(Position.X, Position.Y - 1);
        yield return grid.GetTile(Position.X, Position.Y + 1);
        yield return grid.GetTile(Position.X - 1, Position.Y);
        yield return grid.GetTile(Position.X + 1, Position.Y);
    }

    public IEnumerable<Tile> GetAdjacentPassableNeighbors(LevelGrid grid)
        => GetAdjacentNeighbors(grid).Where(tile => tile.Passable);

    public IReadOnlyList<Tile> GetConnectedPassableTiles(LevelGrid grid)
    {
        var connected = new List<Tile> { this };
        var frontier = new Stack<Tile>();
        frontier.Push(this);

        while (frontier.Count > 0)
        {
            var current = frontier.Pop();
            foreach (var neighbor in current.GetAdjacentPassableNeighbors(grid).Where(tile => !connected.Contains(tile)))
            {
                connected.Add(neighbor);
                frontier.Push(neighbor);
            }
        }

        return connected;
    }

    public void SetEffect(EffectKind kind)
    {
        Effect = new TileEffect(kind, GameConstants.EffectDurationTurns);
    }

    public void TickEffect()
    {
        Effect?.Tick();
        if (Effect?.IsExpired == true)
        {
            Effect = null;
        }
    }
}
