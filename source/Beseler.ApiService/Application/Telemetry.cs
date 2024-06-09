using System.Diagnostics;

namespace Beseler.ApiService.Application;

internal static class Telemetry
{
    public static readonly ActivitySource Source = new("Beseler.ApiService");
    public static Activity? StartActivity(string name, string? parentId = null) =>
        Source.StartActivity(name, ActivityKind.Internal, parentId);

    public static void SetTag_AccountId(this Activity activity, int accountId) =>
        activity.SetTag("account.id", accountId);
}
