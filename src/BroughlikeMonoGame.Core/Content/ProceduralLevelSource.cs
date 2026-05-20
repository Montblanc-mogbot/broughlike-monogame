using System;
using System.Collections.Generic;

namespace BroughlikeMonoGame.Core;

public sealed class ProceduralLevelSource : ILevelSource
{
    public LevelPlan Build(Random random, FloorBuildContext context)
    {
        LevelGrid grid;
        for (var attempts = 0; attempts < 1000; attempts++)
        {
            grid = new LevelGrid(context.MapSize);
            var passableTiles = grid.Generate(random);
            var candidate = grid.GetRandomPassableTile(random);
            if (passableTiles == grid.CountPassableConnectedFrom(candidate))
            {
                return BuildPlanFromGrid(grid, random, context);
            }
        }

        throw new InvalidOperationException("Timed out generating a connected map.");
    }

    private static LevelPlan BuildPlanFromGrid(LevelGrid grid, Random random, FloorBuildContext context)
    {
        var reserved = new HashSet<Point2>();
        var playerStart = TakeRandomTile(grid, random, reserved);
        var exitPosition = TakeRandomTile(grid, random, reserved);

        var monsters = new List<MonsterPlacement>();
        for (var i = 0; i < context.SpawnProfile.InitialMonsterCount; i++)
        {
            WorldObjectDefinition? deathDrop = null;
            if (i < context.SpawnProfile.InitialEnemyItemDropCount)
            {
                deathDrop = new WorldObjectDefinition(
                    WorldObjectDefinitionKind.ItemPickup,
                    ItemId: context.SpawnProfile.PickRandomItemId(random));
            }

            monsters.Add(new MonsterPlacement(context.SpawnProfile.PickRandomMonster(random), TakeRandomTile(grid, random, reserved), deathDrop));
        }

        var worldObjects = new List<WorldObjectPlacement>();
        for (var i = 0; i < context.SpawnProfile.InitialTreasureCount; i++)
        {
            worldObjects.Add(new WorldObjectPlacement(new WorldObjectDefinition(WorldObjectDefinitionKind.Treasure), TakeRandomTile(grid, random, reserved)));
        }

        for (var i = 0; i < context.SpawnProfile.InitialFloorItemCount; i++)
        {
            worldObjects.Add(new WorldObjectPlacement(
                new WorldObjectDefinition(
                    WorldObjectDefinitionKind.ItemPickup,
                    ItemId: context.SpawnProfile.PickRandomItemId(random)),
                TakeRandomTile(grid, random, reserved)));
        }

        grid.Replace(grid.GetTile(exitPosition.X, exitPosition.Y), TileKind.Exit);
        return new LevelPlan(grid, playerStart, exitPosition, monsters, worldObjects);
    }

    private static Point2 TakeRandomTile(LevelGrid grid, Random random, HashSet<Point2> reserved)
    {
        for (var attempts = 0; attempts < 1000; attempts++)
        {
            var tile = grid.GetRandomPassableTile(random, candidate => !reserved.Contains(candidate.Position));
            if (reserved.Add(tile.Position))
            {
                return tile.Position;
            }
        }

        throw new InvalidOperationException("Failed to reserve a unique passable tile while building the level plan.");
    }
}
