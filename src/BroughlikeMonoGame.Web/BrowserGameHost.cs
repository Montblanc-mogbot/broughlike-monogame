using System;
using BroughlikeMonoGame.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BroughlikeMonoGame.Web;

internal sealed class BrowserGameHost : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch? _spriteBatch;
    private Texture2D? _pixel;
    private SpriteFont? _font;
    private InputSnapshot _previousInput;
    private GameApp? _app;
    private bool _initialized;

    public BrowserGameHost()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = string.Empty;
        IsMouseVisible = true;
        Window.AllowUserResizing = false;
        Window.Title = "Broughlike MonoGame";
    }

    public new void Tick()
    {
        if (!_initialized)
        {
            Run();
            _initialized = true;
        }

        base.Tick();
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferWidth = Layout.ScreenWidth;
        _graphics.PreferredBackBufferHeight = Layout.ScreenHeight;
        _graphics.ApplyChanges();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        _font = Content.Load<SpriteFont>("Fonts/UiFont");
        _app = new GameApp(new GameAppDependencies(_font, _pixel), new BrowserScoreStorage());
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var snapshot = InputSnapshot.Create(keyboard, _previousInput.KeyboardState);
        _app?.Update(snapshot);
        _previousInput = snapshot;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Palette.Background);

        if (_spriteBatch is null || _app is null)
        {
            return;
        }

        _app.Draw(_spriteBatch, GraphicsDevice.Viewport);

        base.Draw(gameTime);
    }
}
