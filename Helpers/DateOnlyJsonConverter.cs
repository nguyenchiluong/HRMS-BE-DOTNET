using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Custom JSON converter for DateOnly that handles multiple date formats
/// </summary>
public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private static readonly string[] DateFormats = new[]
    {
        "yyyy-MM-dd",           // ISO date
        "yyyy-MM-ddTHH:mm:ss",  // ISO datetime without timezone
        "yyyy-MM-ddTHH:mm:ssZ", // ISO datetime with Z
        "yyyy-MM-ddTHH:mm:ss.fffZ", // ISO datetime with milliseconds
        "yyyy-MM-ddTHH:mm:ss.fffffffZ", // ISO datetime with full precision
        "MM/dd/yyyy",           // US format
        "dd/MM/yyyy"            // European format
    };

    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        
        if (string.IsNullOrWhiteSpace(value))
            throw new JsonException("Date value cannot be empty");

        // Try parsing as DateOnly first
        if (DateOnly.TryParse(value, out var dateOnly))
            return dateOnly;

        // Try parsing as DateTime and extract date
        if (DateTime.TryParse(value, out var dateTime))
            return DateOnly.FromDateTime(dateTime);

        // Try specific formats
        foreach (var format in DateFormats)
        {
            if (DateOnly.TryParseExact(value, format, null, System.Globalization.DateTimeStyles.None, out dateOnly))
                return dateOnly;
            
            if (DateTime.TryParseExact(value, format, null, System.Globalization.DateTimeStyles.None, out dateTime))
                return DateOnly.FromDateTime(dateTime);
        }

        throw new JsonException($"Unable to parse '{value}' as a date");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
    }
}

/// <summary>
/// Custom JSON converter for nullable DateOnly
/// </summary>
public class NullableDateOnlyJsonConverter : JsonConverter<DateOnly?>
{
    private readonly DateOnlyJsonConverter _innerConverter = new();

    public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
            return null;

        // Create a new reader to pass to the inner converter
        var bytes = System.Text.Encoding.UTF8.GetBytes($"\"{value}\"");
        var innerReader = new Utf8JsonReader(bytes);
        innerReader.Read(); // Move to the string token
        
        return _innerConverter.Read(ref innerReader, typeof(DateOnly), options);
    }

    public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteStringValue(value.Value.ToString("yyyy-MM-dd"));
        else
            writer.WriteNullValue();
    }
}


