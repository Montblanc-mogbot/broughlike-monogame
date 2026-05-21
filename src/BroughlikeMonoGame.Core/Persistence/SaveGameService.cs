using System.Text.Json;

namespace BroughlikeMonoGame.Core;

public sealed class SaveGameService
{
    private readonly ISaveStorage _storage;

    public SaveGameService(ISaveStorage storage)
    {
        _storage = storage;
    }

    public SaveGame? Load()
    {
        if (!_storage.Exists())
        {
            return null;
        }

        return JsonSerializer.Deserialize<SaveGame>(_storage.ReadAllText() ?? "null");
    }

    public void Save(SaveGame saveGame)
    {
        var json = JsonSerializer.Serialize(saveGame, new JsonSerializerOptions { WriteIndented = true });
        _storage.WriteAllText(json);
    }

    public void Clear() => _storage.Delete();
}
