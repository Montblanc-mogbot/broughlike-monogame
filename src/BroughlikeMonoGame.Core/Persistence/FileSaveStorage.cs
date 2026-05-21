using System.IO;

namespace BroughlikeMonoGame.Core;

public sealed class FileSaveStorage : ISaveStorage
{
    private readonly string _path;

    public FileSaveStorage(string path)
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

    public void Delete()
    {
        if (File.Exists(_path))
        {
            File.Delete(_path);
        }
    }
}
