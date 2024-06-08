using Beseler.ApiService.Accounts;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using TokenResult = (System.Guid Id, string Token, System.DateTime ExpiresOn);

namespace Beseler.ApiService.Application.Jwt;

internal sealed class TokenService
{
    private readonly SigningCredentials _signingCredentials;
    private readonly TokenValidationParameters _validationParameters;
    private readonly JsonWebTokenHandler _handler;
    private readonly JwtOptions _options;
    public string Audience => _options.Audience;

    public TokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
        _handler = new();
        _handler.InboundClaimTypeMap.Clear();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.Key!));
        _signingCredentials = new(key, SecurityAlgorithms.HmacSha256);
        _validationParameters = new()
        {
            IssuerSigningKey = key,
            ValidIssuer = options.Value.Issuer,
            ValidAudience = options.Value.Audience,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            NameClaimType = JwtRegisteredClaimNames.Sub,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    }

    public async Task<ClaimsPrincipal?> ValidateAsync(string token)
    {
        var result = await _handler.ValidateTokenAsync(token, _validationParameters);
        if (result.IsValid is false)
            return null;

        return new ClaimsPrincipal(result.ClaimsIdentity);
    }

    public TokenResult GenerateAccessToken(Account account)
    {
        var tokenId = Guid.NewGuid();
        var expiresOn = DateTime.UtcNow.AddMinutes(_options.AccessTokenLifetimeMinutes);
        var claims = GetDefaultClaims(account, tokenId);

        if (account.IsVerified)
            claims.Add(AppClaims.EmailVerifiedClaim(account.IsVerified));

        var token = WriteToken(claims, expiresOn);

        return (tokenId, token, expiresOn);
    }

    public TokenResult GenerateRefreshToken(Account account)
    {
        var tokenId = Guid.NewGuid();
        var expiresOn = DateTime.UtcNow.AddHours(_options.RefreshTokenLifetimeHours);
        var claims = GetDefaultClaims(account, tokenId);
        var token = WriteToken(claims, expiresOn);

        return (tokenId, token, expiresOn);
    }

    public TokenResult GenerateToken(Account account, TimeSpan lifetime, IEnumerable<Claim>? additionalClaims = null)
    {
        var tokenId = Guid.NewGuid();
        var expiresOn = DateTime.UtcNow.Add(lifetime);
        var claims = GetDefaultClaims(account, tokenId);

        if (additionalClaims is not null)
            claims.AddRange(additionalClaims);

        var token = WriteToken(claims, expiresOn);
        return (tokenId, token, expiresOn);
    }

    private string WriteToken(List<Claim> claims, DateTime expiresOn)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new(claims),
            Expires = expiresOn,
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            SigningCredentials = _signingCredentials
        };

        return _handler.CreateToken(descriptor);
    }

    private static List<Claim> GetDefaultClaims(Account account, Guid tokenId)
    {
        return
        [
            new(JwtRegisteredClaimNames.Jti, tokenId.ToString()),
            new(JwtRegisteredClaimNames.Sub, account.AccountId.ToString())
        ];
    }
}
