using System;
using System.Net.Http;
using System.Text.Json;

namespace VetCitasWA.Servicios.REST
{
    public static class RestResponseExtensions
    {
        public static void EnsureVetCitasSuccess(this HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var detalle = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var mensaje = ExtraerMensaje(detalle);

            if (string.IsNullOrWhiteSpace(mensaje))
            {
                mensaje = $"El backend respondio con estado {(int)response.StatusCode}.";
            }

            throw new InvalidOperationException(mensaje);
        }

        private static string ExtraerMensaje(string detalle)
        {
            if (string.IsNullOrWhiteSpace(detalle))
            {
                return "";
            }

            try
            {
                using var json = JsonDocument.Parse(detalle);
                if (json.RootElement.TryGetProperty("mensaje", out var mensaje))
                {
                    return mensaje.GetString() ?? "";
                }
            }
            catch
            {
                // Si el backend devuelve HTML/texto, usamos el cuerpo como detalle.
            }

            return detalle.Trim();
        }
    }
}
