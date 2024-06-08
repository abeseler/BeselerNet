namespace Beseler.ApiService.Application.SendGrid;

internal sealed class SendGridOptions : IConfigSection
{
    public static string SectionName => "SendGrid";
    public string? ApiKey { get; set; }
    public string? SenderEmail { get; set; }
    public string? SenderName { get; set; }
}
