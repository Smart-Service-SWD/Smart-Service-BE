using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartService.API.Helpers;

/// <summary>
/// Custom JsonConverter for Guid that supports multiple formats,
/// including standard hyphenated (D) and non-hyphenated (N) formats.
/// This prevents model binding failures when the frontend sends UUIDs without dashes.
/// </summary>
public class GuidConverter : JsonConverter<Guid>
{
    public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected string for Guid, got {reader.TokenType}");
        }

        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            return Guid.Empty;
        }

        // Guid.TryParse handles N (no dashes), D (hyphenated), B (braces), P (parentheses)
        if (Guid.TryParse(value, out var guid))
        {
            return guid;
        }

        throw new JsonException($"Invalid Guid format: '{value}'. Expected a valid UUID/GUID string.");
    }

    public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
    {
        // Always write in standard hyphenated format for consistency
        writer.WriteStringValue(value.ToString("D"));
    }
}

/// <summary>
/// Custom JsonConverter for nullable Guid.
/// </summary>
public class NullableGuidConverter : JsonConverter<Guid?>
{
    public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected string or null for Guid?, got {reader.TokenType}");
        }

        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (Guid.TryParse(value, out var guid))
        {
            return guid;
        }

        throw new JsonException($"Invalid Guid format: '{value}'. Expected a valid UUID/GUID string or null.");
    }

    public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToString("D"));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
