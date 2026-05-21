using System.Collections.Generic;
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

        var storyFlags = new Dictionary<string, bool>(_worldState.StoryFlags);
        foreach (var flag in session.ProgressFlags)
        {
            storyFlags[flag] = true;
        }

        _worldState = _worldState with
        {
            CurrentStart = new WorldStartState(session.CurrentStartDungeonId, session.CurrentStartFloorNumber),
            Player = new WorldPlayerState(session.CurrentStartPlayerHp, session.CurrentStartPlayerMaxHp, session.CurrentStartInventoryItemIds.ToArray()),
            StoryFlags = storyFlags,
            StashItemIds = session.StashItemIds.ToArray(),
            ActiveRun = session.Mode == GameMode.Running ? session.CreateSaveGame() : null
        };

        _worldStates.Save(_worldState);
    }
}
