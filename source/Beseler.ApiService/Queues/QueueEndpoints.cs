namespace Beseler.ApiService.Queues;

internal static class QueueEndpoints
{
    public static WebApplication MapQueueEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/queues")
            .WithTags("Queues");

        group.MapGet("", () => TypedResults.Problem("Method not implemented."));
        group.MapPost("", () => TypedResults.Problem("Method not implemented."));

        group.MapGet("/{queueName}", (string queueName) => TypedResults.Problem("Method not implemented."));
        group.MapPut("/{queueName}", (string queueName) => TypedResults.Problem("Method not implemented."));
        group.MapDelete("/{queueName}", (string queueName) => TypedResults.Problem("Method not implemented."));

        group.MapPost("/{queueName}/messages", (string queueName) => TypedResults.Problem("Method not implemented."));
        group.MapPut("/{queueName}/messages", (string queueName) => TypedResults.Problem("Method not implemented."));
        group.MapDelete("/{queueName}/messages", (string queueName) => TypedResults.Problem("Method not implemented."));

        return app;
    }
}
