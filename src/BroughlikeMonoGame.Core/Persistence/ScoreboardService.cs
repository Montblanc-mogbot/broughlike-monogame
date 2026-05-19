using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace BroughlikeMonoGame.Core;

public sealed class ScoreboardService
{
    private readonly IScoreStorage _storage;

    public ScoreboardService(IScoreStorage storage)
    {
        _storage = storage;
    }

    public List<ScoreEntry> Load()
    {
        if (!_storage.Exists())
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<ScoreEntry>>(_storage.ReadAllText() ?? "[]") ?? [];
    }

    public void Save(List<ScoreEntry> scores)
    {
        var json = JsonSerializer.Serialize(scores, new JsonSerializerOptions { WriteIndented = true });
        _storage.WriteAllText(json);
    }

    public List<ScoreEntry> AddScore(List<ScoreEntry> scores, int score, bool won)
    {
        var next = scores.ToList();
        var scoreEntry = new ScoreEntry { Score = score, Run = 1, TotalScore = score, Active = won };
        var lastScore = next.LastOrDefault();
        if (lastScore is not null)
        {
            next.RemoveAt(next.Count - 1);
            if (lastScore.Active)
            {
                scoreEntry = new ScoreEntry
                {
                    Score = score,
                    Run = lastScore.Run + 1,
                    TotalScore = lastScore.TotalScore + score,
                    Active = won,
                };
            }
            else
            {
                next.Add(lastScore);
            }
        }

        next.Add(scoreEntry);
        Save(next);
        return next;
    }
}
