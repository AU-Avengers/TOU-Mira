using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using TownOfUs.Modifiers.Game.Impostor;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Impostor;

public sealed class DeadlyQuotaOptions : AbstractOptionGroup<DeadlyQuotaModifier>
{
    public override string GroupName => "Deadly Quota";
    public override Color GroupColor => Palette.ImpostorRoleHeaderRed;
    public override uint GroupPriority => 40;

    [ModdedNumberOption("Minimum Kill Quota", 1f, 5f, 1f)]
    public float KillQuotaMin { get; set; } = 2f;

    [ModdedNumberOption("Maximum Kill Quota", 1f, 5f, 1f)]
    public float KillQuotaMax { get; set; } = 4f;

    [ModdedToggleOption("Temporary Shield Until Quota Is Met")]
    public bool QuotaShield { get; set; } = true;

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