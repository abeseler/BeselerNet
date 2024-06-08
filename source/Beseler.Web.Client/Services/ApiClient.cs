namespace Beseler.Web.Client.Services;

internal sealed class ApiClient(HttpClient client, AuthStateProvider authState)
{
    public async Task<TResponse?> GetSecure<TResponse>(string endpoint) where TResponse : class
    {
        await EnsureAccessNotExpired();
        var response = await client.GetAsync(endpoint);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<TResponse>() : null;
    }

    public async Task<TResponse?> PostSecure<TResponse, TRequest>(string endpoint, TRequest body)
        where TResponse : class
        where TRequest : class
    {
        await EnsureAccessNotExpired();
        var response = await client.PostAsJsonAsync(endpoint, body);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<TResponse>() : null;
    }

    public async Task<HttpResponseMessage> Delete(string endpoint) => await client.DeleteAsync(endpoint);

    private async Task EnsureAccessNotExpired()
    {
        if (authState is { Token: not null, IsExpired: false }) { return; }

        var response = await client.PostAsync(Endpoints.Accounts.Refresh, null);
        if (response.IsSuccessStatusCode is false) { return; }

        var token = await response.Content.ReadFromJsonAsync<AccessTokenResponse>();

        authState.NotifyUserAuthentication(token?.AccessToken, token?.ExpiresOn);
    }
}
