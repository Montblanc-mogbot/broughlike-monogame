using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BroughlikeMonoGame.Core;

public sealed class GameRenderer
{
    private readonly SpriteFont _font;
    private readonly Texture2D _pixel;

    public GameRenderer(SpriteFont font, Texture2D pixel)
    {
        _font = font;
        _pixel = pixel;
    }

    public void Draw(SpriteBatch spriteBatch, GameSession session, Viewport viewport)
    {
        var destination = CalculateDestinationRect(viewport);
        var scale = destination.Width / (float)Layout.ScreenWidth;
        var transform = Matrix.CreateScale(scale, scale, 1f) * Matrix.CreateTranslation(destination.X, destination.Y, 0f);

        spriteBatch.GraphicsDevice.Viewport = viewport;
        spriteBatch.GraphicsDevice.ScissorRectangle = destination;

        spriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            transformMatrix: transform,
            rasterizerState: ScissorRasterizerState.Instance);

        if (session.Mode is GameMode.Title)
        {
            DrawTitle(spriteBatch, session);
            spriteBatch.End();
            return;
        }

        DrawBoard(spriteBatch, session);
        DrawSidebar(spriteBatch, session);

        if (session.Mode is GameMode.Dead)
        {
            DrawOverlay(spriteBatch, "You Died");
        }

