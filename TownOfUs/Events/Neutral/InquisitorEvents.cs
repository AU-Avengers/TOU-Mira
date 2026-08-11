using AchievementsAPI.API;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Achievements;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Events.Neutral;

public static class InquisitorEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;
        var victim = @event.Target;

        if (source.Data.Role is InquisitorRole inquis &&
            GameHistory.PlayerStats.TryGetValue(source.PlayerId, out var stats))
        {
            if (victim.HasModifier<InquisitorHereticModifier>())
            {
                stats.CorrectKills += 1;
                if (source.AmOwner && stats.CorrectKills == (int)OptionGroupSingleton<InquisitorOptions>.Instance.AmountOfHeretics.Value)
                {
                    AchievementsTabSingleton<TouNeutRoleAchievementsTab>.Instance.SpanishInquisition.Unlock();
                }
            }
            else if (source != victim)
            {
                stats.IncorrectKills += 1;
                inquis.CanVanquish = false;
            }
        }

        if (PlayerControl.LocalPlayer.Data.Role is InquisitorRole && !victim.AmOwner)
        {
            if (victim.HasModifier<InquisitorHereticModifier>() && !source.AmOwner)
            {
                Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Inquisitor, alpha: 0.1f));
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.Inquisitor.ToTextColor()}{TouLocale.GetParsed("TouRoleInquisitorHereticPerished")}</b></color>", Color.white,
                    new Vector3(0f, 1f, -20f),
                    spr: TouRoleIcons.Inquisitor.LoadAsset());
                notif1.AdjustNotification();
            }
            else if (!victim.HasModifier<InquisitorHereticModifier>() && source.AmOwner)
            {
                Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Inquisitor, alpha: 0.4f));
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.Inquisitor.ToTextColor()}{TouLocale.GetParsed("TouRoleInquisitorWrongTarget") .Replace("<player>", victim.Data.PlayerName)}</b></color>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Inquisitor.LoadAsset());
                notif1.AdjustNotification();
            }
            else if (victim.HasModifier<InquisitorHereticModifier>() && source.AmOwner)
            {
                Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Doomsayer, alpha: 0.4f));
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.Inquisitor.ToTextColor()}{TouLocale.GetParsed("TouRoleInquisitorCorrectTarget") .Replace("<player>", victim.Data.PlayerName)}</b></color>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Inquisitor.LoadAsset());
                notif1.AdjustNotification();
            }
        }

        CustomRoleUtils.GetActiveRolesOfType<InquisitorRole>().Do(x => x.CheckTargetDeath(victim));
    }

    [RegisterEvent]
    public static void PlayerDeathEventHandler(PlayerDeathEvent @event)
    {
        if (@event.DeathReason != DeathReason.Exile)
        {
            return;
        }

        CustomRoleUtils.GetActiveRolesOfType<InquisitorRole>().Do(x => x.CheckTargetDeath(@event.Player));
    }

    [RegisterEvent]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        var exiled = @event.ExileController?.initData?.networkedPlayer?.Object;

        if (exiled == null)
        {
            return;
        }

        CustomRoleUtils.GetActiveRolesOfType<InquisitorRole>().Do(x => x.CheckTargetDeath(exiled));

        var inquis = CustomRoleUtils.GetActiveRolesOfType<InquisitorRole>().FirstOrDefault();
        if (inquis != null && inquis.TargetsDead && !inquis.Player.HasDied())
        {
            if (inquis.Player.AmOwner)
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    TouLocale.GetParsed("TouRoleInquisitorVictoryMessageSelf").Replace("<role>", $"{TownOfUsColors.Inquisitor.ToTextColor()}{inquis.RoleName}</color>"),
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Inquisitor.LoadAsset());

                notif1.AdjustNotification();
            }
            else
            {
                string message;
                LoadableAsset<Sprite> icon;

                if (OptionGroupSingleton<InquisitorOptions>.Instance.InquisAnonymizeWin)
                {
                    message = TouLocale.GetParsed("TouNeutAnonymousVictoryMessage");
                    icon = TouRoleIcons.Neutral;
                }
                else
                {
                    message = TouLocale.GetParsed("TouRoleInquisitorVictoryMessage")
                        .Replace("<role>", $"{TownOfUsColors.Inquisitor.ToTextColor()}{inquis.RoleName}</color>");
                    icon = TouRoleIcons.Inquisitor;
                }

                var notif1 = Helpers.CreateAndShowNotification(
                    message.Replace("<player>", inquis.Player.Data.PlayerName),
                    Color.white, new Vector3(0f, 1f, -20f), spr: icon.LoadAsset());

                notif1.AdjustNotification();
            }
            var stats = GameHistory.PlayerStats[inquis.Player.PlayerId];
            stats.DeathString = TouLocale.Get("DiedToWinning");
            stats.RoundOfDeath = HudManagerHelper.Instance.CurrentRound;
            stats.DiedThisRound = false;
            stats.PlayerState = StoredPlayerState.Dead;
            stats.LockDeathInfo = true;

            inquis.Player.Exiled();
        }
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            return;
        }

        var inquis = CustomRoleUtils.GetActiveRolesOfType<InquisitorRole>().FirstOrDefault();
        if (inquis != null && inquis.TargetsDead && !inquis.Player.HasDied())
        {
            if (inquis.Player.AmOwner)
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{TouLocale.GetParsed("TouRoleInquisitorWonSelf") .Replace("<role>", $"{TownOfUsColors.Inquisitor.ToTextColor()}{inquis.RoleName}</color>")}</b>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Inquisitor.LoadAsset());

                notif1.AdjustNotification();
            }
            else
            {
                string message;
                LoadableAsset<Sprite> icon;

                if (OptionGroupSingleton<InquisitorOptions>.Instance.InquisAnonymizeWin)
                {
                    message = TouLocale.GetParsed("TouNeutAnonymousVictoryMessage")
                        .Replace("<player>", inquis.Player.Data.PlayerName);
                    icon = TouRoleIcons.Neutral;
                }
                else
                {
                    message = $"<b>{TouLocale.GetParsed("TouRoleInquisitorWonOther") .Replace("<role>", $"{TownOfUsColors.Inquisitor.ToTextColor()}{inquis.RoleName}</color>") .Replace("<player>", inquis.Player.Data.PlayerName)}</b>";
                    icon = TouRoleIcons.Inquisitor;
                }

                var notif1 = Helpers.CreateAndShowNotification(
                    message, Color.white, new Vector3(0f, 1f, -20f), spr: icon.LoadAsset());

                notif1.AdjustNotification();
            }
            var stats = GameHistory.PlayerStats[inquis.Player.PlayerId];
            stats.DeathString = TouLocale.Get("DiedToWinning");
            stats.RoundOfDeath = HudManagerHelper.Instance.CurrentRound;
            stats.DiedThisRound = false;
            stats.PlayerState = StoredPlayerState.Dead;
            stats.LockDeathInfo = true;

            inquis.Player.Exiled();
        }
    }
}