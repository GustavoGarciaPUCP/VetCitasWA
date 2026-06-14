using VetCitasWA.Servicios.Modelo.Usuario;

namespace VetCitasWA.Servicios.Modelo.Cita
{
    public class ServicioAtencionResumen
    {
        public VetCitasWA.Servicios.Modelo.Servicio.Servicio Servicio { get; set; }
        public int TotalAtenciones { get; set; }
        public double MontoNetoTotal { get; set; }
    }
}
