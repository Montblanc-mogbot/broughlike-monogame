using System.Collections.Generic;
using System.Linq;

namespace BroughlikeMonoGame.Core;

public static class ItemCatalog
{
    public static IReadOnlyList<ItemDefinition> CreateTutorialItems() =>
    [
        new("woop", "WOOP", session => session.TeleportActor(session.Player, session.GetRandomPassableTile(), 0)),
        new("quake", "QUAKE", session =>
        {
            foreach (var tile in session.Grid.AllTiles())
            {
                if (tile.Occupant is { IsPlayer: false } monster)
                {
                    var adjacentWalls = 4 - tile.GetAdjacentPassableNeighbors(session.Grid).Count();
                    session.DamageMonster(monster, adjacentWalls * 2);
                }
            }

            session.QueueShake(20);
        }),
        new("maelstrom", "MAELSTROM", session =>
        {
            foreach (var monster in session.GetEnemies())
            {
                session.TeleportActor(monster, session.GetRandomPassableTile(), 2);
            }
        }),
        new("mulligan", "MULLIGAN", session => session.StartLevel(1, session.Inventory.ToItemIds())),
        new("aura", "AURA", session =>
        {
            foreach (var tile in session.Player.Tile.GetAdjacentNeighbors(session.Grid))
            {
                session.PlaceEffect(tile, EffectKind.Heal);
                tile.Occupant?.Heal(1);
            }

            session.PlaceEffect(session.Player.Tile, EffectKind.Heal);
            session.Player.Heal(1);
        }),
        new("dash", "DASH", session =>
        {
            var current = session.Player.Tile;
            while (true)
            {
                var next = session.Grid.GetTile(current.Position.X + session.Player.LastMove.X, current.Position.Y + session.Player.LastMove.Y);
                if (next.Passable && next.Occupant is null)
                {
                    current = next;
                }
                else
                {
                    break;
                }
            }

            if (current != session.Player.Tile)
            {
                session.TeleportActor(session.Player, current, 0);
                foreach (var tile in current.GetAdjacentNeighbors(session.Grid))
                {
                    if (tile.Occupant is { IsPlayer: false } monster)
                    {
                        session.PlaceEffect(tile, EffectKind.Dash);
                        monster.Stunned = true;
                        session.DamageMonster(monster, 1);
                    }
                }
            }
        }),
        new("dig", "DIG", session =>
        {
            session.DigAllWalls();
            session.PlaceEffect(session.Player.Tile, EffectKind.Heal);
            session.Player.Heal(2);
        }),
        new("kingmaker", "KINGMAKER", session =>
        {
            foreach (var monster in session.GetEnemies())
            {
                monster.Heal(1);
                session.PlaceWorldObject(monster.Tile, new TreasurePickup());
            }
        }),
        new("alchemy", "ALCHEMY", session => session.TransformAdjacentWallsToTreasure(session.Player.Tile)),
        new("power", "POWER", session => session.Player.BonusAttack = 5),
        new("bubble", "BUBBLE", session => session.Inventory.CopyBackwardIntoEmptySlots()),
        new("bravery", "BRAVERY", session =>
        {
            session.Player.Shield = 2;
            foreach (var monster in session.GetEnemies())
            {
                monster.Stunned = true;
            }
        }),
        new("bolt", "BOLT", session => session.BoltTravel(session.Player.LastMove, EffectKind.Bolt, 4)),
        new("cross", "CROSS", session =>
        {
            session.BoltTravel(new Point2(0, -1), EffectKind.Cross, 2);
            session.BoltTravel(new Point2(0, 1), EffectKind.Cross, 2);
            session.BoltTravel(new Point2(-1, 0), EffectKind.Cross, 2);
            session.BoltTravel(new Point2(1, 0), EffectKind.Cross, 2);
        }),
        new("ex", "EX", session =>
        {
            session.BoltTravel(new Point2(-1, -1), EffectKind.Bolt, 3);
            session.BoltTravel(new Point2(-1, 1), EffectKind.Bolt, 3);
            session.BoltTravel(new Point2(1, -1), EffectKind.Bolt, 3);
            session.BoltTravel(new Point2(1, 1), EffectKind.Bolt, 3);
        }),
        new("black-suit", "BLACK SUIT", session => { }),
    ];
}
