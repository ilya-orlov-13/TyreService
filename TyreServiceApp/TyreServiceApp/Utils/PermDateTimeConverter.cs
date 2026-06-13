using System.Text.Json;
using System.Text.Json.Serialization;

namespace TyreServiceApp.Utils;

public class PermDateTimeConverter : JsonConverter<DateTime>
{
    private static readonly TimeSpan PermOffset = TimeSpan.FromHours(5);

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        if (string.IsNullOrEmpty(str)) return default;
        return DateTime.Parse(str);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        if (value.Kind == DateTimeKind.Unspecified)
        {
            writer.WriteStringValue(new DateTimeOffset(value, PermOffset).ToString("O"));
        }
        else
        {
            writer.WriteStringValue(value.ToString("O"));
        }
    }
}

public class PermNullableDateTimeConverter : JsonConverter<DateTime?>
{
    private static readonly TimeSpan PermOffset = TimeSpan.FromHours(5);

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        if (string.IsNullOrEmpty(str)) return null;
        return DateTime.Parse(str);
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        var dt = value.Value;
        if (dt.Kind == DateTimeKind.Unspecified)
        {
            writer.WriteStringValue(new DateTimeOffset(dt, PermOffset).ToString("O"));
        }
        else
        {
            writer.WriteStringValue(dt.ToString("O"));
        }
    }
}
