using System;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using System.Text.Json;

public static class Globals
{
    public static float Time { get; set; }
    public static ContentManager Content { get; set; }
    public static Market Market { get; set; }
    public static SpriteBatch SpriteBatch { get; set; }
    public static Point WindowSize { get; set; }
    public static Random Rand { get; set; }
    public static List<Drawable> Ybuffer = new();
    public static List<Drawable> TextBuffer = new();

    // Write JSON indented, and preserve references (don't duplicate objects, use $ref instead)
    public static JsonSerializerOptions JsonOptionsS = new() { 
        WriteIndented = true, 
        Converters = { new Vector2Converter(), new PointConverter() },
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve };

    // Deserialize based on derived types using converters
    public static JsonSerializerOptions JsonOptionsD = new() { 
        WriteIndented = true, 
        Converters = { 
            new TaskConverter(), 
            new TileConverter(), 
            new Vector2Converter(),
            new PointConverter()
        },
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve };

    public static void Update(GameTime gt)
    {
        Time = (float)gt.ElapsedGameTime.TotalSeconds;
    }

    public static string Title(string s)
    {
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.Replace('_', ' ').ToLower());
    }
}