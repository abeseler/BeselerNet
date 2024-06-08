using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Beseler.Web.Client.Services;

internal sealed class AuthStateProvider(HttpClient http) : AuthenticationStateProvider
{
    private static readonly JsonWebTokenHandler _handler = new();
    private AuthenticationState _state = new(new());
    private DateTimeOffset? _expiresOn;
    private bool _isFirstRun = true;

    public string? Token { get; private set; }
    public bool IsExpired => _expiresOn is not null && _expiresOn.Value < DateTimeOffset.UtcNow;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_isFirstRun)
        {
            _isFirstRun = false;

            var response = await http.PostAsync(Endpoints.Accounts.Refresh, null);
            if (response.IsSuccessStatusCode is false)
                return _state;

            var tokenData = await response.Content.ReadFromJsonAsync<AccessTokenResponse>();
            if (tokenData is null)
                return _state;

            var principal = ParseToken(tokenData.AccessToken);
            if (principal is null)
                return _state;

            Token = tokenData.AccessToken;
            _expiresOn = tokenData.ExpiresOn;
            _state = new AuthenticationState(principal);

            if (_state.User.Identity?.IsAuthenticated ?? false)
                http.DefaultRequestHeaders.Authorization = new("Bearer", Token);
            else
                http.DefaultRequestHeaders.Authorization = null;
        }

        return _state;
    }

    public void NotifyUserAuthentication(string? token = null, DateTimeOffset? expiresOn = null)
    {
        var principal = ParseToken(token);

        Token = principal is not null ? token : null;
        _expiresOn = principal is not null ? expiresOn : null;
        _state = principal is not null ? new AuthenticationState(principal) : new(new());

        http.DefaultRequestHeaders.Authorization = Token is not null ? new("Bearer", Token) : null;

        Log.Information("Authentication state changed: {User}", _state.User.Identity?.Name);
        NotifyAuthenticationStateChanged(Task.FromResult(_state));
    }

    private static ClaimsPrincipal? ParseToken(string? token)
    {
        if (token is null)
            return null;

        var jwt = _handler.ReadJsonWebToken(token);
        if (jwt is null)
        {
            Log.Warning("Token was not a valid json web token: {Token}", token);
            return null;
        }

        var identity = new ClaimsIdentity(jwt.Claims, "JWT", JwtRegisteredClaimNames.Sub, null);
        return new ClaimsPrincipal(identity);
    }
}
