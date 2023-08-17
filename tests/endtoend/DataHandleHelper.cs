using System.Text.Json;
using System.Text.Json.Serialization;

namespace EndToEnd.Tests;

public abstract class DataHandleHelper
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
    };

    public static T? DeserializeData<T>(string jsonString)
    {
        var deserializedData = JsonSerializer.Deserialize<T>(jsonString, JsonSerializerOptions);
        return deserializedData;
    }

    public static string SerializeData(object objectToSerialize)
    {
        var serializedData = JsonSerializer.Serialize(objectToSerialize, JsonSerializerOptions);
        return serializedData;
    }
}
