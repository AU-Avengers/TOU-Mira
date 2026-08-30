using MiraAPI.GameOptions.Attributes;
using TownOfUs.Modifiers.Game.Impostor;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Impostor;

public sealed class DeadlyQuotaOptions : AbstractTouModifierOptionGroup<DeadlyQuotaModifier>
{
    public override string GroupName => MiraLocaleManager.Get("TouModifierDeadlyQuota", "Deadly Quota");
    public override Color GroupColor => Palette.ImpostorRoleHeaderRed;
    public override uint GroupPriority => 40;

    [ModdedNumberOption("TouOptionDeadlyQuotaMinimumKillQuota", 1f, 5f, 1f)]
    public float KillQuotaMin { get; set; } = 2f;

    [ModdedNumberOption("TouOptionDeadlyQuotaMaximumKillQuota", 1f, 5f, 1f)]
    public float KillQuotaMax { get; set; } = 4f;

    [ModdedToggleOption("TouOptionDeadlyQuotaMeetingKillsCount")]
    public bool MeetingKillsCountTowardsQuota { get; set; } = true;

    [ModdedToggleOption("TouOptionDeadlyQuotaTemporaryShield")]
    public bool QuotaShield { get; set; } = false;

    [ModdedToggleOption("TouOptionDeadlyQuotaRemoveUponDeath")]
    public bool RemoveQuotaUponDeath { get; set; } = true;

    /// <summary>
    /// Picks the quota using Min/Max or falls back to Max if invalid
    /// </summary>
    public int GenerateKillQuota()
    {
        var min = Mathf.FloorToInt(KillQuotaMin);
        var max = Mathf.FloorToInt(KillQuotaMax);

        if (min > max)
            return max;

        if (min == max)
            return max;

        return UnityEngine.Random.Range(min, max + 1);
    }
}