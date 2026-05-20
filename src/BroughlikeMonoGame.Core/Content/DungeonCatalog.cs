namespace BroughlikeMonoGame.Core;

public static class DungeonCatalog
{
    public const string DefaultStartingDungeonId = "hub-start";

    public static DungeonRegistry CreateDefaultRegistry()
        => new([
            HubStartDefinition.Create(),
            HubSuccessDefinition.Create(),
            HubFailureDefinition.Create(),
            TutorialDungeonDefinition.Create()
        ]);
}
