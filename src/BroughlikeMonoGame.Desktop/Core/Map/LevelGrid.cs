using System;
using System.Collections.Generic;
using System.Linq;

namespace BroughlikeMonoGame.Desktop.Core;

public sealed class LevelGrid
{
    private readonly Tile[,] _tiles;
    private readonly Tile _voidWall = new(-1, -1, TileKind.Wall);

    public LevelGrid(int size)
    {
        Size = size;
        _tiles = new Tile[size, size];
    }

    public int Size { get; }

    public Tile GetTile(int x, int y)
    {
        if (!IsInBounds(x, y))
        {
            return _voidWall;
        }

        return _tiles[x, y];
    }

    public bool IsInBounds(int x, int y)
        => x > 0 && y > 0 && x < Size - 1 && y < Size - 1;

    public IEnumerable<Tile> AllTiles()
    {
        for (var x = 0; x < Size; x++)
        {
            for (var y = 0; y < Size; y++)
            {
                yield return _tiles[x, y];
            }
        }
    }

    public int Generate(Random random)
    {
        var passableCount = 0;
        for (var x = 0; x < Size; x++)
        {
            for (var y = 0; y < Size; y++)
            {
                var kind = random.NextDouble() < 0.3 || !IsInBounds(x, y)
                    ? TileKind.Wall
                    : TileKind.Floor;
                _tiles[x, y] = new Tile(x, y, kind);
                if (kind == TileKind.Floor)
                {
                    passableCount++;
                }
            }
        }

        return passableCount;
    }

    public Tile GetRandomPassableTile(Random random, Func<Tile, bool>? predicate = null)
    {
        for (var attempts = 0; attempts < 1000; attempts++)
        {
            var tile = GetTile(random.Next(0, Size), random.Next(0, Size));
            if (tile.Passable && tile.Occupant is null && (predicate is null || predicate(tile)))
            {
                return tile;
            }
        }

        throw new InvalidOperationException("Failed to find a passable tile.");
    }

    public void Replace(Tile tile, TileKind kind)
    {
        tile.SetKind(kind);
        tile.HasTreasure = false;
    }

    public int CountPassableConnectedFrom(Tile tile) => tile.GetConnectedPassableTiles(this).Count;
}
