using Microsoft.Xna.Framework.Input;

namespace BroughlikeMonoGame.Desktop.Core;

public readonly record struct InputSnapshot(KeyboardState KeyboardState, KeyboardState PreviousKeyboardState)
{
    public static InputSnapshot Create(KeyboardState current, KeyboardState previous) => new(current, previous);

    public bool IsNewKeyPress(Keys key) => KeyboardState.IsKeyDown(key) && PreviousKeyboardState.IsKeyUp(key);
}
