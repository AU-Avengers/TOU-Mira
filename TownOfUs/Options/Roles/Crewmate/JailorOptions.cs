using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class JailorOptions : AbstractOptionGroup<JailorRole>
{
    public override string GroupName => TouLocale.Get(TouNames.Jailor, "Jailor");

    [ModdedNumberOption("Jail Cooldown", 1f, 30f, 1f, MiraNumberSuffixes.Seconds)]
    public float JailCooldown { get; set; } = 20f;

    [ModdedNumberOption("Max Number Of Executes", 1f, 5f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxExecutes { get; set; } = 3f;

    [ModdedToggleOption("Jail Same Person Twice In A Row")]
    public bool JailInARow { get; set; } = false;

    [ModdedToggleOption("Jailee Can Use Public Chat")]
    public bool JaileePublicChat { get; set; } = false;
}