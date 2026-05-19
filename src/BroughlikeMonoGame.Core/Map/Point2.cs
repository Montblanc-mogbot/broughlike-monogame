namespace BroughlikeMonoGame.Core;

public readonly record struct Point2(int X, int Y)
{
    public static readonly Point2 Zero = new(0, 0);
}
