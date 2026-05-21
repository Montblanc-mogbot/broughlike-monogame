using System.Collections.Generic;

namespace BroughlikeMonoGame.Core;

public sealed record SaveGame(
    string DungeonId,
    int FloorNumber,
    float PlayerHp,
    float PlayerMaxHp,
    int Score,
    int InventoryCapacity,
    IReadOnlyList<string?> InventoryItemIds,
    IReadOnlyList<string> ProgressFlags);
