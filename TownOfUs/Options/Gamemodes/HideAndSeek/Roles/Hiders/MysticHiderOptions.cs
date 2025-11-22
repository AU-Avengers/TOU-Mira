using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Gamemodes.CrewmateHiders;

namespace TownOfUs.Options.Gamemodes.HideAndSeek.Roles.Hiders;

public sealed class MysticHiderOptions : AbstractOptionGroup<MysticHiderRole>
{
    public override string GroupName => TouLocale.Get("TouRoleMystic", "Mystic");

    [ModdedNumberOption("Dead Body Arrow Duration", 0.1f, 5f, 0.1f, MiraNumberSuffixes.Seconds, "0.00")]
    public float MysticArrowDuration { get; set; } = 1.5f;
}