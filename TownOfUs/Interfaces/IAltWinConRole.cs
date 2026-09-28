using UnityEngine;

namespace TownOfUs.Interfaces;

public interface IAltWinConRole
{
    bool ReachedWinCondition { get; }

    AltWinResult WinOutcome { get; }

    AltWinResult EffectiveWinOutcome { get; }

    bool AboutToTorment { get; set; }

    bool HasKilled { get; set; }

    Color RoleColor { get; }

    LoadableAsset<Sprite> WinIcon { get; }
}
public enum AltWinResult
{
    Leaves,
    KillsOnePlayer,
    EndsGame,
}