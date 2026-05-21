using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BroughlikeMonoGame.Core;

public sealed class GameApp
{
    private readonly GameSession _session;
    private readonly GameRenderer _renderer;
    private readonly RunStatePersistence _runStatePersistence;

    public GameApp(GameAppDependencies dependencies, IScoreStorage? scoreStorage = null, ISaveStorage? saveStorage = null)
    {
        var appDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BroughlikeMonoGame");
        scoreStorage ??= new FileScoreStorage(Path.Combine(appDataDirectory, "scores.json"));
        saveStorage ??= new FileSaveStorage(Path.Combine(appDataDirectory, "savegame.json"));
        _session = new GameSession(new Random(), new AudioService(), new ScoreboardService(scoreStorage), ItemCatalog.CreateTutorialItems(), DungeonCatalog.CreateDefaultRegistry(), DungeonCatalog.DefaultStartingDungeonId);
        _runStatePersistence = new RunStatePersistence(new SaveGameService(saveStorage));
        _runStatePersistence.Initialize(_session);
        _renderer = new GameRenderer(dependencies.Font, dependencies.Pixel, dependencies.Art);
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

                _runStatePersistence.Sync(_session);
            }

            return;
        }

        var handledInput = false;
        Point2? move = null;
        if (input.IsNewKeyPress(Keys.W) || input.IsNewKeyPress(Keys.Up))
        {
            move = new Point2(0, -1);
        }
        else if (input.IsNewKeyPress(Keys.S) || input.IsNewKeyPress(Keys.Down))
        {
            move = new Point2(0, 1);
        }
        else if (input.IsNewKeyPress(Keys.A) || input.IsNewKeyPress(Keys.Left))
        {
            move = new Point2(-1, 0);
        }
        else if (input.IsNewKeyPress(Keys.D) || input.IsNewKeyPress(Keys.Right))
        {
            move = new Point2(1, 0);
        }

        if (move is { } delta)
        {
            _session.TryMovePlayer(delta);
            handledInput = true;
        }

        for (var i = 0; i < 9; i++)
        {
            var key = Keys.D1 + i;
            if (input.IsNewKeyPress(key) || input.IsNewKeyPress(Keys.NumPad1 + i))
            {
                _session.UseItem(i);
                handledInput = true;
            }
        }

        if (handledInput)
        {
            _runStatePersistence.Sync(_session);
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
