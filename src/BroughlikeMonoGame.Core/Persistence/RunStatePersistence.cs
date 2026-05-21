namespace BroughlikeMonoGame.Core;

public sealed class RunStatePersistence
{
    private readonly SaveGameService _saveGames;

    public RunStatePersistence(SaveGameService saveGames)
    {
        _saveGames = saveGames;
    }

    public void Initialize(GameSession session)
    {
        var save = _saveGames.Load();
        if (save is null)
        {
            session.ShowTitle();
            return;
        }

        session.LoadSaveGame(save);
    }

    public void Sync(GameSession session)
    {
        if (session.Mode == GameMode.Running)
        {
            _saveGames.Save(session.CreateSaveGame());
            return;
        }

        _saveGames.Clear();
    }
}
