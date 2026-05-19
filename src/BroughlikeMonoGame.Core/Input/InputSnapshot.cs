using System;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace BroughlikeMonoGame.Core;

public readonly record struct InputSnapshot(KeyboardState KeyboardState, KeyboardState PreviousKeyboardState)
{
    public static InputSnapshot Create(KeyboardState current, KeyboardState previous) => new(current, previous);

    public bool IsNewKeyPress(Keys key) => KeyboardState.IsKeyDown(key) && PreviousKeyboardState.IsKeyUp(key);

    public string DescribeCurrentKeys() => DescribeKeys(KeyboardState.GetPressedKeys());

    public string DescribePreviousKeys() => DescribeKeys(PreviousKeyboardState.GetPressedKeys());

    private static string DescribeKeys(Keys[] keys)
        => keys.Length == 0 ? "none" : string.Join(',', keys.Select(key => key.ToString()));
}
