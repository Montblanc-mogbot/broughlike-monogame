using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BroughlikeMonoGame.Core;

internal enum ApartmentRoomStyle
{
    Default,
    Bedroom,
    LivingRoom,
    Hallway,
    BurstnerRoom,
}

public sealed class GameRenderer
{
    private const int TileInset = 3;
    private readonly SpriteFont _font;
    private readonly Texture2D _pixel;
    private readonly GameArt _art;

    public GameRenderer(SpriteFont font, Texture2D pixel, GameArt art)
    {
        _font = font;
        _pixel = pixel;
        _art = art;
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

        _currentFloorName = session.CurrentFloorDisplayName;
        DrawBoard(spriteBatch, session);
        DrawSidebar(spriteBatch, session);

        if (!string.IsNullOrWhiteSpace(session.BannerMessage) && session.Mode == GameMode.Running)
        {
            DrawMessageBox(spriteBatch, session.BannerMessage);
        }

        if (session.Mode is GameMode.Dead)
        {
            DrawOverlay(spriteBatch, "You Died");
        }

        spriteBatch.End();
    }

    private void DrawTitle(SpriteBatch spriteBatch, GameSession session)
    {
        DrawOverlay(spriteBatch, "THE\nTRIAL");
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
        var useApartmentSprites = UsesApartmentSprites(session);
        foreach (var tile in session.Grid.AllTiles())
        {
            DrawTile(spriteBatch, tile, shake, useApartmentSprites);

            if (tile.WorldObject is not null)
            {
                DrawWorldObject(spriteBatch, tile, shake, tile.WorldObject, useApartmentSprites);
            }

            if (tile.Effect is not null)
            {
                DrawEffect(spriteBatch, tile, shake, tile.Effect);
            }
        }

        foreach (var monster in session.Monsters.Where(monster => !monster.Dead))
        {
            DrawMonster(spriteBatch, monster, shake);
        }

        DrawMonster(spriteBatch, session.Player, shake, useApartmentSprites);
    }

    private void DrawTile(SpriteBatch spriteBatch, Tile tile, Point2 shake, bool useApartmentSprites)
    {
        var rect = TileRect(tile.Position, shake);
        if (useApartmentSprites)
        {
            var (texture, tint) = GetApartmentTileArt(tile);
            spriteBatch.Draw(texture, rect, tint);
            spriteBatch.Draw(_pixel, rect, Color.Black * 0.18f);
            return;
        }

        spriteBatch.Draw(_pixel, rect, Palette.TileOutline);
        var inner = Shrink(rect, TileInset);
        spriteBatch.Draw(_pixel, inner, GetTileColor(tile));

        switch (tile.Kind)
        {
            case TileKind.Floor:
                DrawFloorDetails(spriteBatch, inner, tile.Position);
                break;
            case TileKind.Wall:
                DrawWallDetails(spriteBatch, inner, tile.Position);
                break;
            case TileKind.Exit:
                DrawExitDetails(spriteBatch, inner);
                break;
        }
    }

    private void DrawFloorDetails(SpriteBatch spriteBatch, Rectangle rect, Point2 point)
    {
        var size = 4;
        var left = rect.X + 10 + (point.X % 2) * 6;
        var top = rect.Y + 12 + (point.Y % 2) * 5;
        spriteBatch.Draw(_pixel, new Rectangle(left, top, size, size), Palette.FloorSpeck);
        spriteBatch.Draw(_pixel, new Rectangle(rect.Right - left + rect.X - size, rect.Bottom - top + rect.Y - size, size, size), Palette.FloorSpeck * 0.7f);
    }

    private void DrawWallDetails(SpriteBatch spriteBatch, Rectangle rect, Point2 point)
    {
        var brickHeight = 10;
        for (var y = rect.Y + 8; y < rect.Bottom - 6; y += brickHeight)
        {
            var offset = ((y / brickHeight) + point.X) % 2 == 0 ? 0 : 10;
            for (var x = rect.X + 6 - offset; x < rect.Right - 6; x += 20)
            {
                var width = Math.Min(16, rect.Right - 6 - x);
                if (width > 6)
                {
                    spriteBatch.Draw(_pixel, new Rectangle(x, y, width, 2), Palette.WallMortar);
                }
            }
        }
    }

