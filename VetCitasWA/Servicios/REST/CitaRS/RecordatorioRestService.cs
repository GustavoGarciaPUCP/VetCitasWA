using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using VetCitasWA.Servicios.Modelo.Cita;

namespace VetCitasWA.Servicios.REST.CitaRS
{
    public class RecordatorioRestService
    {
        private readonly HttpClient http;

        public RecordatorioRestService(HttpClient http)
        {
            this.http = http;
        }

        // 1. INSERTAR RECORDATORIO
        public int Insertar(Recordatorio recordatorio)
        {
            var response = http.PostAsJsonAsync("webresources/RecordatorioRS/insertar", recordatorio).GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        // 2. MODIFICAR RECORDATORIO
        public int Modificar(Recordatorio recordatorio)
        {
            var response = http.PutAsJsonAsync("webresources/RecordatorioRS/modificar", recordatorio).GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        // 3. ELIMINAR RECORDATORIO POR ID
        public int Eliminar(int id)
        {
            var response = http.DeleteAsync($"webresources/RecordatorioRS/eliminar/{id}").GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        // 4. LISTAR TODOS LOS RECORDATORIOS
        public List<Recordatorio> ListarTodos()
        {
            return http.GetFromJsonAsync<List<Recordatorio>>("webresources/RecordatorioRS/listarTodos")
                .GetAwaiter().GetResult() ?? new List<Recordatorio>();
        }

        // 5. BUSCAR RECORDATORIO POR ID
        public Recordatorio? BuscarPorId(int id)
        {
            return http.GetFromJsonAsync<Recordatorio>($"webresources/RecordatorioRS/buscarPorId/{id}")
                .GetAwaiter().GetResult();
        }

        // 6. LISTAR POR MASCOTA O CLIENTE (Filtro por Texto)
        public List<Recordatorio> ListarPorMascotaOCliente(string texto)
        {
            string busqueda = string.IsNullOrWhiteSpace(texto) ? "null" : texto.Trim();
            return http.GetFromJsonAsync<List<Recordatorio>>($"webresources/RecordatorioRS/listarPorMascotaCliente/{busqueda}")
                .GetAwaiter().GetResult() ?? new List<Recordatorio>();
        }

        // 7. FILTRAR POR ESTADO Y FECHA (Usa LocalDate)
        public List<Recordatorio> ListarPorEstadoYFecha(string estado, DateTime? fecha)
        {
            string est = string.IsNullOrWhiteSpace(estado) ? "null" : estado.Trim();
            string fechaStr = fecha.HasValue ? fecha.Value.ToString("yyyy-MM-dd") : "null";

            return http.GetFromJsonAsync<List<Recordatorio>>($"webresources/RecordatorioRS/listarPorEstadoFecha/{est}/{fechaStr}")
                .GetAwaiter().GetResult() ?? new List<Recordatorio>();
        }

        // 8. MARCAR RECORDATORIO COMO ENVIADO
        public int MarcarEnviado(int idRecordatorio, int modifiedBy)
        {
            var response = http.PutAsync($"webresources/RecordatorioRS/marcarEnviado/{idRecordatorio}/{modifiedBy}", null).GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        // 9. CONTAR RECORDATORIOS PENDIENTES
        public int ContarPendientes()
        {
            return http.GetFromJsonAsync<int>("webresources/RecordatorioRS/contarPendientes")
                .GetAwaiter().GetResult();
        }
    }
}
