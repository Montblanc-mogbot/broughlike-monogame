using System.Collections.Generic;
using System.Linq;

namespace BroughlikeMonoGame.Core;

public static class MonsterCatalog
{
    public static readonly MonsterArchetype Player = new(MonsterKind.Player, "Player", Palette.Player, 3);
    public static readonly MonsterArchetype Bird = new(MonsterKind.Bird, "Bird", Palette.Bird, 3);
    public static readonly MonsterArchetype Snake = new(MonsterKind.Snake, "Snake", Palette.Snake, 1);
    public static readonly MonsterArchetype Tank = new(MonsterKind.Tank, "Tank", Palette.Tank, 2);
    public static readonly MonsterArchetype Eater = new(MonsterKind.Eater, "Eater", Palette.Eater, 1);
    public static readonly MonsterArchetype Jester = new(MonsterKind.Jester, "Jester", Palette.Jester, 2);

    public static readonly IReadOnlyList<MonsterArchetype> Enemies =
    [
        Bird,
        Snake,
        Tank,
        Eater,
        Jester,
    ];

    public static MonsterArchetype Get(MonsterKind kind)
        => kind == MonsterKind.Player
            ? Player
            : Enemies.First(archetype => archetype.Kind == kind);
}
