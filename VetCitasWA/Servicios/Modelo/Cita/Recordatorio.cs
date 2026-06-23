using VetCitasWA.Servicios.Modelo.Common.Enums;
using VetCitasWA.Servicios.Modelo.Common.Model;

namespace VetCitasWA.Servicios.Modelo.Cita
{
    public class Recordatorio : EntidadAuditable
    {
        public int Id { get; set; }
        public DateTime FechaProgramada { get; set; }
        public CanalRecordatorio Canal { get; set; }
        public EstadoSeguimiento EstadoSeguimiento { get; set; }
        public string Mensaje { get; set; }
        public Cita Cita { get; set; }
    }
}
