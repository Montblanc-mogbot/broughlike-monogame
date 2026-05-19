using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BroughlikeMonoGame.Core;

public sealed class GameApp
{
    private readonly GameSession _session;
    private readonly GameRenderer _renderer;

    public GameApp(GameAppDependencies dependencies, IScoreStorage? scoreStorage = null)
    {
        scoreStorage ??= new FileScoreStorage(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BroughlikeMonoGame", "scores.json"));
        _session = new GameSession(new Random(), new AudioService(), new ScoreboardService(scoreStorage), SpellBook.Create());
        _session.ShowTitle();
        _renderer = new GameRenderer(dependencies.Font, dependencies.Pixel);
    }

    public void Update(InputSnapshot input)
    {
        _session.AdvanceFrame();

        if (_session.Mode is GameMode.Title or GameMode.Dead)
        {
            if (AnyActionPressed(input))
            {
                if (_session.Mode == GameMode.Dead)
                {
                    _session.ShowTitle();
                }
                else
                {
                    _session.StartGame();
                }
            }

            return;
        }

        Point2? move = null;
        if (input.IsNewKeyPress(Keys.W) || input.IsNewKeyPress(Keys.Up)) move = new Point2(0, -1);
        else if (input.IsNewKeyPress(Keys.S) || input.IsNewKeyPress(Keys.Down)) move = new Point2(0, 1);
        else if (input.IsNewKeyPress(Keys.A) || input.IsNewKeyPress(Keys.Left)) move = new Point2(-1, 0);
        else if (input.IsNewKeyPress(Keys.D) || input.IsNewKeyPress(Keys.Right)) move = new Point2(1, 0);

        if (move is { } delta)
        {
            _session.TryMovePlayer(delta);
        }

        for (var i = 0; i < 9; i++)
        {
            var key = Keys.D1 + i;
            if (input.IsNewKeyPress(key) || input.IsNewKeyPress(Keys.NumPad1 + i))
            {
                _session.CastSpell(i);
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch, Viewport viewport) => _renderer.Draw(spriteBatch, _session, viewport);

    private static bool AnyActionPressed(InputSnapshot input)
        => input.IsNewKeyPress(Keys.W)
           || input.IsNewKeyPress(Keys.A)
           || input.IsNewKeyPress(Keys.S)
           || input.IsNewKeyPress(Keys.D)
           || input.IsNewKeyPress(Keys.Up)
           || input.IsNewKeyPress(Keys.Down)
           || input.IsNewKeyPress(Keys.Left)
           || input.IsNewKeyPress(Keys.Right)
           || input.IsNewKeyPress(Keys.Enter)
           || input.IsNewKeyPress(Keys.Space);
}
