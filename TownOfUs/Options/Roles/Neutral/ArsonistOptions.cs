using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Options.Roles.Neutral;

public sealed class ArsonistOptions : AbstractOptionGroup<ArsonistRole>
{
    public override string GroupName => TouLocale.Get("TouRoleArsonist", "Arsonist");

    [ModdedNumberOption("TouOptionArsonistDouseCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float DouseCooldown { get; set; } = 25f;
    public ModdedNumberOption DouseUses { get; } = new("TouOptionArsonistDouseUses", 5f, 0f, 30f, 1f, "∞", "#", MiraNumberSuffixes.None, "0");

    [ModdedToggleOption("TouOptionArsonistDouseInteractions")]
    public bool DouseInteractions { get; set; } = true;

    [ModdedToggleOption("TouOptionArsonistLegacyMode")]
    public bool LegacyArsonist { get; set; } = true;

    public ModdedNumberOption IgniteRadius { get; set; } = new("TouOptionArsonistIgniteRadius", 0.25f, 0.05f, 1f, 0.05f,
        MiraNumberSuffixes.Multiplier, "0.00")
    {
        Visible = () => !OptionGroupSingleton<ArsonistOptions>.Instance.LegacyArsonist
    };

    [ModdedToggleOption("TouOptionArsonistCanVent")]
    public bool CanVent { get; set; }

    [ModdedToggleOption("TouOptionArsonistImpVision")]
    public bool ImpostorVision { get; set; } = true;
}