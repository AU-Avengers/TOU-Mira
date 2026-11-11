using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Usables;
using MiraAPI.GameOptions;
using TownOfUs.AnimationSystem;
using TownOfUs.AnimationSystem.Events;
using TownOfUs.Modules;
using TownOfUs.Networking;
using TownOfUs.Options;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Events.Neutral;

public static class WerewolfEvents
{
    [RegisterEvent]
    public static void BeforeKillAnim(BeforeKillAnimationEvent @event)
    {
        if (!OptionGroupSingleton<GameMechanicOptions>.Instance.ShowInfoSensitiveKillAnimations.Value || @event.Killer?.Object?.GetSignificantRole() is not WerewolfRole)
        {
            return;
        }
        @event.FinalAnimation = KillAnimationHelpers.GetAndDisplayGenericKillAnim(HudManager.Instance.KillOverlay,
            @event.Killer, @event.Victim, ExtendedKillAnimType.Werewolf_Slash);
    }

    [RegisterEvent]
    public static void PlayerCanUseEventHandler(PlayerCanUseEvent @event)
    {
        if (!@event.IsVent)
        {
            return;
        }

        if (!PlayerControl.LocalPlayer)
        {
            return;
        }

        if (@event.Usable.TryCast<Vent>() == null)
        {
            return;
        }

        if (!PlayerControl.LocalPlayer.inVent && PlayerControl.LocalPlayer.Data.Role is WerewolfRole { Rampaging: false })
        {
            @event.Cancel();
        }
    }
}