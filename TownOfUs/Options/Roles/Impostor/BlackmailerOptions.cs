using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class BlackmailerOptions : AbstractOptionGroup<BlackmailerRole>
{
    public override string GroupName => TouLocale.Get("TouRoleBlackmailer", "Blackmailer");

    [ModdedNumberOption("TouOptionBlackmailerNumberOfBlackmailUsesPerGame", 0f, 15f, 5f, MiraNumberSuffixes.None, "0", true)]
    public float MaxBlackmails { get; set; } = 0f;

    [ModdedNumberOption("TouOptionBlackmailerBlackmailCooldown", 1f, 30f, suffixType: MiraNumberSuffixes.Seconds)]
    public float BlackmailCooldown { get; set; } = 20f;

    [ModdedNumberOption("TouOptionBlackmailerMaxPlayersAliveUntilVoting", 1f, 15f)]
    public float MaxAliveForVoting { get; set; } = 5f;

    [ModdedToggleOption("TouOptionBlackmailerBlackmailSamePersonTwiceInARow")]
    public bool BlackmailInARow { get; set; } = false;

    [ModdedToggleOption("TouOptionBlackmailerOnlyTargetSeesBlackmail")]
    public bool OnlyTargetSeesBlackmail { get; set; } = false;

    [ModdedToggleOption("TouOptionBlackmailerCanKillWithTeammate")]
    public bool BlackmailerKill { get; set; } = true;
}