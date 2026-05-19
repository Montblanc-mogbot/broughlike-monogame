namespace BroughlikeMonoGame.Core;

public sealed class TreasurePickup : WorldObject
{
    public TreasurePickup() : base("Treasure")
    {
    }

    public override WorldObjectVisualKind VisualKind => WorldObjectVisualKind.Treasure;

    public override void Interact(GameSession session, MonsterActor actor, Tile tile)
    {
        if (!actor.IsPlayer)
        {
            return;
        }

        tile.WorldObject = null;
        session.GainTreasure();
    }
}
