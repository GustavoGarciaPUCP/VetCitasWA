using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using VetCitasWA.Servicios.Modelo.Servicio;

namespace VetCitasWA.Servicios.REST.ServicioRS
{
    public class ServicioRestService
    {
        private readonly HttpClient http;

        public ServicioRestService(HttpClient http)
        {
            this.http = http;
        }

        public int Insertar(VetCitasWA.Servicios.Modelo.Servicio.Servicio servicio)
        {
            var response = http.PostAsJsonAsync("ServicioRS/insertar", servicio).GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public int Modificar(VetCitasWA.Servicios.Modelo.Servicio.Servicio servicio)
        {
            var response = http.PutAsJsonAsync("ServicioRS/modificar", servicio).GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public int Eliminar(int id)
        {
            var response = http.DeleteAsync($"ServicioRS/eliminar/{id}").GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public List<VetCitasWA.Servicios.Modelo.Servicio.Servicio> ListarTodos()
        {
            return http.GetFromJsonAsync<List<VetCitasWA.Servicios.Modelo.Servicio.Servicio>>("ServicioRS/listar")
                .GetAwaiter().GetResult() ?? new List<VetCitasWA.Servicios.Modelo.Servicio.Servicio>();
        }

        public VetCitasWA.Servicios.Modelo.Servicio.Servicio? BuscarPorId(int id)
        {
            return http.GetFromJsonAsync<VetCitasWA.Servicios.Modelo.Servicio.Servicio>($"ServicioRS/buscar/{id}")
                .GetAwaiter().GetResult();
        }
    }
}