using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.HnsCrewmate;

namespace TownOfUs.Options.Roles.HnsCrewmate;

public sealed class HnsMysticOptions : AbstractOptionGroup<HnsMysticRole>
{
    public override string GroupName => TouLocale.Get("HnsRoleMystic", "Mystic");

    [ModdedNumberOption("Dead Body Arrow Duration", 0.1f, 5f, 0.1f, MiraNumberSuffixes.Seconds, "0.00")]
    public float MysticArrowDuration { get; set; } = 1.5f;
}