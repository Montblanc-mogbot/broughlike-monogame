using System;
using Microsoft.Xna.Framework;

namespace BroughlikeMonoGame.Desktop.Core;

public sealed class MonsterActor
{
    private const int RecentDamageFrames = 12;
    private const int AttackLungeDurationFrames = 8;
    private const int StunPulseDurationFrames = 18;

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

    public int HurtFlashFrames { get; private set; }

    public int DamageTakenThisTurn { get; private set; }

    public Point2 LastDamageDirection { get; private set; }

    public int AttackLungeFrames { get; private set; }

    public Point2 AttackDirection { get; private set; }

    public int StunPulseFrames { get; private set; }

    public void Heal(float amount)
    {
        Hp = MathF.Min(GameConstants.MaxHp, Hp + amount);
    }

    public void TickAnimation()
    {
        OffsetX = ApproachZero(OffsetX, 1f / 8f);
        OffsetY = ApproachZero(OffsetY, 1f / 8f);
        HurtFlashFrames = Math.Max(0, HurtFlashFrames - 1);
        AttackLungeFrames = Math.Max(0, AttackLungeFrames - 1);
        StunPulseFrames = Math.Max(0, StunPulseFrames - 1);
    }

    public void MoveTo(Tile tile)
    {
        if (Tile is not null)
        {
            Tile.Occupant = null;
            OffsetX = Tile.Position.X - tile.Position.X;
            OffsetY = Tile.Position.Y - tile.Position.Y;
        }
        else
        {
            OffsetX = 0f;
            OffsetY = 0f;
        }

        Tile = tile;
        tile.Occupant = this;
    }

    private static float ApproachZero(float value, float step)
    {
        if (MathF.Abs(value) <= step)
        {
            return 0f;
        }

        return value - MathF.Sign(value) * step;
    }

    public void Damage(float amount)
    {
        Damage(amount, default);
    }

    public void Damage(float amount, Point2 sourceDirection)
    {
        if (Shield > 0)
        {
            return;
        }

        HurtFlashFrames = RecentDamageFrames;
        DamageTakenThisTurn = (int)MathF.Ceiling(amount);
        LastDamageDirection = sourceDirection;
        Hp -= amount;
        if (Hp <= 0)
        {
            Dead = true;
            Tile.Occupant = null;
        }
    }

    public void StartAttackLunge(Point2 direction)
    {
        AttackDirection = direction;
        AttackLungeFrames = AttackLungeDurationFrames;
    }

    public void SetStunned(bool stunned)
    {
        Stunned = stunned;
        if (stunned)
        {
            StunPulseFrames = StunPulseDurationFrames;
        }
    }

    public void ClearTurnFeedback()
    {
        DamageTakenThisTurn = 0;
        if (AttackLungeFrames == 0)
        {
            AttackDirection = default;
        }

        if (HurtFlashFrames == 0)
        {
            LastDamageDirection = default;
        }
    }
}
