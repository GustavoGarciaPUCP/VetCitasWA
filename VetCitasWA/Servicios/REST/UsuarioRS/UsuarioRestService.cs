using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
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
        {
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
                return null;
            }

            var detalle = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            throw new InvalidOperationException($"El backend rechazo la autenticacion con estado {(int)response.StatusCode}. {detalle}");
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

            http.PostAsync("UsuarioRS/restablecerContrasena", content).GetAwaiter().GetResult();
        }
    }
}