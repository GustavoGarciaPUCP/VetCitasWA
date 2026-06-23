using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using VetCitasWA.Servicios.Modelo.Cita;
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

        // 1. INSERTAR SERVICIO
        public int Insertar(Servicio servicio)
        {
            var response = http.PostAsJsonAsync("ServicioRS/insertar", servicio).GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        // 2. MODIFICAR SERVICIO
        public int Modificar(Servicio servicio)
        {
            var response = http.PutAsJsonAsync("ServicioRS/modificar", servicio).GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        // 3. ELIMINAR SERVICIO POR ID (FÍSICO)
        public int Eliminar(int id)
        {
            var response = http.DeleteAsync($"ServicioRS/eliminar/{id}").GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public int Eliminar(int id, int modifiedBy)
        {
            var response = http.DeleteAsync($"ServicioRS/eliminar/{id}/{modifiedBy}").GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        // 4. LISTAR TODOS LOS SERVICIOS
        public List<Servicio> ListarTodos()
        {
            return http.GetFromJsonAsync<List<Servicio>>("ServicioRS/listarTodos")
                .GetAwaiter().GetResult() ?? new List<Servicio>();
        }

        // 5. BUSCAR SERVICIO POR ID
        public Servicio? BuscarPorId(int id)
        {
            return http.GetFromJsonAsync<Servicio>($"ServicioRS/buscarPorId/{id}")
                .GetAwaiter().GetResult();
        }

        // 6. DESHABILITAR SERVICIO POR ID (LÓGICO)
        public int Deshabilitar(int id)
        {
            var response = http.PutAsync($"ServicioRS/deshabilitar/{id}", null).GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public int Deshabilitar(int id, int modifiedBy)
        {
            var response = http.PutAsync($"ServicioRS/deshabilitar/{id}/{modifiedBy}", null).GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        // 7. FILTRAR SERVICIOS POR NOMBRE O TIPO
        public List<Servicio> ListarPorNombreOTipo(string texto)
        {
            string busqueda = string.IsNullOrWhiteSpace(texto) ? "null" : texto.Trim();
            return http.GetFromJsonAsync<List<Servicio>>($"ServicioRS/buscarPorTexto/{busqueda}")
                .GetAwaiter().GetResult() ?? new List<Servicio>();
        }

        // 8. LISTAR SERVICIOS POR ESTADO (Activo / Inactivo)
        public List<Servicio> ListarPorEstado(bool activo)
        {
            // Forzamos a minúsculas ("true"/"false") para coincidir con el ruteo nativo de Java
            string estadoStr = activo.ToString().ToLower();
            return http.GetFromJsonAsync<List<Servicio>>($"ServicioRS/listarPorEstado/{estadoStr}")
                .GetAwaiter().GetResult() ?? new List<Servicio>();
        }

        // 9. REPORTE / DASHBOARD - TOP SERVICIOS MÁS DEMANDADOS
        public List<ServicioAtencionResumen> TopNMasDemandados(DateTime desde, DateTime hasta, int limite)
        {
            string desdeStr = desde.ToString("yyyy-MM-ddTHH:mm:ss");
            string hastaStr = hasta.ToString("yyyy-MM-ddTHH:mm:ss");

            string url = $"ServicioRS/topDemandados/{desdeStr}/{hastaStr}/{limite}";

            return http.GetFromJsonAsync<List<ServicioAtencionResumen>>(url)
                .GetAwaiter().GetResult() ?? new List<ServicioAtencionResumen>();
        }
    }
}
