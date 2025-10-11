using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Modifiers.Game.Impostor;
using TownOfUs.Modules;
using TownOfUs.Options.Modifiers.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Events.Modifiers;

public static class TelepathEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;
        var victim = @event.Target;

        if (PlayerControl.LocalPlayer.HasModifier<TelepathModifier>() && !source.AmOwner && !victim.AmOwner)
        {
            var options = OptionGroupSingleton<TelepathOptions>.Instance;
            if (victim.IsImpostor() && source == victim && options.KnowFailedGuess && MeetingHud.Instance &&
                victim.TryGetModifier<ImpostorAssassinModifier>(out var assassin) && assassin.LastAttemptedVictim)
            {
                Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.ImpSoft, alpha: 0.4f));
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.ImpSoft.ToTextColor()}Your teammate, {victim.Data.PlayerName}, attempted to shoot {assassin.LastAttemptedVictim!.Data.PlayerName} as {assassin.LastGuessedItem}, but failed!</b></color>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouModifierIcons.Telepath.LoadAsset());
                notif1.AdjustNotification();
            }
            else if (source.IsImpostor() && source != victim && options.KnowCorrectGuess && MeetingHud.Instance)
            {
                Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.ImpSoft, alpha: 0.05f));
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.ImpSoft.ToTextColor()}Your teammate, {source.Data.PlayerName}, shot {victim.Data.PlayerName} as {victim.GetRoleWhenAlive().TeamColor.ToTextColor()}{victim.GetRoleWhenAlive().GetRoleName()}</color>!</b></color>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouModifierIcons.Telepath.LoadAsset());
                notif1.AdjustNotification();
            }
            else if (source.IsImpostor() && source != victim)
            {
                Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.ImpSoft, alpha: 0.05f));
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.ImpSoft.ToTextColor()}Your teammate, {source.Data.PlayerName}, has killed.</b></color>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouModifierIcons.Telepath.LoadAsset());
                notif1.AdjustNotification();
                if (options.KnowKillLocation)
                {
                    victim?.AddModifier<TelepathDeathNotifierModifier>(PlayerControl.LocalPlayer);
                }
            }
            else if (victim.IsImpostor() && options.KnowDeath)
            {
                Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.ImpSoft, alpha: 0.4f));
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.ImpSoft.ToTextColor()}Your teammate, {victim.Data.PlayerName}, has been murdered!.</b></color>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouModifierIcons.Telepath.LoadAsset());
                notif1.AdjustNotification();
                if (options.KnowDeathLocation)
                {
                    victim?.AddModifier<TelepathDeathNotifierModifier>(PlayerControl.LocalPlayer);
                }
            }
        }
    }
}