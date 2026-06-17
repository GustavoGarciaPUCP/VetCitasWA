using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VetCitasWA.Servicios.UI
{
    // El backend usa LocalDateTime (formato "yyyy-MM-ddTHH:mm:ss", SIN zona horaria).
    // System.Text.Json serializa un DateTime con Kind=Local añadiendo el offset (-05:00),
    // lo que rompe la deserialización en Java. Este convertidor fuerza el formato sin zona
    // en ambos sentidos. Aplicable también a DateTime? (System.Text.Json maneja el null).
    public class FechaHoraJsonConverter : JsonConverter<DateTime>
    {
        private const string Formato = "yyyy-MM-ddTHH:mm:ss";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var texto = reader.GetString();
            if (string.IsNullOrWhiteSpace(texto))
                return default;

            // Acepta con o sin zona; tomamos la hora "de reloj" tal cual viene
            if (DateTimeOffset.TryParse(texto, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto))
                return dto.DateTime;

            return DateTime.Parse(texto, CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(Formato, CultureInfo.InvariantCulture));
        }
    }
}
