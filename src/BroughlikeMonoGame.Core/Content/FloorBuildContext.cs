using System;

namespace BroughlikeMonoGame.Core;

public sealed class FloorBuildContext
{
    public FloorBuildContext(int levelNumber, int mapSize, SpawnProfile spawnProfile, Func<string, ItemDefinition> resolveItemDefinition)
    {
        LevelNumber = levelNumber;
        MapSize = mapSize;
        SpawnProfile = spawnProfile;
        ResolveItemDefinition = resolveItemDefinition;
    }

    public int LevelNumber { get; }

    public int MapSize { get; }

    public SpawnProfile SpawnProfile { get; }

    public Func<string, ItemDefinition> ResolveItemDefinition { get; }
}
