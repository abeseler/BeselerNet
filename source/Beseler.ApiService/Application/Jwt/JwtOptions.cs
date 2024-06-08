namespace Beseler.ApiService.Application.Jwt;

internal sealed class JwtOptions : IConfigSection
{
    public static string SectionName => "Jwt";

    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required string Key { get; set; }
    public int AccessTokenLifetimeMinutes { get; set; }
    public int RefreshTokenLifetimeHours { get; set; }
}
