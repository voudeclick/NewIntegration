using System;
using System.Buffers;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using VDC.Integration.APIClient.Shopify.Models.Request;

namespace VDC.Integration.APIClient.Converters
{
    public class DecimalToOptionalConverter : JsonConverter<Optional<decimal?>>
    {
        public override Optional<decimal?> Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
        {

            if (reader.TokenType == JsonTokenType.String)
            {
                ReadOnlySpan<byte> span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
                if (Utf8Parser.TryParse(span, out decimal number, out int bytesConsumed) && span.Length == bytesConsumed)
                    return new Optional<decimal?>(number);

                if (Decimal.TryParse(reader.GetString(), out number))
                    return new Optional<decimal?>(number);
            }

            return new Optional<decimal?>(reader.GetDecimal());
        }

        public override void Write(Utf8JsonWriter writer, Optional<decimal?> value, JsonSerializerOptions options)
        {
            if (!value.Value.HasValue)
                writer.WriteNullValue();
            else
                writer.WriteNumberValue(value.Value.Value);
        }
    }
}

