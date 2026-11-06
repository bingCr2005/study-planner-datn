using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
namespace StudyPlanner.Api.HocTap;
public sealed class MocThoiGianJsonConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader,Type type,JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (value is null || !Regex.IsMatch(value,@"T.*(?:[zZ]|[+-]\d{2}:\d{2})$") || !DateTimeOffset.TryParse(value,CultureInfo.InvariantCulture,DateTimeStyles.None,out var result))
            throw new JsonException("Thời điểm phải có offset, ví dụ 2026-11-04T19:00:00+07:00.");
        return result.ToUniversalTime();
    }
    public override void Write(Utf8JsonWriter writer,DateTimeOffset value,JsonSerializerOptions options) => writer.WriteStringValue(value.ToUniversalTime());
}
