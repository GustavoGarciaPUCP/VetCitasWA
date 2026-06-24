using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using VetCitasWA.Servicios.REST;
using VetCitasWA.Servicios.Modelo.Usuario;

namespace VetCitasWA.Servicios.REST.UsuarioRS
{
    public class UsuarioRestService
    {
        private readonly HttpClient http;

        public UsuarioRestService(HttpClient http)
        {
            this.http = http;
        }

        public Usuario? Autenticar(string username, string contrasenaPlana)
            => Autenticar(username, contrasenaPlana, out _);

        public Usuario? Autenticar(string username, string contrasenaPlana, out int? intentosRestantes)
        {
            intentosRestantes = null;

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("contrasenaPlana", contrasenaPlana)
            });

            HttpResponseMessage response;
            try
            {
                response = http.PostAsync("UsuarioRS/autenticar", content).GetAwaiter().GetResult();
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException("No se pudo conectar con el backend de VetCitas.", ex);
            }

            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadFromJsonAsync<Usuario>().GetAwaiter().GetResult();
            }

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                // Lee, si viene, el numero de intentos restantes antes del bloqueo.
                var cuerpo = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                try
                {
                    using var json = System.Text.Json.JsonDocument.Parse(cuerpo);
                    if (json.RootElement.TryGetProperty("intentosRestantes", out var ir)
                        && ir.ValueKind == System.Text.Json.JsonValueKind.Number)
                    {
                        intentosRestantes = ir.GetInt32();
                    }
                }
                catch { /* cuerpo no-JSON: se ignora */ }

                return null;
            }

            // 429 Too Many Requests: cuenta bloqueada temporalmente.
            if ((int)response.StatusCode == 429)
            {
                throw new InvalidOperationException(response.ReadVetCitasMessage());
            }

            var detalle = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            throw new InvalidOperationException($"El backend rechazo la autenticacion con estado {(int)response.StatusCode}. {detalle}");
        }

        public async Task<bool> EstaActivoAsync(int idUsuario)
        {
            if (idUsuario <= 0) return false;
            try
            {
                return await http.GetFromJsonAsync<bool>($"UsuarioRS/estaActivo/{idUsuario}");
            }
            catch
            {
                // Ante un fallo transitorio del backend no se cierra la sesion del
                // usuario para evitar desconexiones por errores de red.
                return true;
            }
        }

        public void CambiarContrasena(int idUsuario, string contrasenaActual, string nuevaContrasena)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("idUsuario", idUsuario.ToString()),
                new KeyValuePair<string, string>("contrasenaActual", contrasenaActual),
                new KeyValuePair<string, string>("nuevaContrasena", nuevaContrasena)
            });

            http.PostAsync("UsuarioRS/cambiarContrasena", content).GetAwaiter().GetResult();
        }

        public void RestablecerContrasena(int idUsuarioObjetivo, string nuevaContrasena, int idAdmin)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("idUsuarioObjetivo", idUsuarioObjetivo.ToString()),
                new KeyValuePair<string, string>("nuevaContrasena", nuevaContrasena),
                new KeyValuePair<string, string>("idAdmin", idAdmin.ToString())
            });

            var response = http.PostAsync("UsuarioRS/restablecerContrasena", content).GetAwaiter().GetResult();
            response.EnsureVetCitasSuccess();
        }
    }
}