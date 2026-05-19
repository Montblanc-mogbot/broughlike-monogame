using System.Collections.Generic;
using System.Linq;

namespace BroughlikeMonoGame.Core;

public static class SpellBook
{
    public static IReadOnlyList<SpellDefinition> Create() =>
    [
        new("WOOP", session => session.TeleportActor(session.Player, session.GetRandomPassableTile(), 0)),
        new("QUAKE", session =>
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
        new("MAELSTROM", session =>
        {
            foreach (var monster in session.GetEnemies())
            {
                session.TeleportActor(monster, session.GetRandomPassableTile(), 2);
            }
        }),
        new("MULLIGAN", session => session.StartLevel(1, session.PlayerSpells)),
        new("AURA", session =>
        {
            foreach (var tile in session.Player.Tile.GetAdjacentNeighbors(session.Grid))
            {
                session.PlaceEffect(tile, EffectKind.Heal);
                tile.Occupant?.Heal(1);
            }

            session.PlaceEffect(session.Player.Tile, EffectKind.Heal);
            session.Player.Heal(1);
        }),
        new("DASH", session =>
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
        new("DIG", session =>
        {
            session.DigAllWalls();
            session.PlaceEffect(session.Player.Tile, EffectKind.Heal);
            session.Player.Heal(2);
        }),
        new("KINGMAKER", session =>
        {
            foreach (var monster in session.GetEnemies())
            {
                monster.Heal(1);
                monster.Tile.HasTreasure = true;
            }
        }),
        new("ALCHEMY", session => session.TransformAdjacentWallsToTreasure(session.Player.Tile)),
        new("POWER", session => session.Player.BonusAttack = 5),
        new("BUBBLE", session =>
        {
            for (var i = session.PlayerSpells.Count - 1; i > 0; i--)
            {
                if (session.PlayerSpells[i] is null)
                {
                    session.PlayerSpells[i] = session.PlayerSpells[i - 1];
                }
            }
        }),
        new("BRAVERY", session =>
        {
            session.Player.Shield = 2;
            foreach (var monster in session.GetEnemies())
            {
                monster.Stunned = true;
            }
        }),
        new("BOLT", session => session.BoltTravel(session.Player.LastMove, EffectKind.Bolt, 4)),
        new("CROSS", session =>
        {
            session.BoltTravel(new Point2(0, -1), EffectKind.Cross, 2);
            session.BoltTravel(new Point2(0, 1), EffectKind.Cross, 2);
            session.BoltTravel(new Point2(-1, 0), EffectKind.Cross, 2);
            session.BoltTravel(new Point2(1, 0), EffectKind.Cross, 2);
        }),
        new("EX", session =>
        {
            session.BoltTravel(new Point2(-1, -1), EffectKind.Bolt, 3);
            session.BoltTravel(new Point2(-1, 1), EffectKind.Bolt, 3);
            session.BoltTravel(new Point2(1, -1), EffectKind.Bolt, 3);
            session.BoltTravel(new Point2(1, 1), EffectKind.Bolt, 3);
        }),
    ];
}
