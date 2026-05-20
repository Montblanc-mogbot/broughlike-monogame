namespace BroughlikeMonoGame.Core;

public sealed record MonsterPlacement(MonsterKind Kind, Point2 Position, WorldObjectDefinition? DeathDrop = null);
