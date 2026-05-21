using System.Collections.Generic;

namespace BroughlikeMonoGame.Core;

public sealed record WorldState(
    string SlotId,
    WorldStartState CurrentStart,
    WorldPlayerState Player,
    IReadOnlyDictionary<string, bool> StoryFlags,
    SaveGame? ActiveRun,
    IReadOnlyList<string?> StashItemIds,
    IReadOnlyList<string> UnlockedDungeons,
    string? LastCompletedDungeon)
{
    public static WorldState CreateDefault(string slotId, string startingDungeonId)
        => new(
            slotId,
            new WorldStartState(startingDungeonId, 1),
            new WorldPlayerState(
                GameConstants.StartingHp,
                MonsterCatalog.Player.MaxHp,
                CreateEmptyInventory(GameConstants.InitialSpellCount)),
            new Dictionary<string, bool>(),
            null,
            [],
            [startingDungeonId],
            null);

    private static IReadOnlyList<string?> CreateEmptyInventory(int slots)
    {
        var items = new string?[slots];
        return items;
    }
}

public sealed record WorldStartState(string DungeonId, int FloorNumber);

public sealed record WorldPlayerState(
    float CurrentHp,
    float MaxHp,
    IReadOnlyList<string?> InventoryItemIds);
