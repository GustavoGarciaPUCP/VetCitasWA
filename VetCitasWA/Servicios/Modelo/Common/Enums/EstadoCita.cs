using System.Text.Json.Serialization;

namespace VetCitasWA.Servicios.Modelo.Common.Enums
{
    // El backend serializa este enum como texto (p. ej. "PENDIENTE").
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EstadoCita
    {
        PENDIENTE,
        CONFIRMADA,
        CANCELADA,
        ATENDIDA,
        NO_ASISTIO,
        EN_CONSULTA
    }
}
