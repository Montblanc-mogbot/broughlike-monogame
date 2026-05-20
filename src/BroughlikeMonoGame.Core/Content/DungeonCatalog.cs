namespace BroughlikeMonoGame.Core;

public static class DungeonCatalog
{
    public const string DefaultStartingDungeonId = "apartment-intro";

    public static DungeonRegistry CreateDefaultRegistry()
        => new([
            ApartmentIntroDefinition.Create(),
            HubStartDefinition.Create(),
            HubSuccessDefinition.Create(),
            HubFailureDefinition.Create(),
            TutorialDungeonDefinition.Create()
        ]);
}
