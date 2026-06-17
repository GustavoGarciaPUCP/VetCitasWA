using System;
using System.Text.Json.Serialization;
using VetCitasWA.Servicios.UI;

namespace VetCitasWA.Servicios.Modelo.Cita
{
    public class Atencion
    {
        public int Id { get; set; }
        [JsonConverter(typeof(FechaHoraJsonConverter))]
        public DateTime FechaHora { get; set; }
        public string NotaClinica { get; set; }
        public string NotaPreOperatoria { get; set; }
        public string NotaPostOperatoria { get; set; }
        public string RecomendacionControl { get; set; }
        public double MontoReferencial { get; set; }
        public double DescuentoAplicado { get; set; }
        public Cita Cita { get; set; }
        public string Diagnostico { get; set; }
    }
}
