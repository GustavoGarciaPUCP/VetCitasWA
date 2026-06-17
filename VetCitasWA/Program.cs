using VetCitasWA.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
