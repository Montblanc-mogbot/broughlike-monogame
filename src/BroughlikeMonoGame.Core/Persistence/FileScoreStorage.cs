using System.IO;

namespace BroughlikeMonoGame.Core;

public sealed class FileScoreStorage : IScoreStorage
{
    private readonly string _path;

    public FileScoreStorage(string path)
    {
        _path = path;
    }

    public bool Exists() => File.Exists(_path);

    public string? ReadAllText() => File.Exists(_path) ? File.ReadAllText(_path) : null;

    public void WriteAllText(string content)
    {
        var directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(_path, content);
    }
}
