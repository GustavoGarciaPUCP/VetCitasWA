using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using VetCitasWA.Servicios.Modelo.Usuario;

namespace VetCitasWA.Servicios.REST.UsuarioRS
{
    public class PermisoRestService
    {
        private readonly HttpClient http;

        public PermisoRestService(HttpClient http)
        {
            this.http = http;
        }

        public int Insertar(Permiso permiso)
        {
            var response = http.PostAsJsonAsync("PermisoRS/insertar", permiso).GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public int Modificar(Permiso permiso)
        {
            var response = http.PutAsJsonAsync("PermisoRS/modificar", permiso).GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public int Eliminar(int id)
        {
            var response = http.DeleteAsync($"PermisoRS/eliminar/{id}").GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public List<Permiso> ListarTodos()
        {
            return http.GetFromJsonAsync<List<Permiso>>("PermisoRS/listar")
                .GetAwaiter().GetResult() ?? new List<Permiso>();
        }

        public Permiso? BuscarPorId(int id)
        {
            return http.GetFromJsonAsync<Permiso>($"PermisoRS/buscar/{id}")
                .GetAwaiter().GetResult();
        }
    }
}