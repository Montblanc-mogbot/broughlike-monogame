namespace BroughlikeMonoGame.Core;

public sealed record ExitRoute(
    PortalDestination Destination,
    string? RequiredItemId = null,
    bool RequireItem = true,
    string? RequiredProgressFlag = null,
    bool RequireProgressFlag = true,
    string? GrantsProgressFlag = null,
    string? Label = null,
    bool SetsCurrentStart = false);
