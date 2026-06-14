using VetCitasWA.Servicios.Modelo.Usuario;

namespace VetCitasWA.Servicios.Modelo.Cita
{
    public class VeterinarioAtencionResumen
    {
        public Veterinario Veterinario { get; set; }
        public int TotalAtenciones { get; set; }
        public double MontoNetoTotal { get; set; }
    }
}
