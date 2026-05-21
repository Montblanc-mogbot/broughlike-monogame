namespace BroughlikeMonoGame.Core;

public interface ISaveStorage
{
    bool Exists();

    string? ReadAllText();

    void WriteAllText(string content);

    void Delete();
}
