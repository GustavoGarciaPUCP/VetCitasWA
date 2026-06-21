using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using VetCitasWA.Servicios.Modelo.Usuario;

namespace VetCitasWA.Servicios.REST.UsuarioRS
{
    public class RecepcionistaRestService
    {
        private readonly HttpClient http;

        public RecepcionistaRestService(HttpClient http)
        {
            this.http = http;
        }

        public int Insertar(Recepcionista recepcionista)
        {
            var response = http.PostAsJsonAsync("RecepcionistaRS/insertar", recepcionista).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public int Modificar(Recepcionista recepcionista)
        {
            var response = http.PutAsJsonAsync("RecepcionistaRS/modificar", recepcionista).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public int Eliminar(int id)
        {
            var response = http.DeleteAsync($"RecepcionistaRS/eliminar/{id}").GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public List<Recepcionista> ListarTodos()
        {
            return http.GetFromJsonAsync<List<Recepcionista>>("RecepcionistaRS/listar")
                .GetAwaiter().GetResult() ?? new List<Recepcionista>();
        }

        public Recepcionista? BuscarPorId(int id)
        {
            return http.GetFromJsonAsync<Recepcionista>($"RecepcionistaRS/buscar/{id}")
                .GetAwaiter().GetResult();
        }
    }
}
