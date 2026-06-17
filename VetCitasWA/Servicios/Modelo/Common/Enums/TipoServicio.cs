using System.Text.Json.Serialization;

namespace VetCitasWA.Servicios.Modelo.Common.Enums
{
    // El backend serializa este enum como texto ("CLINICA" / "NO_CLINICA"),
    // por lo que necesitamos el convertidor de string para (de)serializar correctamente.
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TipoServicio
    {
        CLINICA,
        NO_CLINICA
    }
}
