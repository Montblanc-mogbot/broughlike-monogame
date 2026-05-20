namespace BroughlikeMonoGame.Core;

public static class TutorialDungeonDefinition
{
    public static DungeonDefinition Create()
        => new("tutorial", "Tutorial Dungeon", TutorialDungeonFloors.Create());
}
