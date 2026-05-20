namespace BroughlikeMonoGame.Core;

public abstract class WorldObject
{
    protected WorldObject(string displayName)
    {
        DisplayName = displayName;
    }

    public string DisplayName { get; }

    public abstract WorldObjectVisualKind VisualKind { get; }

    public virtual bool BlocksMovement => false;

    public virtual void Interact(GameSession session, MonsterActor actor, Tile tile)
    {
    }
}

public enum WorldObjectVisualKind
{
    Treasure,
    Item,
    Portal,
    Npc,
    Bed,
    Dresser,
}
