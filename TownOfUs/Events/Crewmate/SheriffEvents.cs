using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using TownOfUs.AnimationSystem;
using TownOfUs.AnimationSystem.BasicKills;
using TownOfUs.AnimationSystem.Events;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules;
using TownOfUs.Networking;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

public static class SheriffEvents
{
    [RegisterEvent]
    public static void BeforeKillAnim(BeforeKillAnimationEvent @event)
    {
        if (!OptionGroupSingleton<GameMechanicOptions>.Instance.ShowInfoSensitiveKillAnimations.Value || @event.Killer?.Object?.GetSignificantRole() is not SheriffRole sheriffRole)
        {
            return;
        }

        if (sheriffRole.HasMisfired && sheriffRole.Player.AmOwner)
        {
            @event.FinalAnimation = SuffocateKillAnimation.CreateKillAnimation(@event.Killer, @event.Victim);
            @event.ShouldAlterAnimationAfter = false;
        }
        else
        {
            @event.FinalAnimation = KillAnimationHelpers.GetAndDisplayGenericKillAnim(HudManager.Instance.KillOverlay,
                @event.Killer, @event.Victim, ExtendedKillAnimType.Shoot);
        }
    }

    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            CustomButtonSingleton<SheriffShootButton>.Instance.FailedShot = false;
        }
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;

        if (source.Data.Role is not SheriffRole)
        {
            return;
        }

        if (source.HasModifier<AllianceGameModifier>(allyMod2 => !allyMod2.GetsPunished))
        {
            return;
        }

        var target = @event.Target;
        var options = OptionGroupSingleton<SheriffOptions>.Instance;

        if (GameHistory.PlayerStats.TryGetValue(source.PlayerId, out var stats))
        {
            if (target.IsImpostor() ||
                (target.IsCrewmate() && target.HasModifier<AllianceGameModifier>(allyMod => !allyMod.GetsPunished)) ||
                (target.Is(RoleAlignment.NeutralOutlier) && options.ShootNeutralOutlier) ||
                (target.Is(RoleAlignment.NeutralEvil) && options.ShootNeutralEvil) ||
                (target.Is(RoleAlignment.NeutralKilling) && options.ShootNeutralKiller) ||
                (target.Is(RoleAlignment.NeutralBenign) && options.ShootNeutralBenign))
            {
                stats.CorrectKills += 1;
            }
            else
            {
                stats.IncorrectKills += 1;
            }
        }
    }
}