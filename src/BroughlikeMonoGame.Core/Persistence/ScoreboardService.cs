using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace BroughlikeMonoGame.Core;

public sealed class ScoreboardService
{
    private readonly string _path;

    public ScoreboardService(string path)
    {
        _path = path;
    }

    public List<ScoreEntry> Load()
    {
        if (!File.Exists(_path))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<ScoreEntry>>(File.ReadAllText(_path)) ?? [];
    }

    public void Save(List<ScoreEntry> scores)
    {
        var directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(scores, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_path, json);
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
