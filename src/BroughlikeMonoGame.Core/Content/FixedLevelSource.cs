using System;
using System.Collections.Generic;

namespace BroughlikeMonoGame.Core;

public sealed class FixedLevelSource : ILevelSource
{
    private readonly string[] _rows;
    private readonly IReadOnlyList<MonsterPlacement> _monsters;
    private readonly IReadOnlyList<WorldObjectPlacement> _worldObjects;

    public FixedLevelSource(string[] rows, IReadOnlyList<MonsterPlacement>? monsters = null, IReadOnlyList<WorldObjectPlacement>? worldObjects = null)
    {
        if (rows.Length == 0)
        {
            throw new ArgumentException("Fixed levels require at least one row.", nameof(rows));
        }

        var width = rows[0].Length;
        if (width == 0 || rows.Length != width)
        {
            throw new ArgumentException("Fixed levels must currently be square and non-empty.", nameof(rows));
        }

        for (var i = 1; i < rows.Length; i++)
        {
            if (rows[i].Length != width)
            {
                throw new ArgumentException("All fixed level rows must be the same width.", nameof(rows));
            }
        }

        _rows = rows;
        _monsters = monsters ?? [];
        _worldObjects = worldObjects ?? [];
    }

    public LevelPlan Build(Random random, FloorBuildContext context)
    {
        var grid = new LevelGrid(_rows.Length);
        Point2? playerStart = null;
        Point2? exitPosition = null;

        for (var y = 0; y < _rows.Length; y++)
        {
            for (var x = 0; x < _rows[y].Length; x++)
            {
                var marker = _rows[y][x];
                var position = new Point2(x, y);
                var kind = marker switch
                {
                    '#' => TileKind.Wall,
                    '.' => TileKind.Floor,
                    '@' => TileKind.Floor,
                    '>' => TileKind.Exit,
                    _ => throw new InvalidOperationException($"Unsupported fixed-level marker '{marker}' at {x},{y}.")
                };

                grid.SetTile(x, y, kind);
                if (marker == '@')
                {
                    playerStart = position;
                }
                else if (marker == '>')
                {
                    exitPosition = position;
                }
            }
        }

        if (playerStart is null || exitPosition is null)
        {
            throw new InvalidOperationException("Fixed levels must define both a player start '@' and an exit '>'.");
        }

        return new LevelPlan(grid, playerStart.Value, exitPosition.Value, _monsters, _worldObjects);
    }
}
