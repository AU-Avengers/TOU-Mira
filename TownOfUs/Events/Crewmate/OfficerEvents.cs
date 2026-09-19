using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

public static class OfficerEvents
{
    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (!@event.TriggeredByIntro)
        {
            return;
        }

        var shootButton = CustomButtonSingleton<OfficerShootButton>.Instance;
        shootButton.TotalBullets = -1;
        shootButton.RoundsBeforeReset = 0;
        shootButton.LoadedBullets = 0;
    }

    [RegisterEvent]
    public static void MiraButtonCancelledEventHandler(MiraButtonCancelledEvent @event)
    {
        if (@event.Button is not OfficerShootButton button || button.Target == null)
        {
            return;
        }

        if (PlayerControl.LocalPlayer.Data?.Role is not OfficerRole)
        {
            return;
        }

        if (!OptionGroupSingleton<OfficerOptions>.Instance.BlockedShotConsumesBullet.Value || button.LoadedBullets <= 0)
        {
            return;
        }

        button.LoadedBullets--;
        OfficerRole.RpcOfficerSyncBullets(PlayerControl.LocalPlayer, button.RoundsBeforeReset, button.TotalBullets,
            button.LoadedBullets);
    }
}
