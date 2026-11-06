using UnityEngine;

public enum GameColor { None, Red, Blue, Yellow }
// Detects which color is assigned in the background at the moment
public static class Palette
{
    public static readonly Color Background = new Color(0.94f, 0.93f, 0.89f);

    public static Color Of(GameColor color) => color switch
    {
        GameColor.Red => new Color(0.91f, 0.27f, 0.33f),
        GameColor.Blue => new Color(0.24f, 0.51f, 0.94f),
        GameColor.Yellow => new Color(0.98f, 0.8f, 0.18f),
        _ => Background
    };
}
