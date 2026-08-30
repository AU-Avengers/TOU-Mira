using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Alliance;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Alliance;

public sealed class EgotistOptions : AbstractTouModifierOptionGroup<EgotistModifier>
{
    public override Func<bool> GroupVisible => () => RoleOptions.IsClassicRoleAssignment;
    public override string GroupName => MiraLocaleManager.Get("TouModifierEgotist", "Egotist");
    public override uint GroupPriority => 11;
    public override Color GroupColor => TownOfUsColors.Egotist;

    public ModdedToggleOption EgotistMustSurvive { get; set; } =
        new("TouOptionEgotistMustSurvive", false);

    public ModdedToggleOption EgotistSpeedsUp { get; set; } =
        new("TouOptionEgotistSpeedsUp", true);

    public ModdedNumberOption RoundsToApplyEffects { get; set; } =
        new("TouOptionEgotistRoundsToApplyEffects", 1f, 1f, 5f, 1f,
            MiraNumberSuffixes.None)
        {
            Visible = () => OptionGroupSingleton<EgotistOptions>.Instance.EgotistSpeedsUp
        };

    public ModdedNumberOption SpeedMultiplier { get; set; } =
        new("TouOptionEgotistSpeedMultiplier", 0.1f, 0f, 1.5f, 0.05f,
            MiraNumberSuffixes.Multiplier, "0.000")
        {
            Visible = () => OptionGroupSingleton<EgotistOptions>.Instance.EgotistSpeedsUp
        };

    public ModdedNumberOption CooldowmOffset { get; set; } =
    // I'm not sure if this is intentionally left as is, but, it says Cooldowm (with an M) -Fault
        new("TouOptionEgotistCooldownOffset", 1.5f, 0f, 5f, 0.1f,
            MiraNumberSuffixes.Seconds, "0.00")
        {
            Visible = () => OptionGroupSingleton<EgotistOptions>.Instance.EgotistSpeedsUp
        };
}