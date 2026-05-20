namespace BroughlikeMonoGame.Core;

public sealed class PortalWorldObject : WorldObject
{
    public PortalWorldObject(PortalDestination destination, string? requiredProgressFlag = null, string? grantsProgressFlag = null)
        : base(destination.Label ?? $"Portal to {destination.DungeonId}")
    {
        Destination = destination;
        RequiredProgressFlag = requiredProgressFlag;
        GrantsProgressFlag = grantsProgressFlag;
    }

    public PortalDestination Destination { get; }

    public string? RequiredProgressFlag { get; }

    public string? GrantsProgressFlag { get; }

    public override WorldObjectVisualKind VisualKind => WorldObjectVisualKind.Portal;

    public override void Interact(GameSession session, MonsterActor actor, Tile tile)
    {
        if (!actor.IsPlayer)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(RequiredProgressFlag) && !session.HasProgressFlag(RequiredProgressFlag))
        {
            session.StartNextTurnMessage($"{DisplayName} is sealed.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(GrantsProgressFlag))
        {
            session.UnlockProgressFlag(GrantsProgressFlag);
        }

        session.EnterDungeon(Destination.DungeonId, Destination.FloorNumber);
        session.StartNextTurnMessage(Destination.Label ?? $"Entered {Destination.DungeonId}");
    }
}
