using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using VetCitasWA.Servicios.Modelo.Common.Model;
using VetCitasWA.Servicios.Modelo.Common.Enums;
using VetCitasWA.Servicios.Modelo.Cita;
using VetCitasWA.Servicios.UI;

namespace VetCitasWA.Servicios.Modelo.Cliente
{
    public class Mascota : EntidadAuditable
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        // Nullable: en algunos contextos (p. ej. listado de citas) el backend no puebla la especie
        public TipoEspecie? Especie { get; set; }
        public string Raza { get; set; }

        // El backend maneja LocalDate ("yyyy-MM-dd")
        [JsonConverter(typeof(FechaSoloJsonConverter))]
        public DateTime? FechaNacimiento { get; set; }

        public bool Esterilizado { get; set; }
        public bool Activo { get; set; }
        public double Peso { get; set; }

        public Cliente Cliente { get; set; }
        public List<Atencion> Atenciones { get; set; } = new List<Atencion>();
    }
}