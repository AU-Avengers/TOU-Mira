using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Roles;
using TownOfUs.Buttons;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

public static class AurialEvents
{
    [RegisterEvent]
    public static void MiraButtonClickEventHandler(MiraButtonClickEvent @event)
    {
        var button = @event.Button;
        var source = PlayerControl.LocalPlayer;

        if (!source.AmOwner || button == null || !button.CanClick() || button is IAurialBypassButton)
        {
            return;
        }

        CustomRoleUtils.GetActiveRolesOfType<AurialRole>().Where(x => !x.Player.AmOwner)
            .Do(x => AurialRole.RpcSense(x.Player, source));
    }

    [RegisterEvent]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        var source = @event.Source;

        if (!(TutorialManager.InstanceExists || source.AmOwner))
        {
            return;
        }

        CustomRoleUtils.GetActiveRolesOfType<AurialRole>().Where(x => !x.Player.AmOwner)
            .Do(x => AurialRole.RpcSense(x.Player, source));
    }
}