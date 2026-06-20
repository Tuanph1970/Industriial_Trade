using System.Text.Json;
using IndustryTrade.Modules.IdentityAccess.Application.Users;
using Microsoft.Extensions.Caching.Distributed;

namespace IndustryTrade.Modules.IdentityAccess.Infrastructure.Persistence;

/// <summary>
/// Caches resolved authorization (permissions + data-scope) in the distributed cache so the claims
/// transformation does not hit the database on every authenticated request. A short TTL bounds how
/// long a role/unit change takes to propagate. Backed by Redis when configured, in-memory otherwise.
/// </summary>
internal sealed class CachingUserAuthorizationProvider(
    UserAuthorizationProvider inner, IDistributedCache cache) : IUserAuthorizationProvider
{
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(60);
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task<UserAuthorization> GetByUserNameAsync(string userName, CancellationToken ct)
    {
        var key = $"authz:{userName}";

        var cached = await cache.GetStringAsync(key, ct);
        if (cached is not null)
            return JsonSerializer.Deserialize<UserAuthorization>(cached, Json)!;

        var auth = await inner.GetByUserNameAsync(userName, ct);

        // Cache hits only; a miss may be a user created moments later — don't pin "not found".
        if (auth.Found)
        {
            await cache.SetStringAsync(key, JsonSerializer.Serialize(auth, Json),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = Ttl }, ct);
        }

        return auth;
    }
}
