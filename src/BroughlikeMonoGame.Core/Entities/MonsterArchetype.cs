using Microsoft.Xna.Framework;

namespace BroughlikeMonoGame.Core;

public sealed record MonsterArchetype(MonsterKind Kind, string Name, Color Color, float MaxHp);
