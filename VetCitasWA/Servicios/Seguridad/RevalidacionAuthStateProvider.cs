using System;
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
    /// Si el usuario fue inactivado en la base de datos mientras tenia una sesion
    /// abierta (por ejemplo en otra PC), en la siguiente revalidacion su sesion deja
    /// de ser valida y es redirigido al login.
    /// </summary>
    public class RevalidacionAuthStateProvider : RevalidatingServerAuthenticationStateProvider
    {
        private readonly IServiceScopeFactory scopeFactory;

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
            var usuarioService = scope.ServiceProvider.GetRequiredService<UsuarioRestService>();
            return await usuarioService.EstaActivoAsync(idUsuario);
        }
    }
}
