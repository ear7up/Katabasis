using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class PointConverter : JsonConverter<Point>
{
    public override bool CanConvert(Type type)
    {
        return typeof(Point).IsAssignableFrom(type);
    }

    // Read 
    public override Point Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        reader.Read();
        if (reader.TokenType != JsonTokenType.PropertyName)
            throw new JsonException();

        string propertyName = reader.GetString();
        if (propertyName != "X")
            throw new JsonException();

        reader.Read();
        if (reader.TokenType != JsonTokenType.Number)
            throw new JsonException();

        int x = reader.GetInt32();

        reader.Read();
        if (reader.TokenType != JsonTokenType.PropertyName)
            throw new JsonException();

        propertyName = reader.GetString();
        if (propertyName != "Y")
            throw new JsonException();

        reader.Read();
        if (reader.TokenType != JsonTokenType.Number)
            throw new JsonException();

        int y = reader.GetInt32();

        // Read end object
        reader.Read();

        return new Point(x, y);
    }

    public override void Write(
        Utf8JsonWriter writer,
        Point value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("X", value.X);
        writer.WriteNumber("Y", value.Y);
        writer.WriteEndObject();
    }
}