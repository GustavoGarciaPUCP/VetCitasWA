using System.Text.Json.Serialization;

namespace VetCitasWA.Servicios.Modelo.Usuario
{
    public class Veterinario : Usuario
    {
        // El backend Java expone este campo como "cmpv"
        [JsonPropertyName("cmpv")]
        public string Cmp { get; set; }

        public string Especialidad { get; set; }
    }
}