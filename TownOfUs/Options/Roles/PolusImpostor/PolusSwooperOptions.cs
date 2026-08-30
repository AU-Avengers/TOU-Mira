using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.TownOfPolus.Impostor;

namespace TownOfUs.Options.Roles.PolusImpostor;

public sealed class PolusSwooperOptions : AbstractRoleOptionGroup<PolusSwooperRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfPolusRoleSwooper", "Swooper");

    [ModdedNumberOption("Swoop Cooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float SwoopCooldown { get; set; } = 10f;

    [ModdedNumberOption("Swoop Duration", 5f, 15f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float SwoopDuration { get; set; } = 10f;
}