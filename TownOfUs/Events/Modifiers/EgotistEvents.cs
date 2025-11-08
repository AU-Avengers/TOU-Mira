using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Events.Modifiers;

public static class EgotistEvents
{
    public static int EgotistRoundTracker;
    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        var egoOpts = OptionGroupSingleton<EgotistOptions>.Instance;
        if (@event.TriggeredByIntro)
        {
            EgotistModifier.CooldownReduction = 0f;
            EgotistModifier.SpeedMultiplier = 1f;
            EgotistRoundTracker = (int)egoOpts.RoundsToApplyEffects.Value;
            return;
        }

        EgotistRoundTracker--;
        var ego = ModifierUtils.GetActiveModifiers<EgotistModifier>().FirstOrDefault();
        if (ego != null && !ego.LeaveMessageSent && !ego.Player.HasDied() && Helpers.GetAlivePlayers().Where(x =>
                    x.IsCrewmate() && !(x.TryGetModifier<AllianceGameModifier>(out var ally) && !ally.GetsPunished))
                .ToList().Count == 0)
        {
            ego.HasSurvived = true;
            ego.LeaveMessageSent = true;
            if (ego.Player.AmOwner)
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    TouLocale.GetParsed("TouModifierEgotistVictoryMessageSelf").Replace("<modifier>", $"{TownOfUsColors.Egotist.ToTextColor()}{ego.ModifierName}</color>"),
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouModifierIcons.Egotist.LoadAsset());

                notif1.AdjustNotification();
            }
            else
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    TouLocale.GetParsed("TouModifierEgotistVictoryMessage").Replace("<player>", ego.Player.Data.PlayerName).Replace("<modifier>", $"{TownOfUsColors.Egotist.ToTextColor()}{ego.ModifierName}</color>"),
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouModifierIcons.Egotist.LoadAsset());

                notif1.AdjustNotification();
            }
            ego.Player.Exiled();
        }
        else if (ego != null && ego.Player.HasDied() && !ego.LeaveMessageSent)
        {
            ego.LeaveMessageSent = true;
            var notif1 = Helpers.CreateAndShowNotification(
                TouLocale.GetParsed("TouModifierEgotistDeadMessage").Replace("<modifier>",
                    $"{TownOfUsColors.Egotist.ToTextColor()}{ego.ModifierName}</color>"),
                Color.white, new Vector3(0f, 1f, -20f), spr: TouModifierIcons.Egotist.LoadAsset());

            notif1.AdjustNotification();
        }

        if (ego == null || ego.Player.HasDied())
        {
            EgotistModifier.CooldownReduction = 0f;
            EgotistModifier.SpeedMultiplier = 1f;
        }
        else if (EgotistRoundTracker <= 0)
        {
            if (egoOpts.CooldowmOffset.Value > 0.01f && egoOpts.SpeedMultiplier.Value > 0.01f)
            {
                EgotistModifier.CooldownReduction += egoOpts.CooldowmOffset.Value;
                EgotistModifier.SpeedMultiplier += egoOpts.SpeedMultiplier.Value;

                var text = TouLocale.GetParsed("TouModifierEgotistCooldownSpeedChangeMessage").Replace("<modifier>",
                    $"{TownOfUsColors.Egotist.ToTextColor()}{ego.ModifierName}</color>");
                text = text.Replace("<newOffset>", $"{Math.Round(EgotistModifier.CooldownReduction, 2)}");

                var notif1 = Helpers.CreateAndShowNotification(
                    text.Replace("<newSpeed>", $"{Math.Round(EgotistModifier.SpeedMultiplier, 2)}"),
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouModifierIcons.Egotist.LoadAsset());

                notif1.AdjustNotification();
            }
            else if (egoOpts.CooldowmOffset.Value > 0.01f)
            {
                EgotistModifier.CooldownReduction += egoOpts.CooldowmOffset.Value;

                var text = TouLocale.GetParsed("TouModifierEgotistCooldownChangeMessage").Replace("<modifier>",
                    $"{TownOfUsColors.Egotist.ToTextColor()}{ego.ModifierName}</color>");
                text = text.Replace("<newOffset>", $"{Math.Round(EgotistModifier.CooldownReduction, 3)}");

                var notif1 = Helpers.CreateAndShowNotification(
                    text,
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouModifierIcons.Egotist.LoadAsset());

                notif1.AdjustNotification();
            }
            else if (egoOpts.SpeedMultiplier.Value > 0.01f)
            {
                EgotistModifier.SpeedMultiplier += egoOpts.SpeedMultiplier.Value;

                var text = TouLocale.GetParsed("TouModifierEgotistSpeedChangeMessage").Replace("<modifier>",
                    $"{TownOfUsColors.Egotist.ToTextColor()}{ego.ModifierName}</color>");
                text = text.Replace("<newSpeed>", $"{Math.Round(EgotistModifier.SpeedMultiplier, 3)}");

                var notif1 = Helpers.CreateAndShowNotification(
                    text,
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouModifierIcons.Egotist.LoadAsset());

                notif1.AdjustNotification();
            }
            EgotistRoundTracker = (int)egoOpts.RoundsToApplyEffects.Value;
        }
    }
}