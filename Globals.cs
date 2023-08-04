using System;
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
}