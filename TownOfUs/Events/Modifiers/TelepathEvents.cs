using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Modifiers.Game.Assailant;
using TownOfUs.Modifiers.Game.Impostor;
using TownOfUs.Modules;
using TownOfUs.Options.Modifiers.Impostor;
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
            if (victim.IsImpostorAligned() && source == victim && options.KnowFailedGuess && MeetingHud.Instance &&
                victim.TryGetModifier<AssassinModifier>(out var assassin) && assassin.LastAttemptedVictim)
            {
                Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.ImpSoft, alpha: 0.4f));
                var text = MiraLocaleManager.Get("TownOfUsMira.Modifier.TelepathFailedGuess").Replace("<player>", victim.Data.PlayerName).Replace("<target>", assassin.LastAttemptedVictim!.Data.PlayerName).Replace("<guess>", assassin.LastGuessedItem);
                var notif1 = Helpers.CreateAndShowNotification($"<b>{text}</b>", Color.white, new Vector3(0f, 1f, -20f),
                spr: TouModifierIcons.Telepath.LoadAsset());
                notif1.AdjustNotification();
            }
            else if (source.IsImpostorAligned() && source != victim && options.KnowCorrectGuess && MeetingHud.Instance)
            {
                Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.ImpSoft, alpha: 0.05f));
                var text = MiraLocaleManager.Get("TownOfUsMira.Modifier.TelepathCorrectGuess").Replace("<player>", source.Data.PlayerName).Replace("<target>", victim.Data.PlayerName).Replace("<role>", victim.GetRoleWhenAlive().GetRoleName());

                var notif1 = Helpers.CreateAndShowNotification($"<b>{TownOfUsColors.ImpSoft.ToTextColor()}{text}</color></b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouModifierIcons.Telepath.LoadAsset());
                notif1.AdjustNotification();
            }
            else if (source.IsImpostorAligned() && source != victim)
            {
                Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.ImpSoft, alpha: 0.05f));
                var text = MiraLocaleManager.Get("TownOfUsMira.Modifier.TelepathKill").Replace("<player>", source.Data.PlayerName);
                var notif1 = Helpers.CreateAndShowNotification($"<b>{TownOfUsColors.ImpSoft.ToTextColor()}{text}</color></b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouModifierIcons.Telepath.LoadAsset());
                notif1.AdjustNotification();
                if (options.KnowKillLocation)
                {
                    victim?.AddModifier<TelepathDeathNotifierModifier>(PlayerControl.LocalPlayer);
                }
            }
            else if (victim.IsImpostorAligned() && options.KnowDeath)
            {
                Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.ImpSoft, alpha: 0.4f));
                var text = MiraLocaleManager.Get("TownOfUsMira.Modifier.TelepathDeath").Replace("<player>", victim.Data.PlayerName);
                var notif1 = Helpers.CreateAndShowNotification($"<b>{TownOfUsColors.ImpSoft.ToTextColor()}{text}</color></b>",
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