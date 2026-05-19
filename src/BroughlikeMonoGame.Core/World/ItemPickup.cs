namespace BroughlikeMonoGame.Core;

public sealed class ItemPickup : WorldObject
{
    public ItemPickup(ItemDefinition item) : base(item.DisplayName)
    {
        Item = item;
    }

    public ItemDefinition Item { get; }

    public override WorldObjectVisualKind VisualKind => WorldObjectVisualKind.Item;

    public override void Interact(GameSession session, MonsterActor actor, Tile tile)
    {
        if (!actor.IsPlayer)
        {
            return;
        }

        if (!session.TryStoreInventoryItem(Item))
        {
            session.StartNextTurnMessage($"No room for {Item.DisplayName}");
            return;
        }

        tile.WorldObject = null;
        session.StartNextTurnMessage($"Picked up {Item.DisplayName}");
    }
}
