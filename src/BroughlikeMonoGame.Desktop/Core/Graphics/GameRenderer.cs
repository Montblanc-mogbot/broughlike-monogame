using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BroughlikeMonoGame.Desktop.Core;

public sealed class GameRenderer
{
    private readonly SpriteFont _font;
    private readonly Texture2D _pixel;

    public GameRenderer(SpriteFont font, Texture2D pixel)
    {
        _font = font;
        _pixel = pixel;
    }

    public void Draw(SpriteBatch spriteBatch, GameSession session)
    {
        if (session.Mode is GameMode.Title)
        {
            DrawTitle(spriteBatch, session);
            return;
        }

        DrawBoard(spriteBatch, session);
        DrawSidebar(spriteBatch, session);

        if (session.Mode is GameMode.Dead)
        {
            DrawOverlay(spriteBatch, "You Died");
        }
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
        var lungeOffset = GetLungeOffset(actor);
        var position = new Vector2(
            (actor.Tile.Position.X + actor.OffsetX) * Layout.TileSize + shake.X + lungeOffset.X,
            (actor.Tile.Position.Y + actor.OffsetY) * Layout.TileSize + shake.Y + lungeOffset.Y);
        var bounds = new Rectangle((int)position.X + 8, (int)position.Y + 8, Layout.TileSize - 16, Layout.TileSize - 16);

        var color = actor.Shield > 0 ? Palette.Shield : actor.Archetype.Color;
        if (actor.TeleportCounter > 0)
        {
            color = Palette.UiAccent;
        }
        else if (actor.HurtFlashFrames > 0)
        {
            color = Color.Lerp(color, Palette.DamageFlash, 0.65f);
        }

        spriteBatch.Draw(_pixel, bounds, Palette.ActorShadow);
        var body = Shrink(bounds, 4);
        spriteBatch.Draw(_pixel, body, color);
        DrawDamageMarker(spriteBatch, actor, bounds);
        DrawStunIndicator(spriteBatch, actor, bounds);

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

    private void DrawDamageMarker(SpriteBatch spriteBatch, MonsterActor actor, Rectangle bounds)
    {
        if (actor.DamageTakenThisTurn <= 0)
        {
            return;
        }

        var markerY = bounds.Y - 18 - Math.Max(0, 12 - actor.HurtFlashFrames);
        var marker = new Rectangle(bounds.Center.X - 7, markerY, 14, 14);
        spriteBatch.Draw(_pixel, marker, Palette.DamageFlash);
        if (actor.LastDamageDirection != default)
        {
            var arrow = new Rectangle(
                marker.Center.X - actor.LastDamageDirection.X * 6 - 2,
                marker.Center.Y - actor.LastDamageDirection.Y * 6 - 2,
                4,
                4);
            spriteBatch.Draw(_pixel, arrow, Palette.UiAccent);
        }
    }

    private void DrawStunIndicator(SpriteBatch spriteBatch, MonsterActor actor, Rectangle bounds)
    {
        if (!actor.Stunned && actor.StunPulseFrames <= 0)
        {
            return;
        }

        var pulse = 2 + (actor.StunPulseFrames % 6) / 2;
        var ring = new Rectangle(bounds.X - pulse, bounds.Y - pulse, bounds.Width + pulse * 2, bounds.Height + pulse * 2);
        DrawFrame(spriteBatch, ring, Palette.UiAccent * 0.75f, 2);
        spriteBatch.Draw(_pixel, new Rectangle(bounds.Center.X - 8, bounds.Y - 18, 16, 4), Palette.UiAccent);
    }

    private Vector2 GetLungeOffset(MonsterActor actor)
    {
        if (actor.AttackLungeFrames <= 0 || actor.AttackDirection == default)
        {
            return Vector2.Zero;
        }

        var strength = actor.AttackLungeFrames / 8f;
        return new Vector2(actor.AttackDirection.X, actor.AttackDirection.Y) * (6f * strength);
    }

    private void DrawFrame(SpriteBatch spriteBatch, Rectangle rect, Color color, int thickness)
    {
        spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
        spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
        spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
        spriteBatch.Draw(_pixel, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
    }

    private static string RightPad(params object[] values)
        => string.Concat(values.Select(value => value.ToString()?.PadRight(10) ?? string.Empty));
}