    private void DrawExitDetails(SpriteBatch spriteBatch, Rectangle rect)
    {
        var centerX = rect.Center.X;
        var centerY = rect.Center.Y;
        spriteBatch.Draw(_pixel, new Rectangle(centerX - 4, rect.Y + 10, 8, rect.Height - 20), Palette.ExitGlow);
        spriteBatch.Draw(_pixel, new Rectangle(rect.X + 10, centerY - 4, rect.Width - 20, 8), Palette.ExitGlow * 0.8f);
        spriteBatch.Draw(_pixel, new Rectangle(centerX - 10, centerY - 10, 20, 20), Palette.ExitCore);
    }

    private void DrawWorldObject(SpriteBatch spriteBatch, Tile tile, Point2 shake, WorldObject worldObject, bool useApartmentSprites)
    {
        if (useApartmentSprites)
        {
            Texture2D? sprite = worldObject.VisualKind switch
            {
                WorldObjectVisualKind.Bed => _art.ApartmentBed,
                WorldObjectVisualKind.Dresser => _art.ApartmentDresser,
                WorldObjectVisualKind.Npc => _art.ApartmentFigure,
                WorldObjectVisualKind.Armchair => _art.ApartmentArmchair,
                WorldObjectVisualKind.SideTable => _art.ApartmentSideTable,
                _ => null,
            };

            if (sprite is not null)
            {
                spriteBatch.Draw(sprite, TileRect(tile.Position, shake), Color.White);
                return;
            }
        }

        switch (worldObject.VisualKind)
        {
            case WorldObjectVisualKind.Treasure:
                DrawTreasurePickup(spriteBatch, tile, shake);
                break;
            case WorldObjectVisualKind.Item:
                DrawItemPickup(spriteBatch, tile, shake);
                break;
            case WorldObjectVisualKind.Portal:
                DrawPortal(spriteBatch, tile, shake);
                break;
            case WorldObjectVisualKind.Npc:
                DrawNpc(spriteBatch, tile, shake);
                break;
            case WorldObjectVisualKind.Bed:
                DrawBed(spriteBatch, tile, shake);
                break;
            case WorldObjectVisualKind.Dresser:
                DrawDresser(spriteBatch, tile, shake);
                break;
            case WorldObjectVisualKind.Armchair:
                DrawNpc(spriteBatch, tile, shake);
                break;
            case WorldObjectVisualKind.SideTable:
                DrawDresser(spriteBatch, tile, shake);
                break;
        }
    }

    private void DrawTreasurePickup(SpriteBatch spriteBatch, Tile tile, Point2 shake)
    {
        var rect = Shrink(TileRect(tile.Position, shake), 14);
        spriteBatch.Draw(_pixel, rect, Palette.TreasureShadow);
        spriteBatch.Draw(_pixel, Shrink(rect, 5), Palette.Treasure);
        spriteBatch.Draw(_pixel, new Rectangle(rect.Center.X - 3, rect.Y + 3, 6, rect.Height - 6), Palette.TreasureHighlight);
        spriteBatch.Draw(_pixel, new Rectangle(rect.X + 3, rect.Center.Y - 3, rect.Width - 6, 6), Palette.TreasureHighlight * 0.85f);
    }

    private void DrawItemPickup(SpriteBatch spriteBatch, Tile tile, Point2 shake)
    {
        var rect = Shrink(TileRect(tile.Position, shake), 15);
        spriteBatch.Draw(_pixel, rect, Palette.ActorShadow);
        spriteBatch.Draw(_pixel, Shrink(rect, 4), Palette.UiAccent);
        spriteBatch.Draw(_pixel, new Rectangle(rect.Center.X - 3, rect.Y + 2, 6, rect.Height - 4), Palette.UiPrimary);
        spriteBatch.Draw(_pixel, new Rectangle(rect.X + 2, rect.Center.Y - 3, rect.Width - 4, 6), Palette.UiPrimary * 0.9f);
    }

    private void DrawPortal(SpriteBatch spriteBatch, Tile tile, Point2 shake)
    {
        var rect = Shrink(TileRect(tile.Position, shake), 12);
        spriteBatch.Draw(_pixel, rect, Palette.ActorShadow);
        spriteBatch.Draw(_pixel, Shrink(rect, 3), Palette.ExitGlow * 0.65f);
        spriteBatch.Draw(_pixel, new Rectangle(rect.X + 6, rect.Center.Y - 4, rect.Width - 12, 8), Palette.ExitCore);
        spriteBatch.Draw(_pixel, new Rectangle(rect.Center.X - 4, rect.Y + 6, 8, rect.Height - 12), Palette.ExitCore * 0.9f);
    }

