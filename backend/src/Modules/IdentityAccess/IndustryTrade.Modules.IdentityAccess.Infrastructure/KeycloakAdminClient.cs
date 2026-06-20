using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using IndustryTrade.Modules.IdentityAccess.Application.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IndustryTrade.Modules.IdentityAccess.Infrastructure;

/// <summary>
/// Resets a user's password via the Keycloak Admin REST API: obtain an admin token (admin-cli on the
/// master realm), find the user by username, then set a temporary password. Configuration lives under
/// the "Keycloak" section (AdminBaseUrl, Realm, AdminUser, AdminPassword, DefaultUserPassword).
/// </summary>
internal sealed class KeycloakAdminClient(
    IHttpClientFactory httpFactory, IConfiguration config, ILogger<KeycloakAdminClient> logger)
    : IIdentityProviderAdmin
{
    private string BaseUrl => (config["Keycloak:AdminBaseUrl"] ?? "http://localhost:8090").TrimEnd('/');
    private string Realm => config["Keycloak:Realm"] ?? "industry-trade";
    private string AdminUser => config["Keycloak:AdminUser"] ?? "admin";
    private string AdminPassword => config["Keycloak:AdminPassword"] ?? "admin";
    private string DefaultPassword => config["Keycloak:DefaultUserPassword"] ?? "Abc@12345";

    public async Task<string?> ResetToDefaultPasswordAsync(string userName, CancellationToken ct)
    {
        try
        {
            var http = httpFactory.CreateClient();

            var token = await GetAdminTokenAsync(http, ct);
            if (token is null) return null;
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var userId = await FindUserIdAsync(http, userName, ct);
            if (userId is null)
            {
                logger.LogWarning("Keycloak user '{UserName}' not found; password not reset.", userName);
                return null;
            }

            var reset = await http.PutAsJsonAsync(
                $"{BaseUrl}/admin/realms/{Realm}/users/{userId}/reset-password",
                new { type = "password", value = DefaultPassword, temporary = true }, ct);

            if (!reset.IsSuccessStatusCode)
            {
                logger.LogWarning("Keycloak reset-password returned {Status} for '{UserName}'.", reset.StatusCode, userName);
                return null;
            }

            return DefaultPassword;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Keycloak admin call failed while resetting '{UserName}'.", userName);
            return null;
        }
    }

    private async Task<string?> GetAdminTokenAsync(HttpClient http, CancellationToken ct)
    {
        var response = await http.PostAsync(
            $"{BaseUrl}/realms/master/protocol/openid-connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = "admin-cli",
                ["username"] = AdminUser,
                ["password"] = AdminPassword,
            }), ct);

        if (!response.IsSuccessStatusCode) return null;
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return doc.RootElement.TryGetProperty("access_token", out var t) ? t.GetString() : null;
    }

    private async Task<string?> FindUserIdAsync(HttpClient http, string userName, CancellationToken ct)
    {
        var response = await http.GetAsync(
            $"{BaseUrl}/admin/realms/{Realm}/users?exact=true&username={Uri.EscapeDataString(userName)}", ct);
        if (!response.IsSuccessStatusCode) return null;

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var first = doc.RootElement.EnumerateArray().FirstOrDefault();
        return first.ValueKind == JsonValueKind.Object && first.TryGetProperty("id", out var id)
            ? id.GetString() : null;
    }
}
