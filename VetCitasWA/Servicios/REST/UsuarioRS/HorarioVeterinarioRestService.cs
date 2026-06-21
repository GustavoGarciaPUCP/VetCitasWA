using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using VetCitasWA.Servicios.Modelo.Usuario;

namespace VetCitasWA.Servicios.REST.UsuarioRS
{
    public class HorarioVeterinarioRestService
    {
        private readonly HttpClient http;

        public HorarioVeterinarioRestService(HttpClient http)
        {
            this.http = http;
        }

        public int Insertar(HorarioVeterinario horario)
        {
            var response = http.PostAsJsonAsync("HorarioVeterinarioRS/insertar", horario).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public int Modificar(HorarioVeterinario horario)
        {
            var response = http.PutAsJsonAsync("HorarioVeterinarioRS/modificar", horario).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public int Eliminar(int id)
        {
            var response = http.DeleteAsync($"HorarioVeterinarioRS/eliminar/{id}").GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public List<HorarioVeterinario> ListarTodos()
        {
            return http.GetFromJsonAsync<List<HorarioVeterinario>>("HorarioVeterinarioRS/listar")
                .GetAwaiter().GetResult() ?? new List<HorarioVeterinario>();
        }

        public HorarioVeterinario? BuscarPorId(int id)
        {
            return http.GetFromJsonAsync<HorarioVeterinario>($"HorarioVeterinarioRS/buscar/{id}")
                .GetAwaiter().GetResult();
        }

        public List<HorarioVeterinario> ListarPorVeterinario(int idVeterinario)
        {
            return http.GetFromJsonAsync<List<HorarioVeterinario>>($"HorarioVeterinarioRS/listarPorVeterinario/{idVeterinario}")
                .GetAwaiter().GetResult() ?? new List<HorarioVeterinario>();
        }

        public List<HorarioVeterinario> ListarHorarioSemanalPorVeterinario(int idVeterinario)
        {
            return http.GetFromJsonAsync<List<HorarioVeterinario>>($"HorarioVeterinarioRS/listarHorarioSemanalPorVeterinario/{idVeterinario}")
                .GetAwaiter().GetResult() ?? new List<HorarioVeterinario>();
        }
    }
}
