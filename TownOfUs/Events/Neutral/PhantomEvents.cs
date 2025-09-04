using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Patches;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Events.Neutral;

public static class PhantomEvents
{
    [RegisterEvent]
    public static void CompleteTaskEventHandler(CompleteTaskEvent @event)
    {
        if (@event.Player.Data.Role is not PhantomTouRole phantom)
        {
            return;
        }

        phantom.CheckTaskRequirements();

        if (phantom.CompletedAllTasks &&
            OptionGroupSingleton<PhantomOptions>.Instance.PhantomWin is not PhantomWinOptions.EndsGame)
        {
            phantom.Clicked();
            if (phantom.Player.AmOwner)
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>You have successfully won as the {TownOfUsColors.Phantom.ToTextColor()}Phantom</color>, as you finished your tasks postmortem!</b>",
                    Color.white, spr: TouRoleIcons.Phantom.LoadAsset());

                notif1.Text.SetOutlineThickness(0.35f);
                notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
                HudManagerPatches.ZoomButton.SetActive(true);
                if (OptionGroupSingleton<PhantomOptions>.Instance.PhantomWin is PhantomWinOptions.Spooks)
                {
                    DeathHandlerModifier.RpcUpdateDeathHandler(PlayerControl.LocalPlayer, "null", -1, DeathHandlerOverride.SetTrue, lockInfo: DeathHandlerOverride.SetTrue);
                    var notif2 = Helpers.CreateAndShowNotification(
                        $"<b>You have one round to spook a player of your choice to death, choose wisely.</b>",
                        Color.white);

                    notif2.Text.SetOutlineThickness(0.35f);
                    notif2.transform.localPosition = new Vector3(0f, 0.85f, -20f);
                }
            }
            else
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>The {TownOfUsColors.Phantom.ToTextColor()}Phantom</color>, {phantom.Player.Data.PlayerName}, has successfully won, as they completed their tasks postmortem!</b>",
                    Color.white, spr: TouRoleIcons.Phantom.LoadAsset());

                notif1.Text.SetOutlineThickness(0.35f);
                notif1.transform.localPosition = new Vector3(0f, 1f, -20f);
            }
        }
    }
}