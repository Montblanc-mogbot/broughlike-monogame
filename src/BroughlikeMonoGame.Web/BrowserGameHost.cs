using System;
using System.Collections.Generic;
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
    private readonly Queue<Keys> _pendingKeys = new();
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

    public void QueueBrowserKey(Keys key)
    {
        _pendingKeys.Enqueue(key);
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
        var art = new GameArt(
            Content.Load<Texture2D>("Sprites/ApartmentFloor"),
            Content.Load<Texture2D>("Sprites/ApartmentWall"),
            Content.Load<Texture2D>("Sprites/ApartmentDoor"),
            Content.Load<Texture2D>("Sprites/ApartmentBed"),
            Content.Load<Texture2D>("Sprites/ApartmentDresser"),
            Content.Load<Texture2D>("Sprites/ApartmentFigure"),
            Content.Load<Texture2D>("Sprites/ApartmentArmchair"),
            Content.Load<Texture2D>("Sprites/ApartmentSideTable"),
            Content.Load<Texture2D>("Sprites/ApartmentBedroomFloor"),
            Content.Load<Texture2D>("Sprites/ApartmentBedroomWall"),
            Content.Load<Texture2D>("Sprites/ApartmentLivingRoomFloor"),
            Content.Load<Texture2D>("Sprites/ApartmentLivingRoomWall"));
        _app = new GameApp(new GameAppDependencies(_font, _pixel, art), new BrowserScoreStorage());
    }

    protected override void Update(GameTime gameTime)
    {
        InputSnapshot snapshot;
        if (_pendingKeys.Count > 0)
        {
            var key = _pendingKeys.Dequeue();
            snapshot = InputSnapshot.Create(new KeyboardState([key]), default);
        }
        else
        {
            snapshot = InputSnapshot.Create(default, default);
        }

        _app?.Update(snapshot);
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
