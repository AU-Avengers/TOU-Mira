using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Events.Modifiers;

public static class LoverEvents
{
    [RegisterEvent(400)]
    public static void PlayerDeathEventHandler(PlayerDeathEvent @event)
    {
        if (@event.Player == null)
        {
            return;
        }

        if (!@event.Player.TryGetModifier<LoverModifier>(out var loveMod)
            || !OptionGroupSingleton<LoversOptions>.Instance.BothLoversDie || loveMod.OtherLover == null
            || loveMod.OtherLover.HasDied() || loveMod.OtherLover.HasModifier<InvulnerabilityModifier>())
        {
            return;
        }

        switch (@event.DeathReason)
        {
            case DeathReason.Exile:
                DeathHandlerModifier.UpdateDeathHandlerImmediate(loveMod.OtherLover, TouLocale.Get("DiedToHeartbreak"),
                    DeathEventHandlers.CurrentRound, DeathHandlerOverride.SetFalse,
                    lockInfo: DeathHandlerOverride.SetTrue);
                loveMod.OtherLover.Exiled();
                break;
            case DeathReason.Kill:
                var showAnim = MeetingHud.Instance == null && ExileController.Instance == null;
                var murderResultFlags2 = MurderResultFlags.DecisionByHost | MurderResultFlags.Succeeded;

                DeathHandlerModifier.UpdateDeathHandlerImmediate(loveMod.OtherLover, TouLocale.Get("DiedToHeartbreak"),
                    DeathEventHandlers.CurrentRound,
                    (!MeetingHud.Instance && !ExileController.Instance)
                        ? DeathHandlerOverride.SetTrue
                        : DeathHandlerOverride.SetFalse, lockInfo: DeathHandlerOverride.SetTrue);
                loveMod.OtherLover.CustomMurder(
                    loveMod.OtherLover,
                    murderResultFlags2,
                    false,
                    showAnim,
                    false,
                    showAnim,
                    false);
                break;
        }
    }

    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        ModifierUtils.GetActiveModifiers<LoverModifier>().Do(x => x.OnRoundStart());
        if (@event.TriggeredByIntro && PlayerControl.LocalPlayer.TryGetModifier<LoverModifier>(out var lover) &&
            lover.OtherLover != null)
        {
            var notif1 = Helpers.CreateAndShowNotification(
                TouLocale.GetParsed("TouModifierLoverIntroMessage")
                    .Replace("<modifier>", $"{TownOfUsColors.Lover.ToTextColor()}{lover.ModifierName}</color>")
                    .Replace("<player>", $"{TownOfUsColors.Lover.ToTextColor()}{lover.OtherLover}</color>"),
                Color.white, new Vector3(0f, 1f, -20f), spr: TouModifierIcons.Lover.LoadAsset());

            notif1.AdjustNotification();
        }
    }

    [RegisterEvent]
    public static void ChangeRoleHandler(ChangeRoleEvent @event)
    {
        if (!PlayerControl.LocalPlayer)
        {
            return;
        }

        var player = @event.Player;

        if (player.TryGetModifier<LoverModifier>(out var lover) && !@event.NewRole.IsCrewmate())
        {
            lover.ForceDisableTasks = true;
            if (lover.OtherLover != null && lover.OtherLover.TryGetModifier<LoverModifier>(out var lover2))
            {
                lover2.ForceDisableTasks = true;
            }
        }
    }
}