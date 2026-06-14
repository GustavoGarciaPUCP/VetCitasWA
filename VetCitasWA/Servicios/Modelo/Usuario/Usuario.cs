using System.Collections.Generic;
using VetCitasWA.Servicios.Modelo.Common.Model;

namespace VetCitasWA.Servicios.Modelo.Usuario
{
    public class Usuario : EntidadAuditable
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string ContrasenaHash { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string Email { get; set; }
        public bool Activo { get; set; }
        public string Telefono { get; set; }
        public List<RolSistema> Roles { get; set; } = new List<RolSistema>();
    }
}