using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using VetCitasWA.Components;
using VetCitasWA.Servicios.Modelo.Usuario;
using VetCitasWA.Servicios.REST.UsuarioRS;

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
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

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
    try
    {
        usuario = usuarioService.Autenticar(username, contrasena);
    }
    catch
    {
        return Results.Redirect("/login?error=server");
    }

    if (usuario is null || !usuario.Activo)
    {
        return Results.Redirect("/login?error=1");
    }

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
        new Claim(ClaimTypes.Name, $"{usuario.Nombres} {usuario.Apellidos}".Trim()),
        new Claim("Username", usuario.Username ?? ""),
        new Claim("IdUsuario", usuario.Id.ToString())
    };
    // Un claim de rol por cada rol del usuario (permite [Authorize(Roles = "ADMINISTRADOR")])
    foreach (var rol in usuario.Roles)
    {
        claims.Add(new Claim(ClaimTypes.Role, rol.Codigo.ToString()));
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
