using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;

namespace Beseler.Defaults;
public sealed class HttpContextLogEnricher(IHttpContextAccessor accessor) : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (accessor.HttpContext is not { } http) { return; }

        if (http.User.Identity is { IsAuthenticated: true, Name: not null } user)
        {
            Activity.Current?.AddTag("account.id", user.Name);
            logEvent.AddPropertyIfAbsent(new LogEventProperty("AccountId", new ScalarValue(user.Name)));
        }            
    }
}
