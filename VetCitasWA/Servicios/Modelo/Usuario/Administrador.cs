namespace VetCitasWA.Servicios.Modelo.Usuario
{
    public class Administrador : Usuario
    {
        public string Area { get; set; }
        public bool EsSuperAdmin { get; set; }
    }
}