using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class WraithOptions : AbstractOptionGroup<WraithRole>
{
    public override string GroupName => TouLocale.Get("TouRoleWraith", "Wraith");

    public ModdedNumberOption ScareUses { get; } =
        new($"Scare Uses", 10, 3, 15, 1f, "#", "∞", MiraNumberSuffixes.None, "0")
        {
            ChangedEvent = x => LinkOptions((int)x, OptionGroupSingleton<WraithOptions>.Instance.ScareUses, OptionGroupSingleton<WraithOptions>.Instance.ScaresRequiredToClick)
        };

    public ModdedNumberOption ScareCooldown { get; } =
        new($"Scare Cooldown", 25f, 10f, 120f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedNumberOption ScaresForKill { get; } =
        new($"Scares Needed To Kill Player", 3f, 1f, 3f, 1f, "#", "∞", MiraNumberSuffixes.None, "0");

    public ModdedNumberOption ScaresRequiredToClick { get; } =
        new($"Scares Required To Be Clicked", 5, 3, 15, 1f, "#", "∞", MiraNumberSuffixes.None, "0")
        {
            ChangedEvent = x => LinkOptions((int)x, OptionGroupSingleton<WraithOptions>.Instance.ScaresRequiredToClick, OptionGroupSingleton<WraithOptions>.Instance.ScareUses)
        };

    public ModdedNumberOption ClicksNeededToStopWraith { get; } =
        new($"Clicks Needed To Stop Wraith", 3, 1, 3, 1f, "#", "∞", MiraNumberSuffixes.None, "0");

    public ModdedNumberOption SpeedMultiplier { get; set; } =
        new("Speed Multiplier For Scared Players", 1.1f, 1.05f, 1.5f, 0.05f, MiraNumberSuffixes.Multiplier, "0.00");

    private static void LinkOptions(int newAmount, ModdedNumberOption curOpt, ModdedNumberOption refOpt)
    {
        var minAmount = (int)refOpt.Value;
        if (newAmount > minAmount)
        {
            curOpt.SetValue(minAmount);
        }
    }
}