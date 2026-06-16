using VetCitasWA.Servicios.Modelo.Cita;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;

namespace VetCitasWA.Servicios.REST.CitaRS
{
    public class AtencionRestService
    {
        private readonly HttpClient http;

        public AtencionRestService(HttpClient http)
        {
            this.http = http;
        }

        // 1. INSERTAR ATENCIÓN
        public int InsertarAtencion(Atencion atencion)
        {
            var response = http.PostAsJsonAsync("AtencionRS/insertar", atencion).GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        // 2. MODIFICAR ATENCIÓN
        public int ModificarAtencion(Atencion atencion)
        {
            var response = http.PutAsJsonAsync("AtencionRS/modificar", atencion).GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        // 3. ELIMINAR ATENCIÓN POR ID
        public int EliminarAtencion(int id)
        {
            var response = http.DeleteAsync($"AtencionRS/eliminar/{id}").GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        // 4. LISTAR TODAS LAS ATENCIONES
        public List<Atencion> ListarTodos()
        {
            return http.GetFromJsonAsync<List<Atencion>>("AtencionRS/listarTodos")
                .GetAwaiter().GetResult() ?? new List<Atencion>();
        }

        // 5. BUSCAR ATENCIÓN POR ID
        public Atencion? BuscarPorId(int id)
        {
            return http.GetFromJsonAsync<Atencion>($"AtencionRS/buscarPorId/{id}")
                .GetAwaiter().GetResult();
        }

        // 6. BUSCAR ATENCIÓN POR ID DE CITA
        public Atencion? BuscarPorCita(int idCita)
        {
            return http.GetFromJsonAsync<Atencion>($"AtencionRS/buscarPorCita/{idCita}")
                .GetAwaiter().GetResult();
        }

        // 7. HISTORIAL MÉDICO DE UNA MASCOTA
        public List<Atencion> ListarHistorialPorMascota(int idMascota)
        {
            return http.GetFromJsonAsync<List<Atencion>>($"AtencionRS/historialMascota/{idMascota}")
                .GetAwaiter().GetResult() ?? new List<Atencion>();
        }

        // 8. FILTRADO AVANZADO CON PARÁMETROS CONSECUTIVOS
        public List<Atencion> ListarFiltradas(int idVeterinario, string estadoCita, DateTime? fecha, string textoBusqueda)
        {
            int idVet = idVeterinario;
            string est = string.IsNullOrWhiteSpace(estadoCita) ? "null" : estadoCita.Trim();
            string fechaStr = fecha.HasValue ? fecha.Value.ToString("yyyy-MM-dd") : "null";
            string texto = string.IsNullOrWhiteSpace(textoBusqueda) ? "null" : textoBusqueda.Trim();

            string url = $"AtencionRS/filtrar/{idVet}/{est}/{fechaStr}/{texto}";

            return http.GetFromJsonAsync<List<Atencion>>(url)
                .GetAwaiter().GetResult() ?? new List<Atencion>();
        }

        // 9. LISTAR ÚLTIMAS ATENCIONES POR VETERINARIO
        public List<Atencion> ListarUltimasPorVeterinario(int idVeterinario, int limite)
        {
            return http.GetFromJsonAsync<List<Atencion>>($"AtencionRS/listarUltimas/{idVeterinario}/{limite}")
                .GetAwaiter().GetResult() ?? new List<Atencion>();
        }

        // 10. CONTAR ATENCIONES POR VETERINARIO EN UN MES
        public int ContarPorVeterinarioEnMes(int idVeterinario, int anio, int mes)
        {
            return http.GetFromJsonAsync<int>($"AtencionRS/contarPorVeterinario/{idVeterinario}/{anio}/{mes}")
                .GetAwaiter().GetResult();
        }

        // 11. DASHBOARD - SUMAR MONTOS NETOS POR MES
        public double SumarMontosNetosPorMes(int anio, int mes)
        {
            return http.GetFromJsonAsync<double>($"AtencionRS/sumarMontosNetos/{anio}/{mes}")
                .GetAwaiter().GetResult();
        }

        // 12. REPORTES - TOP SERVICIOS POR VETERINARIO
        public List<ServicioAtencionResumen> TopServiciosPorVeterinario(int idVeterinario, int anio, int mes, int limite)
        {
            return http.GetFromJsonAsync<List<ServicioAtencionResumen>>($"AtencionRS/topServicios/{idVeterinario}/{anio}/{mes}/{limite}")
                .GetAwaiter().GetResult() ?? new List<ServicioAtencionResumen>();
        }

        // 13. REPORTES - TOP VETERINARIOS CON MÁS ATENCIONES
        public List<VeterinarioAtencionResumen> TopVeterinariosPorAtenciones(int anio, int mes, int limite)
        {
            return http.GetFromJsonAsync<List<VeterinarioAtencionResumen>>($"AtencionRS/topVeterinarios/{anio}/{mes}/{limite}")
                .GetAwaiter().GetResult() ?? new List<VeterinarioAtencionResumen>();
        }
    }
}
