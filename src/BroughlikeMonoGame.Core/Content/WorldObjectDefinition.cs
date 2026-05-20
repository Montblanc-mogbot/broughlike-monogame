namespace BroughlikeMonoGame.Core;

public enum WorldObjectDefinitionKind
{
    Treasure,
    ItemPickup,
    Portal,
}

public sealed record WorldObjectDefinition(
    WorldObjectDefinitionKind Kind,
    string? ItemId = null,
    PortalDestination? PortalDestination = null);
