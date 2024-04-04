using System;
using System.Text.Json;

namespace VDC.Integration.Domain.Extensions
{
    public static partial class JsonExtensions
    {
        public static object GetObject(this JsonElement element)
        {
            object result;

            result = element.ValueKind switch
            {
                JsonValueKind.Number => GetNumber(element),
                JsonValueKind.False => false,
                JsonValueKind.True => true,
                JsonValueKind.String => GetString(element),
                _ => null,
            };

            return result;
        }

        private static object GetNumber(JsonElement element)
        {
            if (element.TryGetInt32(out int @int))
            {
                return @int;
            }

            return element.GetDecimal();
        }

        private static object GetString(JsonElement element)
        {
            if (element.TryGetDateTime(out DateTime dateTime))
            {
                return dateTime;
            }

            return element.GetString();
        }
    }
}
