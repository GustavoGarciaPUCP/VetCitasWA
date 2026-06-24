using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace VetCitasWA.Servicios.PowerBI
{
    /// <summary>
    /// Exporta un reporte de Power BI a PDF usando la API REST (exportToFile)
    /// con autenticacion de service principal (client credentials). Toda la
    /// operacion ocurre del lado servidor; el secreto nunca llega al navegador.
    /// </summary>
    public class PowerBiExportService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;

        private const string Scope = "https://analysis.windows.net/powerbi/api/.default";
        private const string ApiBase = "https://api.powerbi.com/v1.0/myorg";

        public PowerBiExportService(IHttpClientFactory httpFactory, IConfiguration config)
        {
            _httpFactory = httpFactory;
            _config = config;
        }

        public async Task<byte[]> ExportarReportePdfAsync(
            string? fechaInicio = null,
            string? fechaFin = null,
            string? estado = null,
            string? servicio = null,
            CancellationToken ct = default)
        {
            var tenantId = Requerido("PowerBI:TenantId");
            var clientId = Requerido("PowerBI:ClientId");
            var clientSecret = Requerido("PowerBI:ClientSecret");
            var workspaceId = Requerido("PowerBI:WorkspaceId");
            var reportId = Requerido("PowerBI:ReportId");

            var http = _httpFactory.CreateClient();
            http.Timeout = TimeSpan.FromMinutes(5);

            // 1) Token de service principal (client credentials).
            var token = await ObtenerTokenAsync(http, tenantId, clientId, clientSecret, ct);
            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // 2) Disparar la exportacion a PDF (operacion asincrona), pasando los
            //    filtros como parametros del reporte paginado (Report Builder).
            var exportUrl = $"{ApiBase}/groups/{workspaceId}/reports/{reportId}/ExportTo";
            using var exportResp = await http.PostAsJsonAsync(exportUrl,
                ConstruirCuerpoExport(fechaInicio, fechaFin, estado, servicio), ct);
            await GarantizarOkAsync(exportResp, "iniciar la exportacion", ct);

            var exportId = await LeerPropiedadAsync(exportResp, "id", ct)
                ?? throw new InvalidOperationException("Power BI no devolvio el id de exportacion.");

            // 3) Polling hasta que la exportacion termine.
            var estadoUrl = $"{ApiBase}/groups/{workspaceId}/reports/{reportId}/exports/{exportId}";
            string? estadoExport = null;
            for (var intento = 0; intento < 60; intento++)
            {
                ct.ThrowIfCancellationRequested();
                using var estadoResp = await http.GetAsync(estadoUrl, ct);
                await GarantizarOkAsync(estadoResp, "consultar el estado de la exportacion", ct);

                var cuerpoEstado = await estadoResp.Content.ReadAsStringAsync(ct);
                using var docEstado = JsonDocument.Parse(cuerpoEstado);
                estadoExport = docEstado.RootElement.TryGetProperty("status", out var st) ? st.GetString() : null;

                if (string.Equals(estadoExport, "Succeeded", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
                if (string.Equals(estadoExport, "Failed", StringComparison.OrdinalIgnoreCase))
                {
                    // Se incluye el detalle de error que devuelve Power BI para poder diagnosticar
                    // (p. ej. parametro inexistente, formato de valor invalido, o credenciales del origen).
                    var detalle = docEstado.RootElement.TryGetProperty("error", out var err)
                        ? err.ToString()
                        : cuerpoEstado;
                    throw new InvalidOperationException($"La exportacion del reporte de Power BI fallo. {detalle}");
                }

                await Task.Delay(TimeSpan.FromSeconds(3), ct);
            }

            if (!string.Equals(estadoExport, "Succeeded", StringComparison.OrdinalIgnoreCase))
            {
                throw new TimeoutException("La exportacion de Power BI no termino a tiempo. Intenta nuevamente.");
            }

            // 4) Descargar el archivo PDF.
            return await http.GetByteArrayAsync($"{estadoUrl}/file", ct);
        }

        // Arma el cuerpo del ExportTo. Si hay filtros, los manda como parameterValues
        // del reporte paginado (mismos nombres de parametro definidos en el reporte).
        private static object ConstruirCuerpoExport(string? fechaInicio, string? fechaFin, string? estado, string? servicio)
        {
            var parametros = new List<object>();

            // El reporte espera las fechas en formato US con AM/PM (igual que su
            // selector: "6/19/2026 4:00:00 PM"). Inicio a las 12:00:00 AM y fin a las
            // 11:59:59 PM (inclusivo).
            var us = System.Globalization.CultureInfo.GetCultureInfo("en-US");
            if (TryFecha(fechaInicio, out var dIni))
                parametros.Add(new
                {
                    name = "Fromvetcitasdbcitafechahorainicio",
                    value = dIni.Date.ToString("M/d/yyyy h:mm:ss tt", us)
                });

            if (TryFecha(fechaFin, out var dFin))
                parametros.Add(new
                {
                    name = "Tovetcitasdbcitafechahorainicio",
                    value = dFin.Date.AddDays(1).AddSeconds(-1).ToString("M/d/yyyy h:mm:ss tt", us)
                });

            // Nombres reales segun el DAX del reporte (RSCustomDaxFilter).
            if (!string.IsNullOrWhiteSpace(estado) && !estado.Equals("TODOS", StringComparison.OrdinalIgnoreCase))
                parametros.Add(new { name = "vetcitasdbcitaestado", value = estado });

            if (!string.IsNullOrWhiteSpace(servicio) && !servicio.Equals("TODOS", StringComparison.OrdinalIgnoreCase))
                parametros.Add(new { name = "vetcitasdbservicionombre", value = servicio });

            if (parametros.Count == 0)
            {
                return new { format = "PDF" };
            }

            return new
            {
                format = "PDF",
                paginatedReportConfiguration = new { parameterValues = parametros }
            };
        }

        private static bool TryFecha(string? texto, out DateTime fecha)
            => DateTime.TryParse(texto, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out fecha);

        private async Task<string> ObtenerTokenAsync(
            HttpClient http, string tenantId, string clientId, string clientSecret, CancellationToken ct)
        {
            var tokenUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
            var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["scope"] = Scope
            });

            using var resp = await http.PostAsync(tokenUrl, form, ct);
            await GarantizarOkAsync(resp, "obtener el token de Power BI", ct);

            var token = await LeerPropiedadAsync(resp, "access_token", ct);
            return token ?? throw new InvalidOperationException("No se pudo obtener el token de Power BI.");
        }

        private static async Task<string?> LeerPropiedadAsync(HttpResponseMessage resp, string propiedad, CancellationToken ct)
        {
            var json = await resp.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty(propiedad, out var valor) ? valor.GetString() : null;
        }

        private static async Task GarantizarOkAsync(HttpResponseMessage resp, string accion, CancellationToken ct)
        {
            if (resp.IsSuccessStatusCode) return;

            // 429: Power BI limita la cantidad de solicitudes de exportacion por intervalo.
            if ((int)resp.StatusCode == 429)
            {
                var segundos = resp.Headers.RetryAfter?.Delta?.TotalSeconds;
                var espera = segundos.HasValue
                    ? $" Espera ~{(int)segundos.Value} segundos antes de reintentar."
                    : " Espera unos minutos antes de reintentar.";
                throw new InvalidOperationException(
                    "Power BI esta limitando las solicitudes de exportacion (demasiados intentos seguidos)." + espera);
            }

            var detalle = await resp.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"No se pudo {accion} (HTTP {(int)resp.StatusCode}). {detalle}");
        }

        private string Requerido(string clave)
        {
            var valor = _config[clave];
            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new InvalidOperationException(
                    $"Falta la configuracion '{clave}'. Definela con user-secrets (dotnet user-secrets set \"{clave}\" ...).");
            }
            return valor;
        }
    }
}
