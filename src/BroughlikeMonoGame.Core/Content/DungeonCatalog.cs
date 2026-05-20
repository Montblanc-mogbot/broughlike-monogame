namespace BroughlikeMonoGame.Core;

public static class DungeonCatalog
{
    public static DungeonRegistry CreateDefaultRegistry()
        => new([TutorialDungeonDefinition.Create()]);
}
