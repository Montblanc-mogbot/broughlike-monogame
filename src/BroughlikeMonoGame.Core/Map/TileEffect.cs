namespace BroughlikeMonoGame.Core;

public sealed class TileEffect
{
    public TileEffect(EffectKind kind, int remainingTurns)
    {
        Kind = kind;
        RemainingTurns = remainingTurns;
    }

    public EffectKind Kind { get; }

    public int RemainingTurns { get; private set; }

    public float Alpha => RemainingTurns / (float)GameConstants.EffectDurationTurns;

    public void Tick()
    {
        if (RemainingTurns > 0)
        {
            RemainingTurns--;
        }
    }

    public bool IsExpired => RemainingTurns <= 0;
}
