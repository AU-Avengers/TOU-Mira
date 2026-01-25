using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class BarkeeperOptions : AbstractOptionGroup<BarkeeperRole>
{
    public override string GroupName => "Barkeeper";

    [ModdedNumberOption("Roleblock Cooldown", 5f, 30f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float RoleblockCooldown { get; set; } = 20f;
    [ModdedNumberOption("Roleblock Duration", 5f, 30f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float RoleblockDuration { get; set; } = 15f;

    [ModdedToggleOption("Grant Hangover")]
    public bool Hangover { get; set; } = true;
    public ModdedNumberOption HangoverDuration { get; } =
        new("Hangover Duration", 5f, 30f, 1f, 20f, MiraNumberSuffixes.None)
        {
            Visible = () => OptionGroupSingleton<BarkeeperOptions>.Instance.Hangover
        };

    [ModdedNumberOption("Roleblock Delay", 1f, 10f, 1f, MiraNumberSuffixes.Seconds)]
    public float RoleblockDelay { get; set; } = 3f;
    [ModdedToggleOption("Invert Controls Of Roleblocked")]
    public bool InvertControlsOfRoleblocked { get; set; } = true;

}