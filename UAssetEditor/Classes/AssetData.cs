using Newtonsoft.Json;

namespace UAssetEditor.Classes;

public class SerializableProperties : JsonConverter<List<UProperty>>
{
    public override void WriteJson(JsonWriter writer, List<UProperty>? value, JsonSerializer serializer)
    {
        if (value == null)
            return;

        writer.WriteStartObject();
        foreach (var prop in value)
        {
            writer.WritePropertyName(prop.Name);
            writer.WriteValue(prop.Value);
        }
        writer.WriteEndObject();
    }

    public override List<UProperty>? ReadJson(JsonReader reader, Type objectType, List<UProperty>? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}

public class SerializableExportEntry : JsonConverter<ExportContainer>
{
    public override void WriteJson(JsonWriter writer, ExportContainer? value, JsonSerializer serializer)
    {
        writer.WriteStartArray();

        if (value != null)
        {
            foreach (var export in value)
            {
                writer.WriteStartObject();
                
                writer.WritePropertyName("Class");
                writer.WriteValue(export.Class);
                
                writer.WritePropertyName("Name");
                writer.WriteValue(export.Name);
                
                writer.WritePropertyName("Properties");
                
                if (export.TryGetProperties(out var ctn))
                    writer.WriteValue(ctn!.Properties);
                else
                    writer.WriteValue(Array.Empty<object>());
            }
        }
        
        writer.WriteEndArray();
    }

    public override ExportContainer? ReadJson(JsonReader reader, Type objectType, ExportContainer? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}