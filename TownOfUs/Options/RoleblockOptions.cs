using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace TownOfUs.Options;

public sealed class RoleblockOptions : AbstractOptionGroup
{
    public override string GroupName => "Roleblock Mechanics";
    public override uint GroupPriority => 1;

    public ModdedToggleOption RoleblockAffectsConsoles { get; set; } = new("Roleblock Affects Non-Role Actions", false);

    public ModdedNumberOption RoleblockDuration { get; } =
        new("Roleblock Duration", 15f, 5f, 30f, 2.5f, MiraNumberSuffixes.Seconds);

    public ModdedToggleOption InvertControlsOfRoleblocked { get; set; } = new("Invert Controls Of Roleblocked", true);

    public ModdedToggleOption Hangover { get; set; } = new("Grant Hangover", true);

    public ModdedNumberOption HangoverDuration { get; } =
        new("Hangover Duration", 30f, 15f, 120f, 20f, MiraNumberSuffixes.Seconds)
        {
            Visible = () => OptionGroupSingleton<RoleblockOptions>.Instance.Hangover.Value
        };
}