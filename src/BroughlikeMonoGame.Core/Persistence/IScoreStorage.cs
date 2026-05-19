namespace BroughlikeMonoGame.Core;

public interface IScoreStorage
{
    bool Exists();
    string? ReadAllText();
    void WriteAllText(string content);
}
