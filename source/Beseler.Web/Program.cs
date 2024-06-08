using Beseler.Defaults;
using Beseler.Web;
using Serilog;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
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
        options.ResourceAttributes.Add("service.name", "beseler-web");
    })
    .Enrich.FromLogContext());

builder.AddServiceDefaults();

builder.Services.AddHttpForwarderWithServiceDiscovery();

builder.Services.AddAntiforgery();
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

var app = builder.Build();

app.MapForwarder("/api/{**catch-all}", "http://BeselerApi", builder =>
{
    builder.RequestTransforms.Add(new PathStringTransform(PathStringTransform.PathTransformMode.RemovePrefix, "/api"));
});

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseHsts();
}
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Beseler.Web.Client.Routes).Assembly);

app.MapDefaultEndpoints();

app.Run();
