using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using VetCitasWA.Servicios.Modelo.Cita;
using VetCitasWA.Servicios.REST;

namespace VetCitasWA.Servicios.REST.CitaRS
{
    public class CitaRestService
    {
        private readonly HttpClient http;

        public CitaRestService(HttpClient http)
        {
            this.http = http;
        }

        // 1. INSERTAR CITA
        public int InsertarCita(Cita cita)
        {
            var response = http.PostAsJsonAsync("CitaRS/insertar", cita).GetAwaiter().GetResult();
            response.EnsureVetCitasSuccess();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        // 2. MODIFICAR CITA
        public int ModificarCita(Cita cita)
        {
            var response = http.PutAsJsonAsync("CitaRS/modificar", cita).GetAwaiter().GetResult();
            response.EnsureVetCitasSuccess();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        // 3. ELIMINAR CITA POR ID
        public int EliminarCita(int idCita)
        {
            var response = http.DeleteAsync($"CitaRS/eliminar/{idCita}").GetAwaiter().GetResult();
            response.EnsureVetCitasSuccess();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        // 4. LISTAR TODAS LAS CITAS
        public List<Cita> ListarTodas()
        {
            return http.GetFromJsonAsync<List<Cita>>("CitaRS/listarTodas")
                .GetAwaiter().GetResult() ?? new List<Cita>();
        }

        // 5. OBTENER CITA POR ID
        public Cita? ObtenerPorId(int idCita)
        {
            return http.GetFromJsonAsync<Cita>($"CitaRS/obtenerPorId/{idCita}")
                .GetAwaiter().GetResult();
        }

        // 6. REPROGRAMAR CITA (Recibe LocalDateTime como ISO String)
        public int Reprogramar(int idCita, DateTime fechaInicio, DateTime fechaFin, string motivo, int modifiedBy)
        {
            string inicioStr = fechaInicio.ToString("yyyy-MM-ddTHH:mm:ss");
            string finStr = fechaFin.ToString("yyyy-MM-ddTHH:mm:ss");
            string mot = string.IsNullOrWhiteSpace(motivo) ? "null" : Uri.EscapeDataString(motivo.Trim());

            var response = http.PutAsync($"CitaRS/reprogramar/{idCita}/{inicioStr}/{finStr}/{mot}/{modifiedBy}", null).GetAwaiter().GetResult();
            response.EnsureVetCitasSuccess();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public (bool Ok, int Resultado, string Mensaje) ReprogramarSeguro(int idCita, DateTime fechaInicio, DateTime fechaFin, string motivo, int modifiedBy)
        {
            string inicioStr = fechaInicio.ToString("yyyy-MM-ddTHH:mm:ss");
            string finStr = fechaFin.ToString("yyyy-MM-ddTHH:mm:ss");
            string mot = string.IsNullOrWhiteSpace(motivo) ? "null" : Uri.EscapeDataString(motivo.Trim());

            var response = http.PutAsync($"CitaRS/reprogramar/{idCita}/{inicioStr}/{finStr}/{mot}/{modifiedBy}", null).GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
            {
                return (false, 0, response.ReadVetCitasMessage());
            }

            var resultado = response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
            return (resultado == 1, resultado, resultado == 1 ? "" : "No se pudo reprogramar la cita.");
        }

        // 7. CAMBIAR VETERINARIO
        public int CambiarVeterinario(int idCita, int idNuevoVeterinario, int modifiedBy)
        {
            var response = http.PutAsync($"CitaRS/cambiarVeterinario/{idCita}/{idNuevoVeterinario}/{modifiedBy}", null).GetAwaiter().GetResult();
            response.EnsureVetCitasSuccess();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        // 8. VALIDAR DISPONIBILIDAD DE SLOT
        public bool ValidarDisponibilidadSlot(int idVeterinario, DateTime fechaInicio, DateTime fechaFin)
        {
            string inicioStr = fechaInicio.ToString("yyyy-MM-ddTHH:mm:ss");
            string finStr = fechaFin.ToString("yyyy-MM-ddTHH:mm:ss");

            return http.GetFromJsonAsync<bool>($"CitaRS/validarDisponibilidad/{idVeterinario}/{inicioStr}/{finStr}")
                .GetAwaiter().GetResult();
        }

        // 9. CONTAR CITAS POR ESTADO EN RANGO DE FECHAS (Dashboard)
        public int ContarPorEstadoEnRango(string estado, DateTime desde, DateTime hasta)
        {
            string desdeStr = desde.ToString("yyyy-MM-ddTHH:mm:ss");
            string hastaStr = hasta.ToString("yyyy-MM-ddTHH:mm:ss");
            string est = string.IsNullOrWhiteSpace(estado) ? "null" : estado.Trim();

            return http.GetFromJsonAsync<int>($"CitaRS/contarPorEstado/{est}/{desdeStr}/{hastaStr}")
                .GetAwaiter().GetResult();
        }

        // 10. CONTAR CITAS POR VETERINARIO EN RANGO DE FECHAS
        public int ContarPorVeterinarioEnRango(int idVeterinario, DateTime desde, DateTime hasta)
        {
            string desdeStr = desde.ToString("yyyy-MM-ddTHH:mm:ss");
            string hastaStr = hasta.ToString("yyyy-MM-ddTHH:mm:ss");

            return http.GetFromJsonAsync<int>($"CitaRS/contarPorVeterinario/{idVeterinario}/{desdeStr}/{hastaStr}")
                .GetAwaiter().GetResult();
        }

        // 11. CANCELAR CITA
        public int CancelarCita(int idCita, string motivo, int modifiedBy)
        {
            string mot = string.IsNullOrWhiteSpace(motivo) ? "null" : Uri.EscapeDataString(motivo.Trim());
            var response = http.PutAsync($"CitaRS/cancelar/{idCita}/{mot}/{modifiedBy}", null).GetAwaiter().GetResult();
            response.EnsureVetCitasSuccess();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        // 12. CONFIRMAR CITA
        public int ConfirmarCita(int idCita, int modifiedBy)
        {
            var response = http.PutAsync($"CitaRS/confirmar/{idCita}/{modifiedBy}", null).GetAwaiter().GetResult();
            response.EnsureVetCitasSuccess();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        // 13. MARCAR CITA EN CONSULTA
        public int MarcarEnConsulta(int idCita, int idUsuario)
        {
            var response = http.PutAsync($"CitaRS/marcarEnConsulta/{idCita}/{idUsuario}", null).GetAwaiter().GetResult();
            response.EnsureVetCitasSuccess();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        // 14. MARCAR CITA COMO ATENDIDA
        public int MarcarAtendida(int idCita, int modifiedBy)
        {
            var response = http.PutAsync($"CitaRS/marcarAtendida/{idCita}/{modifiedBy}", null).GetAwaiter().GetResult();
            response.EnsureVetCitasSuccess();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        // 15. MARCAR CITA COMO NO ASISTIÓ
        public int MarcarNoAsistio(int idCita, int modifiedBy)
        {
            var response = http.PutAsync($"CitaRS/marcarNoAsistio/{idCita}/{modifiedBy}", null).GetAwaiter().GetResult();
            response.EnsureVetCitasSuccess();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        // 16. LISTAR POR VETERINARIO Y FECHA ESPECÍFICA (Usa LocalDate)
        public List<Cita> ListarPorVeterinarioYFecha(int idVeterinario, DateTime fecha)
        {
            string fechaStr = fecha.ToString("yyyy-MM-dd");
            return http.GetFromJsonAsync<List<Cita>>($"CitaRS/listarPorVeterinarioFecha/{idVeterinario}/{fechaStr}")
                .GetAwaiter().GetResult() ?? new List<Cita>();
        }

        // 17. FILTRADO CONSOLIDADO AVANZADO (Múltiples PathParam)
        public List<Cita> ListarFiltradas(int idVeterinario, DateTime? fechaInicio, DateTime? fechaFin, string estado, string textoBusqueda)
        {
            int idVet = idVeterinario;
            string inicioStr = fechaInicio.HasValue ? fechaInicio.Value.ToString("yyyy-MM-dd") : "null";
            string finStr = fechaFin.HasValue ? fechaFin.Value.ToString("yyyy-MM-dd") : "null";
            string est = string.IsNullOrWhiteSpace(estado) ? "null" : estado.Trim();
            string texto = string.IsNullOrWhiteSpace(textoBusqueda) ? "null" : textoBusqueda.Trim();

            string url = $"CitaRS/filtrar/{idVet}/{inicioStr}/{finStr}/{est}/{texto}";

            return http.GetFromJsonAsync<List<Cita>>(url)
                .GetAwaiter().GetResult() ?? new List<Cita>();
        }
    }
}
