using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

public static class LookoutEvents
{
    [RegisterEvent]
    public static void CompleteTaskEvent(CompleteTaskEvent @event)
    {
        if (@event.Player.AmOwner && @event.Player.Data.Role is LookoutRole &&
            OptionGroupSingleton<LookoutOptions>.Instance.TaskUses &&
            !OptionGroupSingleton<LookoutOptions>.Instance.LoResetOnNewRound)
        {
            var button = CustomButtonSingleton<WatchButton>.Instance;
            ++button.UsesLeft;
            ++button.ExtraUses;
            button.SetUses(button.UsesLeft);
        }
    }

    [RegisterEvent]
    public static void MiraButtonClickEventHandler(MiraButtonClickEvent @event)
    {
        // Warning("Lookout click event!");
        var button = @event.Button as CustomActionButton<PlayerControl>;
        var source = PlayerControl.LocalPlayer;
        var target = button?.Target;

        if (target == null || button == null || !button.CanClick())
        {
            return;
        }

        CheckForLookoutWatched(source, target);
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var victim = @event.Target;
        var source = @event.Source;

        CheckForLookoutWatched(source, victim);
    }

    [RegisterEvent]
    public static void EjectionEventEventHandler(EjectionEvent @event)
    {
        if (!OptionGroupSingleton<LookoutOptions>.Instance.LoResetOnNewRound)
        {
            return;
        }

        ModifierUtils.GetPlayersWithModifier<LookoutWatchedModifier>()
            .Do(x => x.RemoveModifier<LookoutWatchedModifier>());

        var button = CustomButtonSingleton<WatchButton>.Instance;
        button.SetUses((int)OptionGroupSingleton<LookoutOptions>.Instance.MaxWatches);
    }

    public static void CheckForLookoutWatched(PlayerControl source, PlayerControl target)
    {
        if (MeetingHud.Instance || ExileController.Instance)
        {
            return;
        }

        if (!target.HasModifier<LookoutWatchedModifier>() || !source.AmOwner)
        {
            return;
        }

        LookoutRole.RpcSeePlayer(target, source);
    }
}