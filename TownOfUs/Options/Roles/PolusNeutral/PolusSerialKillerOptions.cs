using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.TownOfPolus.Neutral;

namespace TownOfUs.Options.Roles.PolusNeutral;

public sealed class PolusSerialKillerOptions : AbstractRoleOptionGroup<PolusSerialKillerRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.TownOfPolus.Role.SerialKiller", "Serial Killer");

    [ModdedNumberOption("Kill Cooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float KillCooldown { get; set; } = 25f;

    [ModdedNumberOption("Cooldown Reduction For Kills In A Round", 0f, 10f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float CooldownKillStreakReduction { get; set; } = 2.5f;

    [ModdedNumberOption("Minimum Players to Kill", 1, 7f, 1)]
    public float MinPlayers { get; set; } = 5;
}