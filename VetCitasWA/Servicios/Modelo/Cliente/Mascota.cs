using System;
using System.Collections.Generic;
using VetCitasWA.Servicios.Modelo.Common.Model;
using VetCitasWA.Servicios.Modelo.Common.Enums;
using VetCitasWA.Servicios.Modelo.Cita;

namespace VetCitasWA.Servicios.Modelo.Cliente
{
    public class Mascota : EntidadAuditable
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public TipoEspecie Especie { get; set; }
        public string Raza { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        public bool Esterilizado { get; set; }
        public bool Activo { get; set; }
        public double Peso { get; set; }

        public Cliente Cliente { get; set; }
        public List<Atencion> Atenciones { get; set; } = new List<Atencion>();
    }
}