    private void DrawNpc(SpriteBatch spriteBatch, Tile tile, Point2 shake)
    {
        var rect = Shrink(TileRect(tile.Position, shake), 12);
        spriteBatch.Draw(_pixel, rect, Palette.ActorShadow);
        var body = Shrink(rect, 6);
        spriteBatch.Draw(_pixel, body, Palette.UiAccent);
        spriteBatch.Draw(_pixel, new Rectangle(body.Center.X - 10, body.Y + 8, 20, 10), Palette.UiPrimary);
        spriteBatch.Draw(_pixel, new Rectangle(body.Center.X - 3, body.Bottom - 14, 6, 10), Palette.ActorEyeBright);
    }

    private void DrawBed(SpriteBatch spriteBatch, Tile tile, Point2 shake)
    {
        var rect = Shrink(TileRect(tile.Position, shake), 10);
        spriteBatch.Draw(_pixel, rect, Palette.ActorShadow);
        spriteBatch.Draw(_pixel, new Rectangle(rect.X + 4, rect.Y + 8, rect.Width - 8, rect.Height - 12), Palette.UiMuted);
        spriteBatch.Draw(_pixel, new Rectangle(rect.X + 8, rect.Y + 12, rect.Width - 16, rect.Height - 20), Palette.UiPrimary * 0.8f);
        spriteBatch.Draw(_pixel, new Rectangle(rect.X + 8, rect.Y + 12, 12, 10), Color.White);
    }

    private void DrawDresser(SpriteBatch spriteBatch, Tile tile, Point2 shake)
    {
        var rect = Shrink(TileRect(tile.Position, shake), 12);
        spriteBatch.Draw(_pixel, rect, Palette.ActorShadow);
        spriteBatch.Draw(_pixel, Shrink(rect, 4), new Color(106, 72, 48));
        spriteBatch.Draw(_pixel, new Rectangle(rect.X + 8, rect.Y + 14, rect.Width - 16, 4), Palette.UiPrimary * 0.65f);
        spriteBatch.Draw(_pixel, new Rectangle(rect.X + 8, rect.Y + 28, rect.Width - 16, 4), Palette.UiPrimary * 0.65f);
        spriteBatch.Draw(_pixel, new Rectangle(rect.Center.X - 2, rect.Y + 10, 4, rect.Height - 20), Palette.TreasureHighlight);
    }

    private void DrawEffect(SpriteBatch spriteBatch, Tile tile, Point2 shake, TileEffect effect)
    {
        var rect = Shrink(TileRect(tile.Position, shake), 6);
        var color = GetEffectColor(effect.Kind) * effect.Alpha;

        switch (effect.Kind)
        {
            case EffectKind.Heal:
                spriteBatch.Draw(_pixel, new Rectangle(rect.Center.X - 4, rect.Y + 8, 8, rect.Height - 16), color);
                spriteBatch.Draw(_pixel, new Rectangle(rect.X + 8, rect.Center.Y - 4, rect.Width - 16, 8), color);
                break;
            case EffectKind.Bolt:
                for (var i = 0; i < 4; i++)
                {
                    spriteBatch.Draw(_pixel, new Rectangle(rect.X + 8 + i * 10, rect.Y + 10 + (i % 2) * 8, 10, 6), color);
                }
                break;
            case EffectKind.Cross:
                spriteBatch.Draw(_pixel, new Rectangle(rect.Center.X - 4, rect.Y + 4, 8, rect.Height - 8), color);
                spriteBatch.Draw(_pixel, new Rectangle(rect.X + 4, rect.Center.Y - 4, rect.Width - 8, 8), color);
                break;
            case EffectKind.Dash:
                spriteBatch.Draw(_pixel, new Rectangle(rect.X + 6, rect.Y + 12, rect.Width - 12, 6), color);
                spriteBatch.Draw(_pixel, new Rectangle(rect.X + 12, rect.Bottom - 18, rect.Width - 24, 6), color * 0.8f);
                break;
        }
    }

