namespace BroughlikeMonoGame.Core;

public enum WorldObjectDefinitionKind
{
    Treasure,
    ItemPickup,
}

public sealed record WorldObjectDefinition(WorldObjectDefinitionKind Kind, string? ItemId = null);
