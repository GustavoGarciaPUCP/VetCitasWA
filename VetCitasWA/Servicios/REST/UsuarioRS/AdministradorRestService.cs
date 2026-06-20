using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using VetCitasWA.Servicios.Modelo.Usuario;

namespace VetCitasWA.Servicios.REST.UsuarioRS
{
    public class AdministradorRestService
    {
        private readonly HttpClient http;

        public AdministradorRestService(HttpClient http)
        {
            this.http = http;
        }

        public int Insertar(Administrador administrador)
        {
            var response = http.PostAsJsonAsync("AdministradorRS/insertar", administrador).GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public int Modificar(Administrador administrador)
        {
            var response = http.PutAsJsonAsync("AdministradorRS/modificar", administrador).GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public int Eliminar(int id)
        {
            var response = http.DeleteAsync($"AdministradorRS/eliminar/{id}").GetAwaiter().GetResult();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public List<Administrador> ListarTodos()
        {
            return http.GetFromJsonAsync<List<Administrador>>("AdministradorRS/listar")
                .GetAwaiter().GetResult() ?? new List<Administrador>();
        }

        public Administrador? BuscarPorId(int id)
        {
            return http.GetFromJsonAsync<Administrador>($"AdministradorRS/buscar/{id}")
                .GetAwaiter().GetResult();
        }

        public List<Usuario> ListarUsuariosFiltrados(string texto, string codigoRol, bool? activo)
        {
            string txtSafe = string.IsNullOrWhiteSpace(texto) ? "" : Uri.EscapeDataString(texto);
            string rolSafe = (string.IsNullOrWhiteSpace(codigoRol) || codigoRol == "Todos los roles")
                             ? "" : Uri.EscapeDataString(codigoRol);

            string url = $"AdministradorRS/listarUsuariosFiltrados?texto={txtSafe}&codigoRol={rolSafe}";

            if (activo.HasValue)
            {
                url += $"&activo={activo.Value}";
            }

            return http.GetFromJsonAsync<List<Usuario>>(url).GetAwaiter().GetResult() ?? new List<Usuario>();
        }

        public int ModificarUsuarioBasico(Usuario usuario, int modifiedBy)
        {
            var response = http.PutAsJsonAsync($"AdministradorRS/modificarUsuarioBasico/{modifiedBy}", usuario)
                .GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            return response.Content.ReadFromJsonAsync<int>().GetAwaiter().GetResult();
        }

        public void AsignarRol(int idUsuario, string codigoRol)
        {
            string rol = Uri.EscapeDataString(codigoRol ?? "");
            var response = http.PostAsync($"AdministradorRS/asignarRol?idUsuario={idUsuario}&codigoRol={rol}", null)
                .GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }

        public void RevocarRol(int idUsuario, string codigoRol)
        {
            string rol = Uri.EscapeDataString(codigoRol ?? "");
            var response = http.PostAsync($"AdministradorRS/revocarRol?idUsuario={idUsuario}&codigoRol={rol}", null)
                .GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }
    }
}