    private void DrawMonster(SpriteBatch spriteBatch, MonsterActor actor, Point2 shake, bool useApartmentSprites = false)
    {
        var lungeOffset = GetLungeOffset(actor);
        var position = new Vector2(
            (actor.Tile.Position.X + actor.OffsetX) * Layout.TileSize + shake.X + lungeOffset.X,
            (actor.Tile.Position.Y + actor.OffsetY) * Layout.TileSize + shake.Y + lungeOffset.Y);
        var bounds = new Rectangle((int)position.X + 8, (int)position.Y + 8, Layout.TileSize - 16, Layout.TileSize - 16);

        if (useApartmentSprites && actor.IsPlayer)
        {
            spriteBatch.Draw(_art.ApartmentFigure, new Rectangle((int)position.X, (int)position.Y, Layout.TileSize, Layout.TileSize), Color.White);
            DrawDamageMarker(spriteBatch, actor, bounds);
            DrawStunIndicator(spriteBatch, actor, bounds);
            return;
        }

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
        DrawActorSilhouette(spriteBatch, actor, body);
        DrawActorEyes(spriteBatch, actor, body);
        DrawDamageMarker(spriteBatch, actor, bounds);
        DrawStunIndicator(spriteBatch, actor, bounds);

        var hp = (int)MathF.Ceiling(actor.Hp);
        for (var i = 0; i < hp; i++)
        {
            var heart = new Rectangle(bounds.X + (i % 3) * 10, bounds.Y - 10 - (i / 3) * 10, 8, 8);
            spriteBatch.Draw(_pixel, heart, Palette.DamageFlash);
        }
    }

    private void DrawActorSilhouette(SpriteBatch spriteBatch, MonsterActor actor, Rectangle body)
    {
        switch (actor.Kind)
        {
            case MonsterKind.Player:
                DrawDiamond(spriteBatch, body, Palette.ActorFeaturePrimary);
                spriteBatch.Draw(_pixel, new Rectangle(body.Center.X - 3, body.Y + 8, 6, body.Height - 16), Palette.ActorFeaturePrimary);
                break;
            case MonsterKind.Bird:
                spriteBatch.Draw(_pixel, new Rectangle(body.X + 6, body.Center.Y - 4, body.Width - 12, 8), Palette.ActorFeaturePrimary);
                spriteBatch.Draw(_pixel, new Rectangle(body.Center.X - 4, body.Y + 8, 8, body.Height - 16), Palette.ActorFeatureSecondary);
                break;
            case MonsterKind.Snake:
                for (var i = 0; i < 3; i++)
                {
                    spriteBatch.Draw(_pixel, new Rectangle(body.X + 8 + i * 8, body.Y + 10 + (i % 2) * 6, 10, 6), Palette.ActorFeaturePrimary);
                }
                break;
            case MonsterKind.Tank:
                spriteBatch.Draw(_pixel, new Rectangle(body.X + 6, body.Y + 6, body.Width - 12, body.Height - 12), Palette.ActorFeaturePrimary);
                spriteBatch.Draw(_pixel, new Rectangle(body.Center.X - 3, body.Y - 2, 6, 16), Palette.ActorFeatureSecondary);
                break;
            case MonsterKind.Eater:
                spriteBatch.Draw(_pixel, new Rectangle(body.X + 6, body.Center.Y - 10, body.Width - 12, 20), Palette.ActorFeaturePrimary);
                spriteBatch.Draw(_pixel, new Rectangle(body.X + 10, body.Center.Y - 4, body.Width - 20, 8), Palette.ActorShadow);
                break;
            case MonsterKind.Jester:
                spriteBatch.Draw(_pixel, new Rectangle(body.Center.X - 4, body.Y + 6, 8, body.Height - 12), Palette.ActorFeaturePrimary);
                spriteBatch.Draw(_pixel, new Rectangle(body.X + 8, body.Center.Y - 4, body.Width - 16, 8), Palette.ActorFeatureSecondary);
                DrawDiamond(spriteBatch, new Rectangle(body.X + 8, body.Y + 8, 12, 12), Palette.ActorFeaturePrimary);
                DrawDiamond(spriteBatch, new Rectangle(body.Right - 20, body.Y + 8, 12, 12), Palette.ActorFeaturePrimary);
                break;
        }
    }

