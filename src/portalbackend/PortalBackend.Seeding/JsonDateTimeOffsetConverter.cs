using System.Text.Json;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Seeding;

public class JsonDateTimeOffsetConverter: JsonConverter<DateTimeOffset>
{
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (!reader.TryGetDateTimeOffset(out DateTimeOffset value))
            {
                value = DateTimeOffset.Parse(reader.GetString()!);
            }

            return value;
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("O"));
        }
}