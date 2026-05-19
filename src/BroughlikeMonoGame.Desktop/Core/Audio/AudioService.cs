namespace BroughlikeMonoGame.Desktop.Core;

public sealed class AudioService
{
    public void Play(string cue)
    {
        LastCue = cue;
    }

    public string? LastCue { get; private set; }
}
