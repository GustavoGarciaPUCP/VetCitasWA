using System.Text.Json.Serialization;

namespace VetCitasWA.Servicios.Modelo.Common.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EstadoSeguimiento
    {
        PENDIENTE,
        ENVIADO
    }
}
