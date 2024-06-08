using Beseler.Shared;
using Microsoft.AspNetCore.Http;

namespace Beseler.Defaults;
public sealed class CookieService(IHttpContextAccessor accessor)
{
    private readonly HttpRequest? Request = accessor.HttpContext?.Request;
    private readonly HttpResponse? Response = accessor.HttpContext?.Response;

    public bool TryGetValue(CookieKeys key, out string? value)
    {
        value = null;
        return Request?.Cookies.TryGetValue(key.ToString(), out value) ?? false;
    }

    public void Set(CookieKeys key, string value, DateTimeOffset? expiresOn = null)
    {
        Response?.Cookies.Delete(key.ToString(), GetOptions(key));

        if (string.IsNullOrWhiteSpace(value))
            return;

        Response?.Cookies.Append(key.ToString(), value, GetOptions(key, expiresOn));
    }

    public void Remove(CookieKeys key)
    {
        Response?.Cookies.Delete(key.ToString(), GetOptions(key));
    }

    private static CookieOptions GetOptions(CookieKeys key, DateTimeOffset? expiresOn = null)
    {
        return key switch
        {
            CookieKeys.RefreshToken => new()
            {
                Expires = expiresOn,
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = Endpoints.Accounts.Refresh
            },
            _ => new()
        };
    }
}

public enum CookieKeys
{
    RefreshToken
}
