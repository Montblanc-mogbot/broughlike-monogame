using System;

namespace BroughlikeMonoGame.Core;

public sealed record SpellDefinition(string Name, Action<GameSession> Cast);
