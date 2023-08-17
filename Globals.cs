using System;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

public static class Globals
{
    public static float Time { get; set; }
    public static ContentManager Content { get; set; }
    public static SpriteBatch SpriteBatch { get; set; }
    public static Point WindowSize { get; set; }
    public static Random Rand { get; set; }
    public static List<Drawable> Ybuffer = new();

    public static void Update(GameTime gt)
    {
        Time = (float)gt.ElapsedGameTime.TotalSeconds;
    }

    public static string Title(string s)
    {
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.Replace('_', ' ').ToLower());
    }

    public static float UndoScaleUp(float scale)
    {
        return (1f / (1 + scale) + 1f);
    }

    public static float UndoScaleDown(float scale)
    {
        return (1f / (1 - scale) - 1f);
    }
}