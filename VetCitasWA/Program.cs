using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using VetCitasWA.Components;
using VetCitasWA.Servicios.Modelo.Usuario;
using VetCitasWA.Servicios.REST.UsuarioRS;
using VetCitasWA.Servicios.Seguridad;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ===== Autenticación por cookie =====
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opciones =>
    {
        opciones.LoginPath = "/login";
        opciones.AccessDeniedPath = "/login";
        opciones.ExpireTimeSpan = TimeSpan.FromHours(8);
        opciones.SlidingExpiration = true;

        // En cada request HTTP que lleva la cookie, se verifica que el usuario
        // siga activo en la base de datos. Si fue inactivado, se rechaza el
        // principal y se cierra la sesion (captura recargas y navegaciones full-page).
        opciones.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = async context =>
            {
                var principal = context.Principal;
                var idClaim = principal?.FindFirst("IdUsuario")?.Value
                    ?? principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(idClaim, out var idUsuario) || idUsuario <= 0)
                {
                    return;
                }

                // Throttle: evita consultar el backend en cada asset del primer load.
                // Como maximo una verificacion HTTP por minuto por navegador.
                var ahora = DateTimeOffset.UtcNow;
                var ultima = context.Properties.GetString("ultimaRevalidacion");
                if (ultima != null
                    && DateTimeOffset.TryParse(ultima, out var ts)
                    && ahora - ts < TimeSpan.FromMinutes(1))
                {
                    return;
                }

                var usuarioService = context.HttpContext.RequestServices
                    .GetRequiredService<UsuarioRestService>();

                var activo = await usuarioService.EstaActivoAsync(idUsuario);
                if (!activo)
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme);
                    return;
                }

                // Verifica que los roles de la cookie sigan coincidiendo con los de la BD
                // (al SuperAdmin se le excluye porque sus roles son fijos).
                var esSuperAdmin = principal!.HasClaim("EsSuperAdmin", "true");
                if (!esSuperAdmin)
                {
                    try
                    {
                        var adminService = context.HttpContext.RequestServices
                            .GetRequiredService<VetCitasWA.Servicios.REST.UsuarioRS.AdministradorRestService>();

                        var rolesBd = (await Task.Run(() => adminService.ListarRolesDeUsuario(idUsuario)))
                            .Select(r => r.Codigo.ToString())
                            .ToHashSet(StringComparer.OrdinalIgnoreCase);

                        var rolesCookie = principal.FindAll(ClaimTypes.Role)
                            .Select(c => c.Value)
                            .Where(v => v is "ADMINISTRADOR" or "VETERINARIO" or "RECEPCIONISTA")
                            .ToHashSet(StringComparer.OrdinalIgnoreCase);

                        if (!rolesBd.SetEquals(rolesCookie))
                        {
                            context.RejectPrincipal();
                            await context.HttpContext.SignOutAsync(
                                CookieAuthenticationDefaults.AuthenticationScheme);
                            return;
                        }
                    }
                    catch
                    {
                        // Ante un fallo transitorio del backend no se expulsa al usuario.
                    }
                }

                context.Properties.SetString("ultimaRevalidacion", ahora.ToString("o"));
                context.ShouldRenew = true;
            }
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// Revalida el estado de autenticacion del circuito Blazor cada cierto intervalo
// para expulsar a usuarios que fueron inactivados mientras tenian sesion abierta.
builder.Services.AddScoped<AuthenticationStateProvider, RevalidacionAuthStateProvider>();

// Configuración del HttpClient base (Asegúrate de que la URL en appsettings.json sea la correcta)
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration.GetValue<string>("ApiSettings:BaseUrl")!)
});

