using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using TownOfUs.AnimationSystem;
using TownOfUs.AnimationSystem.Events;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Networking;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;

namespace TownOfUs.Events.Neutral;

public static class ArsonistEvents
{
    [RegisterEvent]
    public static void BeforeKillAnim(BeforeKillAnimationEvent @event)
    {
        if (!OptionGroupSingleton<GameMechanicOptions>.Instance.ShowInfoSensitiveKillAnimations.Value || @event.Killer?.Object?.GetSignificantRole() is not ArsonistRole)
        {
            return;
        }
        @event.FinalAnimation = KillAnimationHelpers.GetAndDisplayGenericKillAnim(HudManager.Instance.KillOverlay,
            @event.Killer, @event.Victim, ExtendedKillAnimType.RHM);
    }
    [RegisterEvent]
    public static void MiraButtonClickEventHandler(MiraButtonClickEvent @event)
    {
        var button = @event.Button as CustomActionButton<PlayerControl>;
        var source = PlayerControl.LocalPlayer;
        var target = button?.Target;

        if (target == null || button == null || !button.CanClick() || target.Data.Role is not ArsonistRole ||
            !OptionGroupSingleton<ArsonistOptions>.Instance.DouseInteractions)
        {
            return;
        }

        if (!source.HasModifier<ArsonistDousedModifier>())
        {
            source.RpcAddModifier<ArsonistDousedModifier>(target.PlayerId);
        }
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;
        var victim = @event.Target;
        if (source.Data.Role is ArsonistRole && victim.AmOwner && !MeetingHud.Instance && !ExileController.Instance)
        {
            TouAudio.PlaySound(TouAudio.ArsoIgniteSound);
        }
        ArsonistRole.SetDouseUses();
    }

    [RegisterEvent]
    public static void OnRoundStartEventHandler(RoundStartEvent _)
    {
        ArsonistRole.SetDouseUses();
    }
}