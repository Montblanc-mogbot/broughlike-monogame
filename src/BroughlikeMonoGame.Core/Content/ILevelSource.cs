using System;

namespace BroughlikeMonoGame.Core;

public interface ILevelSource
{
    LevelPlan Build(Random random, FloorBuildContext context);
}
