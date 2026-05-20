namespace BroughlikeMonoGame.Core;

public enum WorldObjectDefinitionKind
{
    Treasure,
    ItemPickup,
    Portal,
    ScriptedInteractable,
}

public sealed record WorldObjectDefinition(
    WorldObjectDefinitionKind Kind,
    string? ItemId = null,
    PortalDestination? PortalDestination = null,
    string? RequiredProgressFlag = null,
    string? GrantsProgressFlag = null,
    string? DisplayName = null,
    string? Message = null,
    WorldObjectVisualKind VisualKind = WorldObjectVisualKind.Item,
    bool BlocksMovement = false,
    Point2? SpawnItemOffset = null);
