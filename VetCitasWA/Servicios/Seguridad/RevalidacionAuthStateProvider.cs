using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VetCitasWA.Servicios.REST.UsuarioRS;

namespace VetCitasWA.Servicios.Seguridad
{
    /// <summary>
    /// Revalida periodicamente el estado de autenticacion del circuito Blazor Server.
    /// Si el usuario fue inactivado, o si le cambiaron los roles mientras tenia una
    /// sesion abierta (por ejemplo en otra PC), en la siguiente revalidacion su sesion
    /// deja de ser valida y es redirigido al login (donde recibe los roles actualizados).
    /// </summary>
    public class RevalidacionAuthStateProvider : RevalidatingServerAuthenticationStateProvider
    {
        private readonly IServiceScopeFactory scopeFactory;

        private static readonly HashSet<string> RolesValidos =
            new(StringComparer.OrdinalIgnoreCase) { "ADMINISTRADOR", "VETERINARIO", "RECEPCIONISTA" };

        public RevalidacionAuthStateProvider(
            ILoggerFactory loggerFactory,
            IServiceScopeFactory scopeFactory) : base(loggerFactory)
        {
            this.scopeFactory = scopeFactory;
        }

        // Cada cuanto se vuelve a verificar la sesion del circuito conectado.
        protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(2);

        protected override async Task<bool> ValidateAuthenticationStateAsync(
            AuthenticationState authenticationState, CancellationToken cancellationToken)
        {
            var user = authenticationState?.User;
            if (user?.Identity is null || !user.Identity.IsAuthenticated)
            {
                return false;
            }

            var idClaim = user.FindFirst("IdUsuario")?.Value
                ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(idClaim, out var idUsuario) || idUsuario <= 0)
            {
                return false;
            }

            await using var scope = scopeFactory.CreateAsyncScope();

            // 1. El usuario debe seguir activo.
            var usuarioService = scope.ServiceProvider.GetRequiredService<UsuarioRestService>();
            if (!await usuarioService.EstaActivoAsync(idUsuario))
            {
                return false;
            }

            // 2. Los roles de la cookie deben coincidir con los actuales en la BD.
            //    Al SuperAdmin se le excluye (sus roles son fijos / se le agregan todos al entrar).
            var esSuperAdmin = user.HasClaim("EsSuperAdmin", "true");
            if (!esSuperAdmin)
            {
                try
                {
                    var adminService = scope.ServiceProvider.GetRequiredService<AdministradorRestService>();
                    var rolesBd = (await Task.Run(() => adminService.ListarRolesDeUsuario(idUsuario)))
                        .Select(r => r.Codigo.ToString())
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    var rolesCookie = user.FindAll(ClaimTypes.Role)
                        .Select(c => c.Value)
                        .Where(v => RolesValidos.Contains(v))
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    // Si cambiaron (se agrego o quito algun rol), se invalida la sesion.
                    if (!rolesBd.SetEquals(rolesCookie))
                    {
                        return false;
                    }
                }
                catch
                {
                    // Ante un fallo transitorio del backend no se expulsa al usuario.
                }
            }

            return true;
        }
    }
}
