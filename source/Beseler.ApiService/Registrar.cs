using Beseler.ApiService.Accounts;
using Beseler.ApiService.Accounts.EventHandlers;
using Beseler.ApiService.Application;
using Beseler.ApiService.Application.Jwt;
using Beseler.ApiService.Application.Outbox;
using Beseler.ApiService.Application.SendGrid;
using Dapper;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SendGrid.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using System.Text;

namespace Beseler.ApiService;

internal static class Registrar
{
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
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
                options.ResourceAttributes.Add("service.name", "beseler-api");
            })
            .Enrich.FromLogContext());

        builder.AddServiceDefaults();
        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing.AddSource(Telemetry.Source.Name);
            });

        builder.Services.AddProblemDetails();
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            var handler = new JsonWebTokenHandler();
            handler.InboundClaimTypeMap.Clear();
            options.TokenHandlers.Clear();
            options.TokenHandlers.Add(handler);

            var key = builder.Configuration.GetValue<string>("Jwt:Key") ?? "";
            options.TokenValidationParameters = new()
            {
                ValidIssuer = builder.Configuration.GetValue<string>("Jwt:Issuer"),
                ValidAudience = builder.Configuration.GetValue<string>("Jwt:Issuer"),
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                NameClaimType = JwtRegisteredClaimNames.Sub,
                ClockSkew = TimeSpan.FromSeconds(30)
            };
        });

        builder.Services.AddAuthorization(Policies.AuthorizationOptions);

        builder.Services
            .AddHttpContextAccessor()
            .AddValidatorsFromAssemblyContaining<Program>()
            .AddSingleton<ILogEventEnricher, HttpContextLogEnricher>()
            .AddTransient<IPasswordHasher<Account>, PasswordHasher<Account>>();

        builder.Services
            .BindConfiguration<JwtOptions>()
            .BindConfiguration<SendGridOptions>();

        builder.Services
            .AddSingleton<TokenService>()
            .AddScoped<CookieService>()
            .AddScoped<EmailService>();

        builder.Services
            .AddSingleton<EventHandlingService>()
            .AddScoped<SendVerificationEmailWhenAccountCreatedHandler>()
            .AddScoped<SendAccountLockedEmailWhenAccountLockedHandler>();

        builder.AddNpgsqlDataSource("Default");
        builder.Services
            .AddSingleton<OutboxRepository>()
            .AddScoped<AccountRepository>()
            .AddScoped<TokenRepository>();

        builder.Services.AddHostedService<OutboxMonitor>();

        DefaultTypeMap.MatchNamesWithUnderscores = true;
        SqlMapper.AddTypeHandler(new SqlDateOnlyTypeHandler());

        builder.Services.AddSendGrid(options =>
        {
            var key = builder.Configuration.GetValue<string?>("SendGrid:ApiKey");
            options.ApiKey = string.IsNullOrWhiteSpace(key) ? "TestKey" : key;
        });

        return builder;
    }
}
