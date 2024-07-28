using Beseler.Defaults;
using Beseler.Shared;
using Beseler.Web;
using Serilog;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureLogging();
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

app.MapForwarder($"/api/{Endpoints.Accounts.Refresh}", "http://ApiService", builder =>
{
    builder.RequestTransforms.Add(new PathStringTransform(PathStringTransform.PathTransformMode.RemovePrefix, "/api"));
});
app.MapForwarder($"/api/{Endpoints.Accounts.Login}", "http://ApiService", builder =>
{
    builder.RequestTransforms.Add(new PathStringTransform(PathStringTransform.PathTransformMode.RemovePrefix, "/api"));
});
app.MapForwarder($"/api/{Endpoints.Accounts.Register}", "http://ApiService", builder =>
{
    builder.RequestTransforms.Add(new PathStringTransform(PathStringTransform.PathTransformMode.RemovePrefix, "/api"));
});
app.MapForwarder($"/api/{Endpoints.Accounts.ConfirmEmail}", "http://ApiService", builder =>
{
    builder.RequestTransforms.Add(new PathStringTransform(PathStringTransform.PathTransformMode.RemovePrefix, "/api"));
});
app.MapForwarder($"/api/{Endpoints.Accounts.ResetPassword}", "http://ApiService", builder =>
{
    builder.RequestTransforms.Add(new PathStringTransform(PathStringTransform.PathTransformMode.RemovePrefix, "/api"));
});
app.MapForwarder("/api/weather", "http://ApiService", builder =>
{
    builder.RequestTransforms.Add(new PathStringTransform(PathStringTransform.PathTransformMode.RemovePrefix, "/api"));
});

app.UseStaticFiles();

app.UseSerilogRequestLogging();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Beseler.Web.Client.Routes).Assembly);

app.MapDefaultEndpoints();

app.Run();
