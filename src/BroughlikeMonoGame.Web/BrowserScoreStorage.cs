namespace BroughlikeMonoGame.Web;

using BroughlikeMonoGame.Core;

internal sealed class BrowserScoreStorage : IScoreStorage
{
    private static string? _scoresJson;

    public bool Exists() => !string.IsNullOrWhiteSpace(_scoresJson);

    public string? ReadAllText() => _scoresJson;

    public void WriteAllText(string content) => _scoresJson = content;
}
