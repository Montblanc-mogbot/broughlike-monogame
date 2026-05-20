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
            _ => throw new InvalidOperationException($"Unsupported world object definition kind '{definition.Kind}'.")
        };
}
