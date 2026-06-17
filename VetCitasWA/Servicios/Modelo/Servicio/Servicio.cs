using System.Text.Json.Serialization;
using VetCitasWA.Servicios.Modelo.Common.Model;
using VetCitasWA.Servicios.Modelo.Common.Enums;

namespace VetCitasWA.Servicios.Modelo.Servicio
{
    public class Servicio : EntidadAuditable
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }

        // El backend Java expone este campo como "tipoServicio"
        [JsonPropertyName("tipoServicio")]
        public TipoServicio Tipo { get; set; }

        public int DuracionMinutos { get; set; }

        // El backend Java expone este campo como "precioReferencial"
        [JsonPropertyName("precioReferencial")]
        public double Precio { get; set; }

        public bool Activo { get; set; }
    }
}
