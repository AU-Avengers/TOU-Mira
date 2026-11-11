using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using TownOfUs.AnimationSystem;
using TownOfUs.AnimationSystem.Events;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Networking;
using TownOfUs.Options;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

public static class OfficerEvents
{
    [RegisterEvent]
    public static void BeforeKillAnim(BeforeKillAnimationEvent @event)
    {
        if (!OptionGroupSingleton<GameMechanicOptions>.Instance.ShowInfoSensitiveKillAnimations.Value || @event.Killer?.Object?.GetSignificantRole() is not OfficerRole)
        {
            return;
        }
        @event.FinalAnimation = KillAnimationHelpers.GetAndDisplayGenericKillAnim(HudManager.Instance.KillOverlay,
            @event.Killer, @event.Victim, ExtendedKillAnimType.Shoot);
    }

    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (!@event.TriggeredByIntro)
        {
            return;
        }

        var shootButton = CustomButtonSingleton<OfficerShootButton>.Instance;
        shootButton.TotalBullets = -1;
        shootButton.RoundsBeforeReset = 0;
        shootButton.LoadedBullets = 0;
    }
}
