using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class HerbalistOptions : AbstractRoleOptionGroup<HerbalistRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Herbalist", "Herbalist");

    [ModdedNumberOption("TouOptionHerbalistHerbCooldown", 10f, 90f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float HerbCooldown { get; set; } = 30f;

    public ModdedNumberOption MaxExposeUses { get; } = new("TouOptionHerbalistMaxExposeUses", 3f, 0f, 15f, 1f, "∞", "∞", MiraNumberSuffixes.None, "0");

    public ModdedNumberOption MaxConfuseUses { get; } = new("TouOptionHerbalistMaxConfuseUses", 5f, 0f, 15f, 1f, "∞", "∞", MiraNumberSuffixes.None, "0");

    public ModdedNumberOption MaxProtectUses { get; } = new("TouOptionHerbalistMaxProtectUses", 7f, 0f, 15f, 1f, "∞", "∞", MiraNumberSuffixes.None, "0");

    [ModdedNumberOption("TouOptionHerbalistConfuseDelay", 0.5f, 5f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float ConfuseDelay{ get; set; } = 3f;

    [ModdedNumberOption("TouOptionHerbalistConfuseDuration", 5f, 30f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float ConfuseDuration { get; set; } = 15f;

    /*
    [ModdedNumberOption("Glamour Duration", 5f, 30f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float GlamourDuration { get; set; } = 15f;*/

    [ModdedNumberOption("TouOptionHerbalistProtectDuration", 5f, 30f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float ProtectDuration { get; set; } = 15f;

    [ModdedToggleOption("TouOptionClericProtectedSeesBarrier")]
    public bool ShowBarrier { get; set; } = false;

    [ModdedToggleOption("TouOptionHerbalistNotifyUponAttack")]
    public bool AttackNotif { get; set; } = true;
}