// Registrar servicios REST - Usuario
builder.Services.AddScoped<VetCitasWA.Servicios.REST.UsuarioRS.AdministradorRestService>();
builder.Services.AddScoped<VetCitasWA.Servicios.REST.UsuarioRS.UsuarioRestService>();
builder.Services.AddScoped<VetCitasWA.Servicios.REST.UsuarioRS.VeterinarioRestService>();
builder.Services.AddScoped<VetCitasWA.Servicios.REST.UsuarioRS.RecepcionistaRestService>();
builder.Services.AddScoped<VetCitasWA.Servicios.REST.UsuarioRS.HorarioVeterinarioRestService>();
builder.Services.AddScoped<VetCitasWA.Servicios.REST.UsuarioRS.PermisoRestService>();

// Registrar servicios REST - Cita
builder.Services.AddScoped<VetCitasWA.Servicios.REST.CitaRS.CitaRestService>();
builder.Services.AddScoped<VetCitasWA.Servicios.REST.CitaRS.AtencionRestService>();
builder.Services.AddScoped<VetCitasWA.Servicios.REST.CitaRS.RecordatorioRestService>();

// Registrar servicios REST - Cliente
builder.Services.AddScoped<VetCitasWA.Servicios.REST.ClienteRS.ClienteRestService>();
builder.Services.AddScoped<VetCitasWA.Servicios.REST.ClienteRS.MascotaRestService>();

// Registrar servicios REST - Servicio
builder.Services.AddScoped<VetCitasWA.Servicios.REST.ServicioRS.ServicioRestService>();

// Servicio de UI - Toasts
builder.Services.AddScoped<VetCitasWA.Servicios.UI.ToastService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// ===== Endpoint de inicio de sesión (POST desde el formulario de Login) =====
// Corre como petición HTTP normal (no por el circuito Blazor), porque SignInAsync
// necesita el HttpContext para escribir la cookie de autenticación.
app.MapPost("/auth/login", async (HttpContext context, UsuarioRestService usuarioService) =>
{
    var form = await context.Request.ReadFormAsync();
    string username = form["username"].ToString();
    string contrasena = form["contrasena"].ToString();

    Usuario? usuario;
    int? intentosRestantes = null;
    try
    {
        usuario = usuarioService.Autenticar(username, contrasena, out intentosRestantes);
    }
    catch (Exception ex)
    {
        // Cuenta bloqueada temporalmente por demasiados intentos fallidos.
        if ((ex.Message ?? "").ToLowerInvariant().Contains("bloquead"))
        {
            return Results.Redirect("/login?error=locked");
        }
        return Results.Redirect("/login?error=server");
    }

    if (usuario is null || !usuario.Activo)
    {
        // Si el backend informó los intentos restantes, se pasan a la vista de login.
        return Results.Redirect(intentosRestantes is int n
            ? $"/login?error=1&intentos={n}"
            : "/login?error=1");
    }

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
        new Claim(ClaimTypes.Name, $"{usuario.Nombres} {usuario.Apellidos}".Trim()),
        new Claim("Username", usuario.Username ?? ""),
        new Claim("IdUsuario", usuario.Id.ToString())
    };
    bool esSuperAdmin = usuario.Id == 1
        || string.Equals(usuario.Username, "superadmin", StringComparison.OrdinalIgnoreCase);

    if (esSuperAdmin)
    {
        claims.Add(new Claim("EsSuperAdmin", "true"));
    }

    // Un claim de rol por cada rol del usuario (permite [Authorize(Roles = "ADMINISTRADOR")])
    foreach (var rol in usuario.Roles)
    {
        claims.Add(new Claim(ClaimTypes.Role, rol.Codigo.ToString()));
    }

    if (esSuperAdmin)
    {
        foreach (var rolExtra in new[] { "ADMINISTRADOR", "VETERINARIO", "RECEPCIONISTA" })
        {
            if (!claims.Any(c => c.Type == ClaimTypes.Role && c.Value == rolExtra))
            {
                claims.Add(new Claim(ClaimTypes.Role, rolExtra));
            }
        }
    }

    var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identidad);
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    return Results.Redirect("/");
}).DisableAntiforgery();

app.MapGet("/auth/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
