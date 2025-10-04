using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Events.Neutral;

public static class ChefEvents
{
    [RegisterEvent]
    public static void EjectionEventHandler(EjectionEvent @event)
    {
        var chef = CustomRoleUtils.GetActiveRolesOfType<ChefRole>().FirstOrDefault();
        if (chef != null && chef.TargetsServed && !chef.Player.HasDied())
        {
            if (chef.Player.AmOwner)
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    TouLocale.GetParsed("TouRoleChefVictoryMessageSelf").Replace("<role>", $"{TownOfUsColors.Chef.ToTextColor()}{chef.RoleName}</color>"),
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Chef.LoadAsset());

                notif1.AdjustNotification();
                DeathHandlerModifier.RpcUpdateDeathHandler(PlayerControl.LocalPlayer, TouLocale.Get("DiedToWinning"),
                    DeathEventHandlers.CurrentRound, DeathHandlerOverride.SetFalse,
                    lockInfo: DeathHandlerOverride.SetTrue);
            }
            else
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    TouLocale.GetParsed("TouRoleChefVictoryMessageSelf").Replace("<player>", chef.Player.Data.PlayerName).Replace("<role>", $"{TownOfUsColors.Chef.ToTextColor()}{chef.RoleName}</color>"),
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Chef.LoadAsset());

                notif1.AdjustNotification();
            }

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

        var chef = CustomRoleUtils.GetActiveRolesOfType<ChefRole>().FirstOrDefault();
        if (chef != null && chef.TargetsServed && !chef.Player.HasDied())
        {
            if (chef.Player.AmOwner)
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    TouLocale.GetParsed("TouRoleChefVictoryMessageSelf"),
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Chef.LoadAsset());

                notif1.AdjustNotification();
                DeathHandlerModifier.RpcUpdateDeathHandler(PlayerControl.LocalPlayer, TouLocale.Get("DiedToWinning"),
                    DeathEventHandlers.CurrentRound, DeathHandlerOverride.SetFalse,
                    lockInfo: DeathHandlerOverride.SetTrue);
            }
            else
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    TouLocale.GetParsed("TouRoleChefVictoryMessageSelf").Replace("<player>", chef.Player.Data.PlayerName),
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Chef.LoadAsset());

                notif1.AdjustNotification();
            }

            chef.Player.Exiled();
        }
    }
}