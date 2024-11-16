using static Microsoft.AspNetCore.Http.StatusCodes;
using static System.Net.Mime.MediaTypeNames.Application;

namespace Beseler.ApiService.Queues;

internal static class QueueEndpoints
{
    public static WebApplication MapQueueEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/queues")
            .WithTags("Queues");

        group.MapGet("", () => TypedResults.Problem("Method not implemented."))
             .Produces<QueueDto[]>(Status200OK, Json);

        group.MapPost("", () => TypedResults.Problem("Method not implemented."))
             .Produces(Status201Created);

        group.MapGet("/{queueName}", (string queueName) => TypedResults.Problem("Method not implemented."))
             .Produces<QueueDto>(Status200OK, Json);

        group.MapPut("/{queueName}", (string queueName) => TypedResults.Problem("Method not implemented."))
             .Produces(Status204NoContent);

        group.MapDelete("/{queueName}", (string queueName) => TypedResults.Problem("Method not implemented."))
             .Produces(Status204NoContent);

        group.MapPost("/{queueName}/messages", (string queueName) => TypedResults.Problem("Method not implemented."))
             .Produces(Status202Accepted);

        group.MapPut("/{queueName}/messages", (string queueName) => TypedResults.Problem("Method not implemented."))
             .Produces<MessageDequeueResponse>(Status200OK, Json);

        group.MapDelete("/{queueName}/messages", (string queueName) => TypedResults.Problem("Method not implemented."))
             .Produces(Status204NoContent);

        return app;
    }
}

public sealed record QueueDto(string QueueName, int MessageLifespanDays, int MessageMaxReceives);
public sealed record MessageEnqueueRequest(string Payload);
public sealed record MessageDequeueResponse(string QueueName, QueuedMessage[] Messages);
public sealed record QueuedMessageDto(string ReceiptHandle, string Payload);
public sealed record MessageDeleteRequest(string ReceiptHandle);
