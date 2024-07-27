using Beseler.ApiService;
using Beseler.ApiService.Accounts;
using Beseler.ApiService.Application;
using Beseler.ApiService.Queues;
using Serilog;

var app = WebApplication.CreateBuilder(args)
    .AddServices()
    .Build();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapWeatherEndpoints();
app.MapAccountEndpoints();
app.MapQueueEndpoints();
app.MapDefaultEndpoints();

app.Run();
