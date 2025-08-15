using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modifiers.Game;
using TownOfUs.Roles.Neutral;
using MiraAPI.Hud;
using TownOfUs.Buttons.Neutral;

namespace TownOfUs.Events.Crewmate;

public static class PredatorEvents
{
    [RegisterEvent]
    public static void MiraButtonClickEventHandler(MiraButtonClickEvent @event)
    {
        var button = @event.Button;
        var source = PlayerControl.LocalPlayer;

        if (button == null || !button.CanClick())
        {
            return;
        }

        CheckForPredatorStaring(source);
    }

    [RegisterEvent]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        var source = @event.Source;

        CheckForPredatorStaring(source);
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;
        var target = @event.Target;

        CheckForPredatorStaring(source);

        if (source.Data.Role is not PredatorRole)
        {
            return;
        }

        if (source.Data.Role is PredatorRole role && role.CaughtPlayers.Contains(target!))
        {
            CustomButtonSingleton<PredatorKillButton>.Instance.SetTimer(0f);
        }

        if (source.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.GetsPunished)
        {
            return;
        }
    }

    private static void CheckForPredatorStaring(PlayerControl source)
    {
        if (MeetingHud.Instance || ExileController.Instance)
        {
            return;
        }

        if (!source.HasModifier<PredatorStaringModifier>())
        {
            return;
        }

        var mod = source.GetModifier<PredatorStaringModifier>();

        if (mod?.Predator == null || !source.AmOwner)
        {
            return;
        }

        PredatorRole.RpcCatchPlayer(mod.Predator, source);
    }
}