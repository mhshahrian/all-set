using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AllSet.JsonConverters
{
    public class FlexibleDateTimeConverter : JsonConverter<DateTime>
    {
        private readonly string[] _dateFormats = new[]
        {
            "yyyy-M-dTH:mm:ss.fffZ",     // 2025-9-25T8:15:00.000Z
            "yyyy-MM-ddTHH:mm:ss.fffZ",  // 2025-09-25T08:15:00.000Z
            "yyyy-M-dTH:mm:ssZ",         // 2025-9-25T8:15:00Z
            "yyyy-MM-ddTHH:mm:ssZ",      // 2025-09-25T08:15:00Z
            "yyyy-M-dTH:mm:ss",          // 2025-9-25T8:15:00
            "yyyy-MM-ddTHH:mm:ss",       // 2025-09-25T08:15:00
            "yyyy-M-d H:mm:ss",          // 2025-9-25 8:15:00
            "yyyy-MM-dd HH:mm:ss"        // 2025-09-25 08:15:00
        };

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateString = reader.GetString();
            
            if (string.IsNullOrEmpty(dateString))
            {
                throw new JsonException("Invalid date format: empty or null");
            }

            // Try parsing with each format
            foreach (var format in _dateFormats)
            {
                if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var result))
                {
                    return result;
                }
            }

            // Fallback to default parsing
            if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var fallbackResult))
            {
                return fallbackResult;
            }

            throw new JsonException($"Invalid date format: {dateString}");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture));
        }
    }
}
