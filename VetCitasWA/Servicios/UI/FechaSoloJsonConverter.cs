using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VetCitasWA.Servicios.UI
{
    // El backend usa LocalDate (formato "yyyy-MM-dd"). System.Text.Json escribiría
    // un DateTime como "...T00:00:00", lo que rompe la deserialización en Java.
    // Este convertidor fuerza el formato solo-fecha en ambos sentidos.
    public class FechaSoloJsonConverter : JsonConverter<DateTime?>
    {
        private const string Formato = "yyyy-MM-dd";

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            var texto = reader.GetString();
            if (string.IsNullOrWhiteSpace(texto))
                return null;

            // Acepta "yyyy-MM-dd" y también ISO con hora por si acaso
            if (DateTime.TryParse(texto, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))
                return fecha;

            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value.ToString(Formato, CultureInfo.InvariantCulture));
            else
                writer.WriteNullValue();
        }
    }
}
