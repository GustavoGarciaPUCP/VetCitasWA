using System.Text.Json.Serialization;

namespace VetCitasWA.Servicios.Modelo.Common.Enums
{
    // El backend serializa este enum como texto ("PERRO" / "GATO").
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TipoEspecie
    {
        PERRO,
        GATO
    }
}
