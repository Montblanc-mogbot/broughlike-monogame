using System.Collections.Generic;

namespace BroughlikeMonoGame.Core;

public static class TutorialDungeonFloors
{
    public static IReadOnlyList<FloorDefinition> Create()
    {
        var spawnProfile = TutorialDungeonSpawnProfiles.CreateMainProfile();
        var floors = new List<FloorDefinition>();
        for (var level = 1; level <= GameConstants.NumberOfLevels; level++)
        {
            floors.Add(new FloorDefinition(
                $"tutorial-floor-{level}",
                $"Tutorial Floor {level}",
                new ProceduralLevelSource(),
                spawnProfile));
        }

        return floors;
    }
}