    private void DrawActorEyes(SpriteBatch spriteBatch, MonsterActor actor, Rectangle body)
    {
        var eyeColor = actor.IsPlayer ? Palette.ActorEyeBright : Palette.ActorEye;
        spriteBatch.Draw(_pixel, new Rectangle(body.Center.X - 10, body.Center.Y - 6, 6, 6), eyeColor);
        spriteBatch.Draw(_pixel, new Rectangle(body.Center.X + 4, body.Center.Y - 6, 6, 6), eyeColor);
    }

    private void DrawDiamond(SpriteBatch spriteBatch, Rectangle area, Color color)
    {
        var halfWidth = Math.Max(2, area.Width / 2);
        var centerX = area.Center.X;
        var centerY = area.Center.Y;
        for (var row = 0; row < area.Height; row++)
        {
            var distance = Math.Abs(row - area.Height / 2f) / (area.Height / 2f);
            var width = Math.Max(2, (int)Math.Round((1f - distance) * halfWidth));
            spriteBatch.Draw(_pixel, new Rectangle(centerX - width / 2, area.Y + row, width, 1), color);
        }

        spriteBatch.Draw(_pixel, new Rectangle(centerX - 1, centerY - 1, 2, 2), color);
    }

    private void DrawSidebar(SpriteBatch spriteBatch, GameSession session)
    {
        var x = Layout.ScreenWidth - Layout.UiTilesWide * Layout.TileSize + 18;
        var width = Layout.UiTilesWide * Layout.TileSize - 36;
        var nameBottom = DrawFittedTextBlock(spriteBatch, session.CurrentDungeon.DisplayName, new Vector2(x, 28), width, Palette.UiPrimary, 0.72f, 0.48f, 2, 8f);
        var floorBottom = DrawFittedTextBlock(spriteBatch, session.CurrentFloorDisplayName, new Vector2(x, nameBottom + 4f), width, Palette.UiMuted, 0.58f, 0.44f, 2, 6f);
        var statsY = floorBottom + 16f;
        DrawText(spriteBatch, $"Score: {session.Score}", new Vector2(x, statsY), Palette.UiPrimary, 0.8f);
        DrawText(spriteBatch, $"HP: {MathF.Ceiling(session.Player.Hp)}/{GameConstants.MaxHp}", new Vector2(x, statsY + 35f), Palette.UiMuted, 0.65f);

        for (var i = 0; i < session.Inventory.SlotCount; i++)
        {
            var item = session.Inventory.GetItem(i);
            var text = $"{i + 1}) {item?.DisplayName ?? string.Empty}";
            DrawText(spriteBatch, text, new Vector2(x, statsY + 80f + i * 34), Palette.UiAccent, 0.62f);
        }
    }

    private void DrawMessageBox(SpriteBatch spriteBatch, string message)
    {
        var width = Layout.MapTiles * Layout.TileSize - 28;
        var height = 74;
        var rect = new Rectangle(14, Layout.ScreenHeight - height - 14, width, height);
        spriteBatch.Draw(_pixel, rect, Color.Black * 0.88f);
        spriteBatch.Draw(_pixel, new Rectangle(rect.X + 3, rect.Y + 3, rect.Width - 6, rect.Height - 6), new Color(28, 28, 42));
        DrawText(spriteBatch, message, new Vector2(rect.X + 14, rect.Y + 18), Palette.UiPrimary, 0.58f);
    }

