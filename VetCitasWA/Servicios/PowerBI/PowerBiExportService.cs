using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace VetCitasWA.Servicios.PowerBI
{
    public enum TipoReportePowerBi
    {
        Citas,
        Clientes
    }

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
            TipoReportePowerBi tipoReporte,
            string? fechaInicio = null,
            string? fechaFin = null,
            string? estado = null,
            string? servicio = null,
            int? minimoCitasCliente = null,
            CancellationToken ct = default)
        {
            var tenantId = Requerido("PowerBI:TenantId");
            var clientId = Requerido("PowerBI:ClientId");
            var clientSecret = Requerido("PowerBI:ClientSecret");
            var workspaceId = Requerido("PowerBI:WorkspaceId");
            var reportId = ReportIdPara(tipoReporte);

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
                ConstruirCuerpoExport(tipoReporte, fechaInicio, fechaFin, estado, servicio, minimoCitasCliente), ct);
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

        private object ConstruirCuerpoExport(
                    TipoReportePowerBi tipoReporte,
                    string? fechaInicio,
                    string? fechaFin,
                    string? estado,
                    string? servicio,
                    int? minimoCitasCliente)
        {
            var parametros = new List<object>();

            var dIni = TryFecha(fechaInicio, out var f1) ? f1 : new DateTime(2000, 1, 1);
            var dFin = TryFecha(fechaFin, out var f2) ? f2 : new DateTime(2099, 12, 31);

            // ==========================================
            // REPORTE 2: CLIENTES FRECUENTES
            // ==========================================
            if (tipoReporte == TipoReportePowerBi.Clientes)
            {
                parametros.Add(new { name = "FechaInicio", value = dIni.ToString("yyyy-MM-dd HH:mm:ss") });
                parametros.Add(new { name = "FechaFin", value = dFin.ToString("yyyy-MM-dd HH:mm:ss") });

                string nombreParamMinimo = _config["PowerBI:ParametroMinimoCitasClientes"];
                if (string.IsNullOrWhiteSpace(nombreParamMinimo))
                {
                    nombreParamMinimo = "MinimoCitas"; // Rescate por si no se lee el appsettings
                }

                int minVal = minimoCitasCliente.HasValue && minimoCitasCliente.Value > 0 ? minimoCitasCliente.Value : 1;
                parametros.Add(new { name = nombreParamMinimo, value = minVal.ToString(CultureInfo.InvariantCulture) });

                return new
                {
                    format = "PDF",
                    paginatedReportConfiguration = new { parameterValues = parametros }
                };
            }

            // ==========================================
            // REPORTE 1: CONSOLIDADO DE CITAS
            // ==========================================
            parametros.Add(new { name = "Fromvetcitasdbcitafechahorainicio", value = dIni.ToString("yyyy-MM-dd HH:mm:ss") });
            parametros.Add(new { name = "Tovetcitasdbcitafechahorainicio", value = dFin.ToString("yyyy-MM-dd HH:mm:ss") });

            string estadoVal = (!string.IsNullOrWhiteSpace(estado) && !estado.Equals("TODOS", StringComparison.OrdinalIgnoreCase))
                ? estado.Replace(",", "|")
                : "TODOS";
            parametros.Add(new { name = "vetcitasdbcitaestado", value = estadoVal });

            string servicioVal = (!string.IsNullOrWhiteSpace(servicio) && !servicio.Equals("TODOS", StringComparison.OrdinalIgnoreCase))
                ? servicio.Replace(",", "|")
                : "TODOS";
            parametros.Add(new { name = "vetcitasdbservicionombre", value = servicioVal });

            return new
            {
                format = "PDF",
                paginatedReportConfiguration = new { parameterValues = parametros }
            };
        }
        private string ReportIdPara(TipoReportePowerBi tipoReporte)
        {
            return tipoReporte switch
            {
                TipoReportePowerBi.Clientes => Requerido("PowerBI:ReportIdClientes"),
                _ => RequeridoOpcional("PowerBI:ReportIdCitas")
                    ?? Requerido("PowerBI:ReportId")
            };
        }

        private string? RequeridoOpcional(string clave)
        {
            var valor = _config[clave];
            return string.IsNullOrWhiteSpace(valor) ? null : valor;
        }

        private static bool TryFecha(string? texto, out DateTime fecha)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                fecha = default;
                return false;
            }

            // Intenta parsear los formatos más comunes (ISO de HTML5 y formatos locales)
            string[] formatos = { "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy", "d/M/yyyy", "yyyy-MM-ddTHH:mm:ss" };

            if (DateTime.TryParseExact(texto, formatos, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out fecha))
            {
                return true;
            }

            // Último recurso usando la configuración regional del servidor
            return DateTime.TryParse(texto, out fecha);
        }
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
