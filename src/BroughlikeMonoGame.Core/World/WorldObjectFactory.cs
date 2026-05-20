using System;

namespace BroughlikeMonoGame.Core;

public static class WorldObjectFactory
{
    public static WorldObject Create(WorldObjectDefinition definition, Func<string, ItemDefinition> resolveItemDefinition)
        => definition.Kind switch
        {
            WorldObjectDefinitionKind.Treasure => new TreasurePickup(),
            WorldObjectDefinitionKind.ItemPickup when definition.ItemId is not null => new ItemPickup(resolveItemDefinition(definition.ItemId)),
            WorldObjectDefinitionKind.ItemPickup => throw new InvalidOperationException("Item pickups require an item id."),
            WorldObjectDefinitionKind.Portal when definition.PortalDestination is not null => new PortalWorldObject(definition.PortalDestination, definition.RequiredProgressFlag, definition.GrantsProgressFlag),
            WorldObjectDefinitionKind.Portal => throw new InvalidOperationException("Portals require a destination."),
            _ => throw new InvalidOperationException($"Unsupported world object definition kind '{definition.Kind}'.")
        };
}
