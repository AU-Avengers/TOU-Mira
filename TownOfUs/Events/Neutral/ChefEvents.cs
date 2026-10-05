using System.Collections;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Events.Neutral;

public static class ChefEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (!CustomRoleUtils.GetActiveRolesOfType<ChefRole>().HasAny())
        {
            return;
        }

        if (!OptionGroupSingleton<ChefOptions>.Instance.ChefArrows)
        {
            return;
        }

        Coroutines.Start(CoCreateChefArrow(@event.Target));
    }

    public static IEnumerator CoCreateChefArrow(PlayerControl target)
    {
        yield return new WaitForSeconds(OptionGroupSingleton<ChefOptions>.Instance.ChefArrowDelay.Value);

        var deadBody = UnityEngine.Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x => x.ParentId == target.PlayerId);

        if (deadBody == null)
        {
            yield break;
        }

        foreach (var chef in CustomRoleUtils.GetActiveRolesOfType<ChefRole>().Select(x => x.Player))
        {
            if (chef.AmOwner)
            {
                chef.AddModifier<ChefArrowModifier>(deadBody, TownOfUsColors.Chef);
            }
        }
    }
    
    [RegisterEvent]
    public static void EjectionEventHandler(EjectionEvent _)
    {
        var chef = CustomRoleUtils.GetActiveRolesOfType<ChefRole>().FirstOrDefault();
        if (chef != null && chef.TargetsServed && !chef.Player.HasDied())
        {
            if (chef.Player.AmOwner)
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    MiraLocaleManager.Get("TownOfUsMira.Role.ChefVictoryMessageSelf").Replace("<role>", $"{TownOfUsColors.Chef.ToTextColor()}{chef.GetRoleName()}</color>"),
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Chef.LoadAsset());

                notif1.AdjustNotification();
            }
            else
            {
                string message;
                LoadableAsset<Sprite> icon;

                if (OptionGroupSingleton<ChefOptions>.Instance.ChefAnonymizeWin)
                {
                    message = MiraLocaleManager.Get("TouNeutAnonymousVictoryMessage");
                    icon = TouRoleIcons.Neutral;
                }
                else
                {
                    message = MiraLocaleManager.Get("TownOfUsMira.Role.ChefVictoryMessage")
                        .Replace("<role>", $"{TownOfUsColors.Chef.ToTextColor()}{chef.GetRoleName()}</color>");
                    icon = TouRoleIcons.Chef;
                }

                var notif1 = Helpers.CreateAndShowNotification(
                    message.Replace("<player>", chef.Player.Data.PlayerName),
                    Color.white, new Vector3(0f, 1f, -20f), spr: icon.LoadAsset());

                notif1.AdjustNotification();
            }
            var stats = GameHistory.PlayerStats[chef.Player.PlayerId];
            stats.DeathString = MiraLocaleManager.Get("DiedToWinning");
            stats.RoundOfDeath = HudManagerHelper.Instance.CurrentRound;
            stats.DiedThisRound = false;
            stats.PlayerState = StoredPlayerState.Dead;
            stats.LockDeathInfo = true;

            chef.Player.Exiled();
        }
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            return;
        }
        var servings = ModifierUtils.GetActiveModifiers<ChefServedModifier>().Where(x => !x.HasFinished).ToList();
        foreach (var serving in servings)
        {
            serving.StartTimer();
        }

        var chef = CustomRoleUtils.GetActiveRolesOfType<ChefRole>().FirstOrDefault();
        if (chef != null && chef.TargetsServed && !chef.Player.HasDied())
        {
            if (chef.Player.AmOwner)
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    MiraLocaleManager.Get("TownOfUsMira.Role.ChefVictoryMessageSelf"),
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Chef.LoadAsset());

                notif1.AdjustNotification();
            }
            else
            {
                string message;
                LoadableAsset<Sprite> icon;
                
                if (OptionGroupSingleton<ChefOptions>.Instance.ChefAnonymizeWin)
                {
                    message = MiraLocaleManager.Get("TouNeutAnonymousVictoryMessage");
                    icon = TouRoleIcons.Neutral;
                }
                else
                {
                    message = MiraLocaleManager.Get("TownOfUsMira.Role.ChefVictoryMessage");
                    icon = TouRoleIcons.Chef;
                }

                var notif1 = Helpers.CreateAndShowNotification(
                    message.Replace("<player>", chef.Player.Data.PlayerName),
                    Color.white, new Vector3(0f, 1f, -20f), spr: icon.LoadAsset());

                notif1.AdjustNotification();
            }
            var stats = GameHistory.PlayerStats[chef.Player.PlayerId];
            stats.DeathString = MiraLocaleManager.Get("DiedToWinning");
            stats.RoundOfDeath = HudManagerHelper.Instance.CurrentRound;
            stats.DiedThisRound = false;
            stats.PlayerState = StoredPlayerState.Dead;
            stats.LockDeathInfo = true;

            chef.Player.Exiled();
        }
    }
}