        spriteBatch.End();
    }

    private void DrawTitle(SpriteBatch spriteBatch, GameSession session)
    {
        DrawOverlay(spriteBatch, "SUPER\nBROUGH BROS.");
        DrawTextCentered(spriteBatch, session.BannerMessage ?? string.Empty, Layout.ScreenHeight / 2 + 10, Palette.UiAccent, 0.7f);

        if (session.Scores.Count == 0)
        {
            return;
        }

        DrawTextCentered(spriteBatch, RightPad("RUN", "SCORE", "TOTAL"), Layout.ScreenHeight / 2 + 70, Color.White, 0.6f);
        var scores = session.Scores.ToList();
        var newest = scores.Last();
        scores = scores.OrderByDescending(entry => entry.TotalScore).ToList();
        scores.Remove(newest);
        scores.Insert(0, newest);

        for (var i = 0; i < Math.Min(10, scores.Count); i++)
        {
            var color = i == 0 ? Palette.UiAccent : Palette.UiPrimary;
            DrawTextCentered(spriteBatch, RightPad(scores[i].Run, scores[i].Score, scores[i].TotalScore), Layout.ScreenHeight / 2 + 100 + i * 24, color, 0.55f);
        }
    }

    private void DrawBoard(SpriteBatch spriteBatch, GameSession session)
    {
        var shake = session.ShakeOffset;
        foreach (var tile in session.Grid.AllTiles())
        {
            var rect = TileRect(tile.Position, shake);
            spriteBatch.Draw(_pixel, rect, GetTileColor(tile));

            if (tile.HasTreasure)
            {
                var treasureRect = Shrink(rect, 20);
                spriteBatch.Draw(_pixel, treasureRect, Palette.Treasure);
            }

            if (tile.Effect is not null)
            {
                spriteBatch.Draw(_pixel, Shrink(rect, 8), GetEffectColor(tile.Effect.Kind) * tile.Effect.Alpha);
            }
        }

        foreach (var monster in session.Monsters.Where(monster => !monster.Dead))
        {
            DrawMonster(spriteBatch, monster, shake);
        }

        DrawMonster(spriteBatch, session.Player, shake);
    }

    private void DrawMonster(SpriteBatch spriteBatch, MonsterActor actor, Point2 shake)
    {
        var position = new Vector2(
            (actor.Tile.Position.X + actor.OffsetX) * Layout.TileSize + shake.X,
            (actor.Tile.Position.Y + actor.OffsetY) * Layout.TileSize + shake.Y);
        var bounds = new Rectangle((int)position.X + 8, (int)position.Y + 8, Layout.TileSize - 16, Layout.TileSize - 16);

        var color = actor.Shield > 0 ? Palette.Shield : actor.Archetype.Color;
        if (actor.TeleportCounter > 0)
        {
            color = Palette.UiAccent;
        }

        spriteBatch.Draw(_pixel, bounds, color);

        var hp = (int)MathF.Ceiling(actor.Hp);
        for (var i = 0; i < hp; i++)
        {
            var heart = new Rectangle(bounds.X + (i % 3) * 10, bounds.Y - 10 - (i / 3) * 10, 8, 8);
            spriteBatch.Draw(_pixel, heart, Palette.DamageFlash);
        }
    }

    private void DrawSidebar(SpriteBatch spriteBatch, GameSession session)
    {
        var x = Layout.ScreenWidth - Layout.UiTilesWide * Layout.TileSize + 18;
        DrawText(spriteBatch, $"Level: {session.Level}", new Vector2(x, 30), Palette.UiPrimary, 0.8f);
        DrawText(spriteBatch, $"Score: {session.Score}", new Vector2(x, 65), Palette.UiPrimary, 0.8f);
        DrawText(spriteBatch, $"HP: {MathF.Ceiling(session.Player.Hp)}/{GameConstants.MaxHp}", new Vector2(x, 100), Palette.UiMuted, 0.65f);

        for (var i = 0; i < session.PlayerSpells.Count; i++)
        {
            var text = $"{i + 1}) {session.PlayerSpells[i] ?? string.Empty}";
            DrawText(spriteBatch, text, new Vector2(x, 145 + i * 34), Palette.UiAccent, 0.62f);
        }

        if (!string.IsNullOrWhiteSpace(session.BannerMessage) && session.Mode == GameMode.Running)
        {
            DrawText(spriteBatch, session.BannerMessage, new Vector2(x, Layout.ScreenHeight - 80), Palette.UiMuted, 0.5f);
        }
    }

    private void DrawOverlay(SpriteBatch spriteBatch, string title)
    {
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, Layout.ScreenWidth, Layout.ScreenHeight), Color.Black * 0.78f);
        DrawTextCentered(spriteBatch, title, Layout.ScreenHeight / 2 - 80, Color.White, 1.2f);
    }

    private void DrawText(SpriteBatch spriteBatch, string text, Vector2 position, Color color, float scale)
        => spriteBatch.DrawString(_font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

    private void DrawTextCentered(SpriteBatch spriteBatch, string text, int y, Color color, float scale)
    {
        var size = _font.MeasureString(text) * scale;
        var position = new Vector2((Layout.ScreenWidth - size.X) / 2f, y);
        DrawText(spriteBatch, text, position, color, scale);
    }

    private static Rectangle TileRect(Point2 point, Point2 shake)
        => new(point.X * Layout.TileSize + shake.X, point.Y * Layout.TileSize + shake.Y, Layout.TileSize, Layout.TileSize);

    public static Rectangle CalculateDestinationRect(Viewport viewport)
    {
        var scale = MathF.Min(
            viewport.Width / (float)Layout.ScreenWidth,
            viewport.Height / (float)Layout.ScreenHeight);

        if (scale <= 0f)
        {
            return new Rectangle(0, 0, Layout.ScreenWidth, Layout.ScreenHeight);
        }

        var width = Math.Max(1, (int)MathF.Floor(Layout.ScreenWidth * scale));
        var height = Math.Max(1, (int)MathF.Floor(Layout.ScreenHeight * scale));
        var x = (viewport.Width - width) / 2;
        var y = (viewport.Height - height) / 2;
        return new Rectangle(x, y, width, height);
    }

    private static Rectangle Shrink(Rectangle rect, int amount)
        => new(rect.X + amount, rect.Y + amount, rect.Width - amount * 2, rect.Height - amount * 2);

    private static Color GetTileColor(Tile tile) => tile.Kind switch
    {
        TileKind.Floor => Palette.Floor,
        TileKind.Wall => Palette.Wall,
        TileKind.Exit => Palette.Exit,
        _ => Palette.Floor,
    };

    private static Color GetEffectColor(EffectKind kind) => kind switch
    {
        EffectKind.Heal => Palette.EffectHeal,
        EffectKind.Bolt => Palette.EffectBolt,
        EffectKind.Cross => Palette.EffectCross,
        EffectKind.Dash => Palette.EffectDash,
        _ => Palette.EffectHeal,
    };

    private static string RightPad(params object[] values)
        => string.Concat(values.Select(value => value.ToString()?.PadRight(10) ?? string.Empty));
}

internal sealed class ScissorRasterizerState : RasterizerState
{
    public static readonly ScissorRasterizerState Instance = new();

    private ScissorRasterizerState()
    {
        ScissorTestEnable = true;
    }
}
