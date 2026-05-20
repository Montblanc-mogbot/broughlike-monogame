using System.Collections.Generic;

namespace BroughlikeMonoGame.Core;

public sealed class ExitDefinition
{
    public ExitDefinition(IReadOnlyList<ExitRoute> routes)
    {
        Routes = routes;
    }

    public IReadOnlyList<ExitRoute> Routes { get; }
}
