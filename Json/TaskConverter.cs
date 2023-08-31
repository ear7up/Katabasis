using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class TaskConverter : JsonConverter<Task>
{
    public override bool CanConvert(Type type)
    {
        return typeof(Task).IsAssignableFrom(type);
    }

    // Read 
    public override Task Read(
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

        TaskDiscriminator typeDiscriminator = (TaskDiscriminator)readerClone.GetInt32();

        JsonSerializerOptions jsonOptions = new() { 
            WriteIndented = true, 
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve };

        Task task = typeDiscriminator switch 
        {
            TaskDiscriminator.Task => JsonSerializer.Deserialize<Task>(ref reader!, jsonOptions),
            TaskDiscriminator.BuyFromMarketTask => JsonSerializer.Deserialize<BuyFromMarketTask>(ref reader!, jsonOptions),
            TaskDiscriminator.BuyTask => JsonSerializer.Deserialize<BuyTask>(ref reader!, jsonOptions),
            TaskDiscriminator.CookTask => JsonSerializer.Deserialize<CookTask>(ref reader!, jsonOptions),
            TaskDiscriminator.DepositInventoryTask => JsonSerializer.Deserialize<DepositInventoryTask>(ref reader!, jsonOptions),
            TaskDiscriminator.EatTask => JsonSerializer.Deserialize<EatTask>(ref reader!, jsonOptions),
            TaskDiscriminator.FindBuildingTask => JsonSerializer.Deserialize<FindBuildingTask>(ref reader!, jsonOptions),
            TaskDiscriminator.FindNewHomeTask => JsonSerializer.Deserialize<FindNewHomeTask>(ref reader!, jsonOptions),
            TaskDiscriminator.FindTileByTypeTask => JsonSerializer.Deserialize<FindTileByTypeTask>(ref reader!, jsonOptions),
            TaskDiscriminator.GoToTask => JsonSerializer.Deserialize<GoToTask>(ref reader!, jsonOptions),
            TaskDiscriminator.IdleAtHomeTask => JsonSerializer.Deserialize<IdleAtHomeTask>(ref reader!, jsonOptions),
            TaskDiscriminator.SellAtMarketTask => JsonSerializer.Deserialize<SellAtMarketTask>(ref reader!, jsonOptions),
            TaskDiscriminator.SellTask => JsonSerializer.Deserialize<SellTask>(ref reader!, jsonOptions),
            TaskDiscriminator.SourceGoodsTask => JsonSerializer.Deserialize<SourceGoodsTask>(ref reader!, jsonOptions),
            TaskDiscriminator.BuildTask => JsonSerializer.Deserialize<BuildTask>(ref reader!, jsonOptions),
            TaskDiscriminator.TryToProduceTask => JsonSerializer.Deserialize<TryToProduceTask>(ref reader!, jsonOptions),
            _ => throw new JsonException()
        };

        return task;
    }

    public override void Write(
        Utf8JsonWriter writer,
        Task value,
        JsonSerializerOptions options)
    {
        // Rather than implement a write function for reach type,
        // we'll just write a Discriminator field as part of the serializable fields for a Task

        /*
        writer.WriteStartObject();

        int taskTypeId = Array.FindIndex(TaskTypes, x => x == value.GetType());
        writer.WriteNumber("TypeDiscriminator", taskTypeId);
        writer.WritePropertyName("TypeValue");

        dynamic casted = Convert.ChangeType(value, value.GetType());
        
        // Including TaskConverter results in an infinite loop here
        // But not including it means subtasks don't get the TypeDiscriminator attribute
        JsonSerializerOptions JsonOptions = new() { 
            WriteIndented = true, 
            // Converters = { new TaskConverter() },
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve 
        };
        JsonSerializer.Serialize(writer, casted, JsonOptions);

        writer.WriteEndObject();
        */

        //JsonSerializer.Serialize(writer, value, Globals.JsonOptions);
    }
}