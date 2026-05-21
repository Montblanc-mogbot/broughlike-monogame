using System;
using System.Collections.Generic;
using System.Linq;

namespace BroughlikeMonoGame.Core;

public static class ContentValidator
{
    public static IReadOnlyList<string> Validate(DungeonRegistry registry, IReadOnlyList<ItemDefinition> items, string? startingDungeonId = null)
    {
        var errors = new List<string>();
        var itemIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.Id))
            {
                errors.Add("Item catalog contains an item with a blank id.");
                continue;
            }

            if (!itemIds.Add(item.Id))
            {
                errors.Add($"Duplicate item id '{item.Id}'.");
            }
        }

        if (!string.IsNullOrWhiteSpace(startingDungeonId) && !registry.Contains(startingDungeonId))
        {
            errors.Add($"Starting dungeon '{startingDungeonId}' does not exist in the registry.");
        }

        foreach (var dungeon in registry.Dungeons)
        {
            ValidateDungeon(dungeon, registry, itemIds, errors);
        }

        return errors;
    }

    public static void ThrowIfInvalid(DungeonRegistry registry, IReadOnlyList<ItemDefinition> items, string? startingDungeonId = null)
    {
        var errors = Validate(registry, items, startingDungeonId);
        if (errors.Count == 0)
        {
            return;
        }

        throw new InvalidOperationException("Content validation failed:" + Environment.NewLine + string.Join(Environment.NewLine, errors.Select(error => $"- {error}")));
    }

    private static void ValidateDungeon(DungeonDefinition dungeon, DungeonRegistry registry, HashSet<string> itemIds, List<string> errors)
    {
        for (var floorNumber = 1; floorNumber <= dungeon.FloorCount; floorNumber++)
        {
            var floor = dungeon.GetFloor(floorNumber);
            var floorLabel = $"dungeon '{dungeon.Id}' floor {floorNumber}";

            ValidateSpawnProfile(floorLabel, floor.SpawnProfile, itemIds, errors);
            ValidateExitDefinition(floorLabel, floor.Exit, registry, itemIds, errors);
            ValidateBuiltLevelPlan(dungeon, floorNumber, floor, registry, itemIds, errors);
        }
    }

    private static void ValidateSpawnProfile(string floorLabel, SpawnProfile profile, HashSet<string> itemIds, List<string> errors)
    {
        if ((profile.InitialFloorItemCount > 0 || profile.InitialEnemyItemDropCount > 0) && profile.ItemTable.Count == 0)
        {
            errors.Add($"{floorLabel} spawns items but has no item table configured.");
        }

        foreach (var entry in profile.ItemTable)
        {
            if (!itemIds.Contains(entry.Value))
            {
                errors.Add($"{floorLabel} item table references unknown item '{entry.Value}'.");
            }
        }
    }

    private static void ValidateExitDefinition(string floorLabel, ExitDefinition? exit, DungeonRegistry registry, HashSet<string> itemIds, List<string> errors)
    {
        if (exit is null)
        {
            return;
        }

        if (exit.Routes.Count == 0)
        {
            errors.Add($"{floorLabel} defines an exit with no routes.");
            return;
        }

        foreach (var route in exit.Routes)
        {
            ValidatePortalDestination(route.Destination, $"{floorLabel} exit route", registry, errors);

            if (!string.IsNullOrWhiteSpace(route.RequiredItemId) && !itemIds.Contains(route.RequiredItemId))
            {
                errors.Add($"{floorLabel} exit route requires unknown item '{route.RequiredItemId}'.");
            }
        }
    }

    private static void ValidateBuiltLevelPlan(DungeonDefinition dungeon, int floorNumber, FloorDefinition floor, DungeonRegistry registry, HashSet<string> itemIds, List<string> errors)
    {
        var floorLabel = $"dungeon '{dungeon.Id}' floor {floorNumber}";
        LevelPlan plan;
        try
        {
            plan = floor.LevelSource.Build(
                new Random(0),
                new FloorBuildContext(floorNumber, Layout.MapTiles, floor.SpawnProfile, id => new ItemDefinition(id, id, _ => { })));
        }
        catch (Exception ex)
        {
            errors.Add($"{floorLabel} failed to build: {ex.Message}");
            return;
        }

        ValidatePoint(plan.PlayerStart, plan.Grid, $"{floorLabel} player start", errors);
        ValidatePoint(plan.ExitPosition, plan.Grid, $"{floorLabel} exit position", errors);

        foreach (var monster in plan.Monsters)
        {
            ValidatePoint(monster.Position, plan.Grid, $"{floorLabel} monster '{monster.Kind}'", errors);
            if (monster.DeathDrop is not null)
            {
                ValidateWorldObject(monster.DeathDrop, $"{floorLabel} death drop for '{monster.Kind}'", registry, itemIds, errors);
            }
        }

        foreach (var worldObject in plan.WorldObjects)
        {
            ValidatePoint(worldObject.Position, plan.Grid, $"{floorLabel} world object '{worldObject.Definition.Kind}'", errors);
            ValidateWorldObject(worldObject.Definition, $"{floorLabel} world object at {worldObject.Position.X},{worldObject.Position.Y}", registry, itemIds, errors);
        }
    }

    private static void ValidateWorldObject(WorldObjectDefinition definition, string label, DungeonRegistry registry, HashSet<string> itemIds, List<string> errors)
    {
        switch (definition.Kind)
        {
            case WorldObjectDefinitionKind.ItemPickup:
                if (string.IsNullOrWhiteSpace(definition.ItemId))
                {
                    errors.Add($"{label} is an item pickup with no item id.");
                }
                else if (!itemIds.Contains(definition.ItemId))
                {
                    errors.Add($"{label} references unknown item '{definition.ItemId}'.");
                }
                break;
            case WorldObjectDefinitionKind.Portal:
                if (definition.PortalDestination is null)
                {
                    errors.Add($"{label} is a portal with no destination.");
                }
                else
                {
                    ValidatePortalDestination(definition.PortalDestination, label, registry, errors);
                }
                break;
            case WorldObjectDefinitionKind.ScriptedInteractable:
                if (string.IsNullOrWhiteSpace(definition.DisplayName))
                {
                    errors.Add($"{label} is a scripted interactable with no display name.");
                }
                if (!string.IsNullOrWhiteSpace(definition.ItemId) && !itemIds.Contains(definition.ItemId))
                {
                    errors.Add($"{label} references unknown item '{definition.ItemId}'.");
                }
                break;
        }
    }

    private static void ValidatePortalDestination(PortalDestination destination, string label, DungeonRegistry registry, List<string> errors)
    {
        if (!registry.Contains(destination.DungeonId))
        {
            errors.Add($"{label} points to unknown dungeon '{destination.DungeonId}'.");
            return;
        }

        var targetDungeon = registry.Get(destination.DungeonId);
        if (destination.FloorNumber < 1 || destination.FloorNumber > targetDungeon.FloorCount)
        {
            errors.Add($"{label} points to impossible floor {destination.FloorNumber} in dungeon '{destination.DungeonId}'.");
        }
    }

    private static void ValidatePoint(Point2 point, LevelGrid grid, string label, List<string> errors)
    {
        if (!grid.IsInBounds(point.X, point.Y))
        {
            errors.Add($"{label} is out of bounds at {point.X},{point.Y}.");
            return;
        }

        if (!grid.GetTile(point.X, point.Y).Passable)
        {
            errors.Add($"{label} is placed on an impassable tile at {point.X},{point.Y}.");
        }
    }
}
