using System;
using Microsoft.Xna.Framework;

namespace BroughlikeMonoGame.Core;

public sealed class MonsterActor
{
    public MonsterActor(MonsterArchetype archetype, Tile tile, bool isPlayer = false)
    {
        Archetype = archetype;
        IsPlayer = isPlayer;
        MaxHp = archetype.MaxHp;
        Hp = archetype.MaxHp;
        TeleportCounter = isPlayer ? 0 : 2;
        LastMove = new Point2(-1, 0);
        MoveTo(tile);
    }

    public MonsterArchetype Archetype { get; }

    public MonsterKind Kind => Archetype.Kind;

    public bool IsPlayer { get; }

    public float MaxHp { get; }

    public float Hp { get; private set; }

    public Tile Tile { get; private set; } = null!;

    public int TeleportCounter { get; set; }

    public bool Stunned { get; set; }

    public bool Dead { get; private set; }

    public float OffsetX { get; private set; }

    public float OffsetY { get; private set; }

    public Point2 LastMove { get; set; }

    public int BonusAttack { get; set; }

    public int Shield { get; set; }

    public bool AttackedThisTurn { get; set; }

    public void Heal(float amount)
    {
        Hp = MathF.Min(GameConstants.MaxHp, Hp + amount);
    }

    public void TickAnimation()
    {
        OffsetX -= MathF.Sign(OffsetX) * (1f / 8f);
        OffsetY -= MathF.Sign(OffsetY) * (1f / 8f);
    }

    public void MoveTo(Tile tile)
    {
        if (Tile is not null)
        {
            Tile.Occupant = null;
            OffsetX = Tile.Position.X - tile.Position.X;
            OffsetY = Tile.Position.Y - tile.Position.Y;
        }

        Tile = tile;
        tile.Occupant = this;
    }

    public void Damage(float amount)
    {
        if (Shield > 0)
        {
            return;
        }

        Hp -= amount;
        if (Hp <= 0)
        {
            Dead = true;
            Tile.Occupant = null;
        }
    }
}
