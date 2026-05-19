using System;

namespace BroughlikeMonoGame.Core;

public sealed record ItemDefinition(string Id, string DisplayName, Action<GameSession> Use);
