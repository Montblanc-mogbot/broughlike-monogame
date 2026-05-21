namespace BroughlikeMonoGame.Web;

using BroughlikeMonoGame.Core;

internal sealed class BrowserSaveStorage : ISaveStorage
{
    private static string? _saveJson;

    public bool Exists() => !string.IsNullOrWhiteSpace(_saveJson);

    public string? ReadAllText() => _saveJson;

    public void WriteAllText(string content) => _saveJson = content;

    public void Delete() => _saveJson = null;
}
