namespace BroughlikeMonoGame.Desktop.Core;

public static class Layout
{
    public const int TileSize = 64;
    public const int MapTiles = 9;
    public const int UiTilesWide = 4;
    public const int ScreenWidth = TileSize * (MapTiles + UiTilesWide);
    public const int ScreenHeight = TileSize * MapTiles;
}
