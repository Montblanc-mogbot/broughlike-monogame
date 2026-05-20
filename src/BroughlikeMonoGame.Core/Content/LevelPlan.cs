using System.Collections.Generic;

namespace BroughlikeMonoGame.Core;

public sealed class LevelPlan
{
    public LevelPlan(LevelGrid grid, Point2 playerStart, Point2 exitPosition, IReadOnlyList<MonsterPlacement> monsters, IReadOnlyList<WorldObjectPlacement> worldObjects)
    {
        Grid = grid;
        PlayerStart = playerStart;
        ExitPosition = exitPosition;
        Monsters = monsters;
        WorldObjects = worldObjects;
    }

    public LevelGrid Grid { get; }

    public Point2 PlayerStart { get; }

    public Point2 ExitPosition { get; }

    public IReadOnlyList<MonsterPlacement> Monsters { get; }

    public IReadOnlyList<WorldObjectPlacement> WorldObjects { get; }
}
