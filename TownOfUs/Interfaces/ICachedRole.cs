namespace TownOfUs.Extensions;

public interface ICachedRole
{
    // if shown first, the cached role appears like so: Bomber (Traitor). Alternatively, if it's not, it'll appear like this: Imitator (Sheriff)
    bool ShowCurrentRoleFirst { get; }
    bool Visible { get; }
    CacheRoleGuess GuessMode { get; }
    RoleBehaviour CachedRole { get; }
}

public enum CacheRoleGuess
{
    CachedRole,
    ActiveRole,
    ActiveOrCachedRole
}