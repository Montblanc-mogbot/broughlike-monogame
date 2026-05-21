using System.Linq;

namespace BroughlikeMonoGame.Core;

public sealed class RunStatePersistence
{
    private readonly WorldStateService _worldStates;
    private readonly string _slotId;
    private WorldState? _worldState;

    public RunStatePersistence(WorldStateService worldStates, string slotId = "slot-1")
    {
        _worldStates = worldStates;
        _slotId = slotId;
    }

    public void Initialize(GameSession session)
    {
        _worldState = _worldStates.Load() ?? WorldState.CreateDefault(_slotId, session.StartingDungeonId);
        if (_worldState.ActiveRun is not null)
        {
            session.LoadSaveGame(_worldState.ActiveRun);
            return;
        }

        session.LoadWorldState(_worldState);
    }

    public void Sync(GameSession session)
    {
        _worldState ??= WorldState.CreateDefault(_slotId, session.StartingDungeonId);
        _worldState = session.Mode == GameMode.Running
            ? _worldState with { ActiveRun = session.CreateSaveGame() }
            : _worldState with { ActiveRun = null };
        _worldStates.Save(_worldState);
    }
}
