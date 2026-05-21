namespace BroughlikeMonoGame.Core;

public static class ToolkitSampleDefinition
{
    public static DungeonDefinition Create()
        => new("toolkit-sample", "Toolkit Sample", ToolkitSampleFloors.Create(), seedsRandomStartingInventory: false);
}
