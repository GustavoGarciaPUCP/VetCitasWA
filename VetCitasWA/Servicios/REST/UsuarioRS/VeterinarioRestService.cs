using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using VetCitasWA.Servicios.Modelo.Usuario;

namespace VetCitasWA.Servicios.REST.UsuarioRS
{
    public class VeterinarioRestService
    {
        private readonly HttpClient http;

        public VeterinarioRestService(HttpClient http)
        {
            this.http = http;
        }

        public int Insertar(Veterinario veterinario)
        {
            var response = http.PostAsJsonAsync("VeterinarioRS/insertar", veterinario).GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public int Modificar(Veterinario veterinario)
        {
            var response = http.PutAsJsonAsync("VeterinarioRS/modificar", veterinario).GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public int Eliminar(int id)
        {
            var response = http.DeleteAsync($"VeterinarioRS/eliminar/{id}").GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public List<Veterinario> ListarTodos()
        {
            return http.GetFromJsonAsync<List<Veterinario>>("VeterinarioRS/listar")
                .GetAwaiter().GetResult() ?? new List<Veterinario>();
        }

        public Veterinario? BuscarPorId(int id)
        {
            return http.GetFromJsonAsync<Veterinario>($"VeterinarioRS/buscar/{id}")
                .GetAwaiter().GetResult();
        }

        public List<Veterinario> ListarDisponibles(DateTime fechaHoraInicio, int idServicio)
        {
            string fechaStr = fechaHoraInicio.ToString("yyyy-MM-ddTHH:mm:ss");
            return http.GetFromJsonAsync<List<Veterinario>>($"VeterinarioRS/listarDisponibles?fechaHoraInicio={fechaStr}&idServicio={idServicio}")
                .GetAwaiter().GetResult() ?? new List<Veterinario>();
        }
    }
}