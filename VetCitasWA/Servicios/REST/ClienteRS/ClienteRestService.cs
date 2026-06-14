using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;

namespace VetCitasWA.Servicios.REST.ClienteRS
{
    public class ClienteRestService
    {
        private readonly HttpClient http;

        public ClienteRestService(HttpClient http)
        {
            this.http = http;
        }

        // 1. INSERTAR CLIENTE
        public int Insertar(Cliente cliente)
        {
            var response = http.PostAsJsonAsync("webresources/ClienteRS/insertar", cliente).GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        // 2. MODIFICAR CLIENTE
        public int Modificar(Cliente cliente)
        {
            var response = http.PutAsJsonAsync("webresources/ClienteRS/modificar", cliente).GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        // 3. ELIMINAR CLIENTE POR ID
        public int Eliminar(int id)
        {
            var response = http.DeleteAsync($"webresources/ClienteRS/eliminar/{id}").GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        // 4. LISTAR TODOS LOS CLIENTES
        public List<Cliente> ListarTodos()
        {
            return http.GetFromJsonAsync<List<Cliente>>("webresources/ClienteRS/listarTodos")
                .GetAwaiter().GetResult() ?? new List<Cliente>();
        }

        // 5. BUSCAR CLIENTE POR ID
        public Cliente? BuscarPorId(int id)
        {
            return http.GetFromJsonAsync<Cliente>($"webresources/ClienteRS/buscarPorId/{id}")
                .GetAwaiter().GetResult();
        }

        // 6. FILTRAR CLIENTES POR NOMBRE, APELLIDO O DNI
        public List<Cliente> ListarPorNombreApellidoODni(string texto)
        {
            // Si el texto viene vacío o nulo, pasamos el comodín "null" para no romper el PathParam
            string busqueda = string.IsNullOrWhiteSpace(texto) ? "null" : texto.Trim();

            return http.GetFromJsonAsync<List<Cliente>>($"webresources/ClienteRS/buscarPorTexto/{busqueda}")
                .GetAwaiter().GetResult() ?? new List<Cliente>();
        }

        // 7. CONTAR CLIENTES ACTIVOS (Métrica de Dashboard)
        public int ContarActivos()
        {
            return http.GetFromJsonAsync<int>("webresources/ClienteRS/contarActivos")
                .GetAwaiter().GetResult();
        }

        // 8. CONTAR NUEVOS CLIENTES REGISTRADOS EN UN MES EN ESPECÍFICO
        public int ContarNuevosEnMes(int anio, int mes)
        {
            return http.GetFromJsonAsync<int>($"webresources/ClienteRS/contarNuevos/{anio}/{mes}")
                .GetAwaiter().GetResult();
        }
    }
}
