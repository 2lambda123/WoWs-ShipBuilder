using System.Globalization;
using MudBlazor;
using MudBlazor.Services;
using NLog.Web;
using Prometheus;
using ReactiveUI;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Web.Extensions;
using WoWsShipBuilder.Web.Services;
using WoWsShipBuilder.Web.Utility;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureAppConfiguration((_, config) =>
{
    config.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);
});

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(options =>
{
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(10);
});
builder.ConfigureShipBuilderOptions();

builder.Logging.ClearProviders();
builder.Host.UseNLog(new() { RemoveLoggerFactoryFilter = false });
SetupExtensions.ConfigureNlog(builder.Configuration.GetValue<bool>("DisableLoki"));

PlatformRegistrationManager.SetRegistrationNamespaces(RegistrationNamespace.Blazor);

builder.Services.UseMicrosoftDependencyResolver();
var resolver = Locator.CurrentMutable;
resolver.InitializeSplat();
resolver.InitializeReactiveUI(RegistrationNamespace.Blazor);

builder.Services.AddMudServices(config => { config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight; });
builder.Services.AddShipBuilderServerServices();

var app = builder.Build();
app.Services.UseMicrosoftDependencyResolver();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
app.UseHttpMetrics(options =>
{
    options.AddCustomLabel("path", context => context.Request.Path);
});
app.UseReferrerTracking();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapMetrics();
    endpoints.MapBlazorHub();
    endpoints.MapFallbackToPage("/_Host");
});

var culture = AppConstants.DefaultCultureDetails.CultureInfo;
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;
Thread.CurrentThread.CurrentCulture = culture;
Thread.CurrentThread.CurrentUICulture = culture;

var dataInitializer = app.Services.GetRequiredService<DataInitializer>();
await dataInitializer.InitializeData();

try
{
    app.Run();
}
finally
{
    NLog.LogManager.Shutdown();
}
