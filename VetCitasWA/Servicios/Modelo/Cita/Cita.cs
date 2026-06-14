using VetCitasWA.Servicios.Modelo.Common.Enums;

namespace VetCitasWA.Servicios.Modelo.Cita
{
    public class Cita
    {
        public int Id { get; set; }
        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFin { get; set; }
        public EstadoCita Estado { get; set; }
        public Mascota Mascota { get; set; }
        public Veterinario Veterinario { get; set; }
        public Servicio Servicio { get; set; }
        public string MotivoCancelacion { get; set; }
        public string MotivoReprogramacion { get; set; }
        public DateTime FechaCancelacion { get; set; }
        public Usuario UsuarioCancelacion { get; set; }
    }
}
