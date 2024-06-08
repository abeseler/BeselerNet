namespace Beseler.Shared.Responses;
public sealed record AccessTokenResponse(string AccessToken, DateTimeOffset ExpiresOn);