    private void DrawOverlay(SpriteBatch spriteBatch, string title)
    {
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, Layout.ScreenWidth, Layout.ScreenHeight), Color.Black * 0.78f);
        DrawTextCentered(spriteBatch, title, Layout.ScreenHeight / 2 - 80, Color.White, 1.2f);
    }

    private void DrawText(SpriteBatch spriteBatch, string text, Vector2 position, Color color, float scale)
        => spriteBatch.DrawString(_font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

    private float DrawFittedTextBlock(
        SpriteBatch spriteBatch,
        string text,
        Vector2 position,
        float maxWidth,
        Color color,
        float preferredScale,
        float minimumScale,
        int maxLines,
        float lineGap)
    {
        var scale = preferredScale;
        var lines = WrapText(text, maxWidth, scale);
        while ((lines.Count > maxLines || lines.Any(line => _font.MeasureString(line).X * scale > maxWidth)) && scale > minimumScale)
        {
            scale -= 0.02f;
            lines = WrapText(text, maxWidth, scale);
        }

        if (lines.Count > maxLines)
        {
            lines = lines.Take(maxLines).ToList();
            var last = lines[^1];
            while (last.Length > 0 && _font.MeasureString($"{last}…").X * scale > maxWidth)
            {
                last = last[..^1];
            }

            lines[^1] = $"{last.TrimEnd()}…";
        }

        var y = position.Y;
        foreach (var line in lines)
        {
            DrawText(spriteBatch, line, new Vector2(position.X, y), color, scale);
            y += _font.LineSpacing * scale + lineGap;
        }

        return y;
    }

    private List<string> WrapText(string text, float maxWidth, float scale)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            return [string.Empty];
        }

        var lines = new List<string>();
        var current = words[0];
        for (var i = 1; i < words.Length; i++)
        {
            var candidate = $"{current} {words[i]}";
            if (_font.MeasureString(candidate).X * scale <= maxWidth)
            {
                current = candidate;
            }
            else
            {
                lines.Add(current);
                current = words[i];
            }
        }

        lines.Add(current);
        return lines;
    }

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

    private static bool UsesApartmentSprites(GameSession session)
        => string.Equals(session.CurrentDungeonId, "apartment-intro", StringComparison.OrdinalIgnoreCase);

    private (Texture2D texture, Color tint) GetApartmentTileArt(Tile tile)
    {
        return CurrentApartmentRoomStyle switch
        {
            ApartmentRoomStyle.Bedroom => tile.Kind switch
            {
                TileKind.Floor => (_art.ApartmentBedroomFloor, Color.White),
                TileKind.Wall => (_art.ApartmentBedroomWall, Color.White),
                TileKind.Exit => (_art.ApartmentDoor, new Color(235, 228, 214)),
                _ => (_art.ApartmentBedroomFloor, Color.White),
            },
            ApartmentRoomStyle.LivingRoom => tile.Kind switch
            {
                TileKind.Floor => (_art.ApartmentLivingRoomFloor, Color.White),
                TileKind.Wall => (_art.ApartmentLivingRoomWall, Color.White),
                TileKind.Exit => (_art.ApartmentDoor, new Color(219, 229, 214)),
                _ => (_art.ApartmentLivingRoomFloor, Color.White),
            },
            ApartmentRoomStyle.Hallway => tile.Kind switch
            {
                TileKind.Floor => (_art.ApartmentLivingRoomFloor, new Color(164, 138, 124)),
                TileKind.Wall => (_art.ApartmentWall, new Color(160, 156, 126)),
                TileKind.Exit => (_art.ApartmentDoor, new Color(166, 126, 102)),
                _ => (_art.ApartmentLivingRoomFloor, new Color(164, 138, 124)),
            },
            ApartmentRoomStyle.BurstnerRoom => tile.Kind switch
            {
                TileKind.Floor => (_art.ApartmentBedroomFloor, new Color(214, 196, 186)),
                TileKind.Wall => (_art.ApartmentBedroomWall, new Color(214, 214, 232)),
                TileKind.Exit => (_art.ApartmentDoor, new Color(210, 196, 180)),
                _ => (_art.ApartmentBedroomFloor, new Color(214, 196, 186)),
            },
            _ => tile.Kind switch
            {
                TileKind.Floor => (_art.ApartmentFloor, Color.White),
                TileKind.Wall => (_art.ApartmentWall, Color.White),
                TileKind.Exit => (_art.ApartmentDoor, Color.White),
                _ => (_art.ApartmentFloor, Color.White),
            }
        };
    }

    private ApartmentRoomStyle CurrentApartmentRoomStyle => _currentFloorName switch
    {
        "Bedroom" => ApartmentRoomStyle.Bedroom,
        "Living Room" => ApartmentRoomStyle.LivingRoom,
        "Hallway" => ApartmentRoomStyle.Hallway,
        "Fraulein Burstner's Room" => ApartmentRoomStyle.BurstnerRoom,
        _ => ApartmentRoomStyle.Default,
    };

    private string _currentFloorName = string.Empty;

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

internal sealed class ScissorRasterizerState : RasterizerState
{
    public static readonly ScissorRasterizerState Instance = new();

    private ScissorRasterizerState()
    {
        ScissorTestEnable = true;
    }
}
