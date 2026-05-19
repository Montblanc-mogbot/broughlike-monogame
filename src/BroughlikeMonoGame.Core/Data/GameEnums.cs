namespace BroughlikeMonoGame.Core;

public enum TileKind
{
    Floor,
    Wall,
    Exit,
}

public enum MonsterKind
{
    Player,
    Bird,
    Snake,
    Tank,
    Eater,
    Jester,
}

public enum EffectKind
{
    Heal,
    Bolt,
    Cross,
    Dash,
}

public enum GameMode
{
    Loading,
    Title,
    Running,
    Dead,
}
