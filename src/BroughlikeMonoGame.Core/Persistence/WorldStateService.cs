using System.Text.Json;

namespace BroughlikeMonoGame.Core;

public sealed class WorldStateService
{
    private readonly ISaveStorage _storage;

    public WorldStateService(ISaveStorage storage)
    {
        _storage = storage;
    }

    public WorldState? Load()
    {
        if (!_storage.Exists())
        {
            return null;
        }

        return JsonSerializer.Deserialize<WorldState>(_storage.ReadAllText() ?? "null");
    }

    public void Save(WorldState state)
    {
        var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
        _storage.WriteAllText(json);
    }
}
