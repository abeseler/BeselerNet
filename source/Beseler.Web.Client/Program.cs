using Beseler.Web.Client.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Serilog.Core;
using Serilog.Debugging;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

SelfLog.Enable(m => Console.Error.WriteLine(m));
var levelSwitch = new LoggingLevelSwitch();
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.ControlledBy(levelSwitch)
    .WriteTo.BrowserConsole()
    .CreateLogger();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri($"{builder.HostEnvironment.BaseAddress}api/") });
builder.Services.AddScoped<ApiClient>();

builder.Services.AddScoped<AuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(p => p.GetRequiredService<AuthStateProvider>());
builder.Services.AddAuthorizationCore(Policies.AuthorizationOptions);
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddBlazoredLocalStorage();

await builder.Build().RunAsync();
