namespace BroughlikeMonoGame.Core;

public sealed class ScriptedInteractableWorldObject : WorldObject
{
    private readonly string? _message;
    private readonly ItemDefinition? _spawnItem;
    private readonly Point2 _spawnOffset;
    private readonly bool _spawnOnlyOnce;
    private bool _hasSpawned;

    public ScriptedInteractableWorldObject(
        string displayName,
        WorldObjectVisualKind visualKind,
        string? message,
        bool blocksMovement,
        ItemDefinition? spawnItem = null,
        Point2? spawnOffset = null,
        bool spawnOnlyOnce = true) : base(displayName)
    {
        VisualKind = visualKind;
        _message = message;
        BlocksMovement = blocksMovement;
        _spawnItem = spawnItem;
        _spawnOffset = spawnOffset ?? Point2.Zero;
        _spawnOnlyOnce = spawnOnlyOnce;
    }

    public override WorldObjectVisualKind VisualKind { get; }

    public override bool BlocksMovement { get; }

    public override void Interact(GameSession session, MonsterActor actor, Tile tile)
    {
        if (!string.IsNullOrWhiteSpace(_message))
        {
            session.StartNextTurnMessage(_message);
        }

        if (_spawnItem is null)
        {
            return;
        }

        if (_spawnOnlyOnce && _hasSpawned)
        {
            return;
        }

        var target = session.Grid.GetTile(tile.Position.X + _spawnOffset.X, tile.Position.Y + _spawnOffset.Y);
        if (!target.Passable || target.Occupant is not null || target.WorldObject is not null)
        {
            return;
        }

        target.WorldObject = new ItemPickup(_spawnItem);
        _hasSpawned = true;
    }
}
