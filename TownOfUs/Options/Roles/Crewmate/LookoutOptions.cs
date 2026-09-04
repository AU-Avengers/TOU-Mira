using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class LookoutOptions : AbstractRoleOptionGroup<LookoutRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Lookout", "Lookout");

    [ModdedNumberOption("TouOptionLookoutWatchCooldown", 1f, 30f, 1f, MiraNumberSuffixes.Seconds)]
    public float WatchCooldown { get; set; } = 20f;

    public ModdedEnumOption WatchType { get; } = new("Watched Player Feedback Reveals", (int)LookoutView.Players, typeof(LookoutView));

    [ModdedNumberOption("TouOptionLookoutMaxWatches", 1f, 15f, 1f, MiraNumberSuffixes.None, "0")]
    public float MaxWatches { get; set; } = 5;

    public ModdedToggleOption LookoutSeesIndirectAttacks { get; } = new("TouOptionLookoutSeesIndirectAttacks", false);

    [ModdedToggleOption("TouOptionLookoutLoResetOnNewRound")]
    public bool LoResetOnNewRound { get; set; } = true;

    public ModdedToggleOption TaskUses { get; } = new("TouOptionLookoutTaskUses", false)
    {
        Visible = () => !OptionGroupSingleton<LookoutOptions>.Instance.LoResetOnNewRound
    };
}

public enum LookoutView
{
    Roles,
    Players
}