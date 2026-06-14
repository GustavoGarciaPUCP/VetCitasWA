using System.Collections.Generic;
using VetCitasWA.Servicios.Modelo.Common.Enums;

namespace VetCitasWA.Servicios.Modelo.Usuario
{
    public class RolSistema
    {
        public int Id { get; set; }
        public CodigoRol Codigo { get; set; }
        public string Descripcion { get; set; }
        public List<Permiso> Permisos { get; set; } = new List<Permiso>();
    }
}