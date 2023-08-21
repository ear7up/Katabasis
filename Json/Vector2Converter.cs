using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Vector2Converter : JsonConverter<Vector2>
{
    public override bool CanConvert(Type type)
    {
        return typeof(Vector2).IsAssignableFrom(type);
    }

    // Read 
    public override Vector2 Read(
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

        double x = reader.GetDouble();

        reader.Read();
        if (reader.TokenType != JsonTokenType.PropertyName)
            throw new JsonException();

        propertyName = reader.GetString();
        if (propertyName != "Y")
            throw new JsonException();

        reader.Read();
        if (reader.TokenType != JsonTokenType.Number)
            throw new JsonException();

        double y = reader.GetDouble();

        // Read end object
        reader.Read();

        return new Vector2((float)x, (float)y);
    }

    public override void Write(
        Utf8JsonWriter writer,
        Vector2 value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("X", value.X);
        writer.WriteNumber("Y", value.Y);
        writer.WriteEndObject();
    }
}