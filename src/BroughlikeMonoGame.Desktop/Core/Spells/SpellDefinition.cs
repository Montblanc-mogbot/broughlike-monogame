using System;

namespace BroughlikeMonoGame.Desktop.Core;

public sealed record SpellDefinition(string Name, Action<GameSession> Cast);
