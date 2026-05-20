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
            var exit = level == GameConstants.NumberOfLevels
                ? new ExitDefinition(
                [
                    new ExitRoute(
                        new PortalDestination("hub-success", 1, "Returned with the key"),
                        RequiredItemId: "power",
                        Label: "Returned with the key"),
                    new ExitRoute(
                        new PortalDestination("hub-failure", 1, "Returned empty-handed"),
                        Label: "Returned empty-handed")
                ])
                : null;

            floors.Add(new FloorDefinition(
                $"tutorial-floor-{level}",
                $"Tutorial Floor {level}",
                new ProceduralLevelSource(),
                spawnProfile,
                exit));
        }

        return floors;
    }
}
