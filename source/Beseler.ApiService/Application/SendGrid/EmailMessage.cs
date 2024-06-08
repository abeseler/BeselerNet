namespace Beseler.ApiService.Application.SendGrid;

internal sealed record EmailMessage
{
    public required string ToEmail { get; init; }
    public required string ToName { get; init; }
    public required string Subject { get; init; }
    public required string Body { get; init; }
    public required string BodyHtml { get; init; }
}
