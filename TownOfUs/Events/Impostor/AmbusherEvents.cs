using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using TownOfUs.AnimationSystem.Events;
using TownOfUs.AnimationSystem.SpecialKills;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Events.Impostor;

public static class AmbusherEvents
{
    [RegisterEvent]
    public static void BeforeKillAnim(BeforeKillAnimationEvent @event)
    {
        if (OptionGroupSingleton<GameMechanicOptions>.Instance.ShowInfoSensitiveKillAnimations.Value && @event.Killer?.Object?.HasModifier<AmbusherConcealedModifier>() == true)
        {
            @event.FinalAnimation = AmbushKillAnimation.CreateKillAnimation(@event.Killer, @event.Victim);
            @event.ShouldAlterAnimationAfter = false;
        }
    }

    [RegisterEvent]
    public static void EjectionEventEventHandler(EjectionEvent _)
    {
        CustomRoleUtils.GetActiveRolesOfType<AmbusherRole>().Do(x => x.Clear());
    }

    [RegisterEvent]
    public static void PlayerDeathEventHandler(PlayerDeathEvent _)
    {
        CustomRoleUtils.GetActiveRolesOfType<AmbusherRole>().Do(x => x.CheckDeadPursued());
    }
}