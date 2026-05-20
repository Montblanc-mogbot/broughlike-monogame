using Microsoft.Xna.Framework.Graphics;

namespace BroughlikeMonoGame.Core;

public sealed record GameArt(
    Texture2D ApartmentFloor,
    Texture2D ApartmentWall,
    Texture2D ApartmentDoor,
    Texture2D ApartmentBed,
    Texture2D ApartmentDresser,
    Texture2D ApartmentFigure,
    Texture2D ApartmentArmchair,
    Texture2D ApartmentSideTable,
    Texture2D ApartmentBedroomFloor,
    Texture2D ApartmentBedroomWall,
    Texture2D ApartmentLivingRoomFloor,
    Texture2D ApartmentLivingRoomWall);
