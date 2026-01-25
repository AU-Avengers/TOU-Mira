using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class PuppeteerOptions : AbstractOptionGroup<PuppeteerRole>
{
    public override string GroupName => TouLocale.Get("TouRolePuppeteer", "Puppeteer");
    public override Color GroupColor => Palette.ImpostorRoleRed;

    public ModdedNumberOption ControlUses { get; } =
        new($"TouOptionPuppeteerControlUses", 3f, -1f, 30f, 1f, "#", "∞", MiraNumberSuffixes.None, "0");

    public ModdedNumberOption ControlPerKills { get; } = new("TouOptionPuppeteerControlPerKill", 2f, 0f, 15f, 1f, "Off", "#", MiraNumberSuffixes.None, "0")
    {
        Visible = () => OptionGroupSingleton<PuppeteerOptions>.Instance.ControlUses != -1
    };

    public ModdedNumberOption VictimSeesControlDirection { get; } =
        new($"TouOptionPuppeteerVictimSeesControlDirection", 3f, 0f, 30f, 1f, "Off", "#", MiraNumberSuffixes.Seconds, "0", halfIncrements: true);

    public ModdedNumberOption ControlCooldown { get; } =
        new($"TouOptionPuppeteerControlCooldown", 25f, 10f, 120f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption ControlDuration { get; } =
        new($"TouOptionPuppeteerControlDuration", 10f, 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedToggleOption CanVent { get; } =
        new($"TouOptionPuppeteerCanVent", true);
}