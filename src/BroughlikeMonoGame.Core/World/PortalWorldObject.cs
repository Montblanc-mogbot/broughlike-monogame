namespace BroughlikeMonoGame.Core;

public sealed class PortalWorldObject : WorldObject
{
    public PortalWorldObject(PortalDestination destination) : base(destination.Label ?? $"Portal to {destination.DungeonId}")
    {
        Destination = destination;
    }

    public PortalDestination Destination { get; }

    public override WorldObjectVisualKind VisualKind => WorldObjectVisualKind.Portal;

    public override void Interact(GameSession session, MonsterActor actor, Tile tile)
    {
        if (!actor.IsPlayer)
        {
            return;
        }

        session.EnterDungeon(Destination.DungeonId, Destination.FloorNumber);
        session.StartNextTurnMessage(Destination.Label ?? $"Entered {Destination.DungeonId}");
    }
}
