using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace TownOfUs.Options;

public sealed class RoleblockOptions : AbstractOptionGroup
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Options.Groups.RoleblockMechanics");
    public override uint GroupPriority => 1;

    public ModdedToggleOption RoleblockAffectsConsoles { get; set; } =
        new("TouOptionRoleblockAffectsConsoles", false);

    public ModdedNumberOption RoleblockDuration { get; } =
        new("TouOptionRoleblockDuration", 15f, 5f, 30f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedToggleOption InvertControlsOfRoleblocked { get; set; } =
        new("TouOptionInvertControlsOfRoleblocked", true);

    public ModdedToggleOption Hangover { get; set; } =
        new("TouOptionRoleblockHangover", true);

    public ModdedNumberOption HangoverDuration { get; } =
        new("TouOptionRoleblockHangoverDuration", 30f, 15f, 120f, 20f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => OptionGroupSingleton<RoleblockOptions>.Instance.Hangover.Value
        };
}