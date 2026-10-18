using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class MorphlingOptions : AbstractRoleOptionGroup<MorphlingRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Morphling", "Morphling");

    [ModdedNumberOption("TouOptionMorphlingSamplesPerGame", 0f, 15f, 5f, MiraNumberSuffixes.None, "0", true)]
    public float MaxSamples { get; set; } = 0f;

    [ModdedNumberOption("TouOptionMorphlingMorphUsesPerRound", 0f, 10f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float MaxMorphs { get; set; } = 0f;

    [ModdedNumberOption("TouOptionMorphlingMorphCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float MorphlingCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionMorphlingMorphDuration", 5f, 45f, 1f, MiraNumberSuffixes.Seconds)]
    public float MorphlingDuration { get; set; } = 10f;

    public ModdedEnumOption CanVent { get; set; } = new("TouOptionMorphlingCanVent", (int)MorphlingVent.Always, typeof(MorphlingVent),
        ["TouOptionMorphlingCanVentEnumNever", "TouOptionMorphlingCanVentEnumUnlessMorphed", "TouOptionMorphlingCanVentEnumAlways"]);
}

public enum MorphlingVent
{
    Never,
    Unmimic,
    Always,
}