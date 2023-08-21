using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class TileConverter : JsonConverter<Tile>
{
    public override bool CanConvert(Type type)
    {
        return typeof(Tile).IsAssignableFrom(type);
    }

    // Read 
    public override Tile Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        Utf8JsonReader readerClone = reader;

        if (readerClone.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        // Skip $id
        readerClone.Read();
        readerClone.Read();

        readerClone.Read();
        if (readerClone.TokenType != JsonTokenType.PropertyName)
            throw new JsonException();

        string propertyName = readerClone.GetString();
        if (propertyName != "Discriminator")
            throw new JsonException();

        readerClone.Read();
        if (readerClone.TokenType != JsonTokenType.Number)
            throw new JsonException();

        TileDiscriminator typeDiscriminator = (TileDiscriminator)readerClone.GetInt32();

        JsonSerializerOptions jsonOptions = new() { 
            WriteIndented = true, 
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve };

        Tile Tile = typeDiscriminator switch 
        {
            TileDiscriminator.Tile => JsonSerializer.Deserialize<Tile>(ref reader!, jsonOptions),
            TileDiscriminator.TileAnimal => JsonSerializer.Deserialize<TileAnimal>(ref reader!, jsonOptions),
            _ => throw new JsonException()
        };

        return Tile;
    }

    public override void Write(
        Utf8JsonWriter writer,
        Tile value,
        JsonSerializerOptions options)
    {

    }
}