using System;
using System.Collections.Generic;
using VetCitasWA.Servicios.Modelo.Common.Model;

namespace VetCitasWA.Servicios.Modelo.Cliente
{
    public class Cliente : EntidadAuditable
    {
        public int Id { get; set; }
        public string Dni { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Observaciones { get; set; }
        public bool Activo { get; set; }
        public List<Mascota> Mascotas { get; set; } = new List<Mascota>();
    }
}