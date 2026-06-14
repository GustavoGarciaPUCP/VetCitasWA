using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;

namespace VetCitasWA.Servicios.REST.ClienteRS
{
    public class MascotaRestService
    {
        private readonly HttpClient http;

        public MascotaRestService(HttpClient http)
        {
            this.http = http;
        }

        // 1. INSERTAR MASCOTA
        public int Insertar(Mascota mascota)
        {
            var response = http.PostAsJsonAsync("webresources/MascotaRS/insertar", mascota).GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        // 2. MODIFICAR MASCOTA
        public int Modificar(Mascota mascota)
        {
            var response = http.PutAsJsonAsync("webresources/MascotaRS/modificar", mascota).GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        // 3. ELIMINAR MASCOTA POR ID
        public int Eliminar(int id)
        {
            var response = http.DeleteAsync($"webresources/MascotaRS/eliminar/{id}").GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        // 4. LISTAR TODAS LAS MASCOTAS
        public List<Mascota> ListarTodos()
        {
            return http.GetFromJsonAsync<List<Mascota>>("webresources/MascotaRS/listarTodos")
                .GetAwaiter().GetResult() ?? new List<Mascota>();
        }

        // 5. BUSCAR MASCOTA POR ID
        public Mascota? BuscarPorId(int id)
        {
            return http.GetFromJsonAsync<Mascota>($"webresources/MascotaRS/buscarPorId/{id}")
                .GetAwaiter().GetResult();
        }

        // 6. FILTRAR MASCOTAS POR NOMBRE O NOMBRE DEL DUEÑO
        public List<Mascota> ListarPorNombreODueno(string texto)
        {
            // Si el cuadro de búsqueda está vacío, mandamos "null" para que Java no dé error 404
            string busqueda = string.IsNullOrWhiteSpace(texto) ? "null" : texto.Trim();

            return http.GetFromJsonAsync<List<Mascota>>($"webresources/MascotaRS/buscarPorTexto/{busqueda}")
                .GetAwaiter().GetResult() ?? new List<Mascota>();
        }

        // 7. LISTAR MASCOTAS PERTENECIENTES A UN CLIENTE ESPECIFICO
        public List<Mascota> ListarPorCliente(int idCliente)
        {
            return http.GetFromJsonAsync<List<Mascota>>($"webresources/MascotaRS/listarPorCliente/{idCliente}")
                .GetAwaiter().GetResult() ?? new List<Mascota>();
        }

        // 8. CONTAR MASCOTAS ACTIVAS (Dashboard)
        public int ContarActivas()
        {
            return http.GetFromJsonAsync<int>("webresources/MascotaRS/contarActivas")
                .GetAwaiter().GetResult();
        }
    }
}
