using System;
using System.Text.Json.Serialization;
using VetCitasWA.Servicios.Modelo.Common.Enums;
using VetCitasWA.Servicios.Modelo.Common.Model;
using VetCitasWA.Servicios.Modelo.Cliente; // Para Mascota
using VetCitasWA.Servicios.Modelo.Usuario; // Para Veterinario
using VetCitasWA.Servicios.UI; // FechaHoraJsonConverter

namespace VetCitasWA.Servicios.Modelo.Cita
{
    public class Cita : EntidadAuditable
    {
        public int Id { get; set; }
        [JsonConverter(typeof(FechaHoraJsonConverter))]
        public DateTime FechaHoraInicio { get; set; }
        [JsonConverter(typeof(FechaHoraJsonConverter))]
        public DateTime FechaHoraFin { get; set; }
        public EstadoCita Estado { get; set; }
        public Mascota Mascota { get; set; }
        public Veterinario Veterinario { get; set; }

        public VetCitasWA.Servicios.Modelo.Servicio.Servicio Servicio { get; set; }

        public string MotivoCancelacion { get; set; }
        public string MotivoReprogramacion { get; set; }
        // Nullable: el backend la envía null cuando la cita no está cancelada
        [JsonConverter(typeof(FechaHoraJsonConverter))]
        public DateTime? FechaCancelacion { get; set; }

        public VetCitasWA.Servicios.Modelo.Usuario.Usuario UsuarioCancelacion { get; set; }
    }
}
