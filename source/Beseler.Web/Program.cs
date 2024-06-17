using Beseler.Defaults;
using Beseler.Shared;
using Beseler.Web;
using Serilog;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Filter.ByExcluding(logEvent =>
        logEvent.Properties.TryGetValue("RequestPath", out var requestPath)
            && requestPath.ToString() switch
            {
                string path when path.StartsWith("\"/_framework") => true,
                string path when path.StartsWith("\"/favicon") => true,
                string path when path.StartsWith("\"/health") => true,
                string path when path.StartsWith("\"/alive") => true,
                _ => false
            })
    .WriteTo.OpenTelemetry(options =>
    {
        options.Endpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]!;
        var headers = builder.Configuration["OTEL_EXPORTER_OTLP_HEADERS"]?.Split(',') ?? [];
        foreach (var header in headers)
        {
            var (key, value) = header.Split('=') switch
            {
            [string k, string v] => (k, v),
                var v => throw new Exception($"Invalid header format {v}")
            };

            options.Headers.Add(key, value);
        }
        options.ResourceAttributes.Add("service.name", "Beseler.Web");
    })
    .Enrich.FromLogContext());

builder.AddServiceDefaults();

builder.Services.AddHttpForwarderWithServiceDiscovery();

builder.Services.AddAntiforgery();
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseHsts();
}

app.MapForwarder($"/api/{Endpoints.Accounts.Refresh}", "http://BeselerApi", builder =>
{
    builder.RequestTransforms.Add(new PathStringTransform(PathStringTransform.PathTransformMode.RemovePrefix, "/api"));
});
app.MapForwarder($"/api/{Endpoints.Accounts.Login}", "http://BeselerApi", builder =>
{
    builder.RequestTransforms.Add(new PathStringTransform(PathStringTransform.PathTransformMode.RemovePrefix, "/api"));
});
app.MapForwarder($"/api/{Endpoints.Accounts.Register}", "http://BeselerApi", builder =>
{
    builder.RequestTransforms.Add(new PathStringTransform(PathStringTransform.PathTransformMode.RemovePrefix, "/api"));
});
app.MapForwarder($"/api/{Endpoints.Accounts.ConfirmEmail}", "http://BeselerApi", builder =>
{
    builder.RequestTransforms.Add(new PathStringTransform(PathStringTransform.PathTransformMode.RemovePrefix, "/api"));
});
app.MapForwarder($"/api/{Endpoints.Accounts.ResetPassword}", "http://BeselerApi", builder =>
{
    builder.RequestTransforms.Add(new PathStringTransform(PathStringTransform.PathTransformMode.RemovePrefix, "/api"));
});
app.MapForwarder("/api/weather", "http://BeselerApi", builder =>
{
    builder.RequestTransforms.Add(new PathStringTransform(PathStringTransform.PathTransformMode.RemovePrefix, "/api"));
});

app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Beseler.Web.Client.Routes).Assembly);

app.MapDefaultEndpoints();

app.Run();
