namespace BroughlikeMonoGame.Core;

public sealed class FloorDefinition
{
    public FloorDefinition(string id, string displayName, ILevelSource levelSource, SpawnProfile spawnProfile, ExitDefinition? exit = null)
    {
        Id = id;
        DisplayName = displayName;
        LevelSource = levelSource;
        SpawnProfile = spawnProfile;
        Exit = exit;
    }

    public string Id { get; }

    public string DisplayName { get; }

    public ILevelSource LevelSource { get; }

    public SpawnProfile SpawnProfile { get; }

    public ExitDefinition? Exit { get; }
}
