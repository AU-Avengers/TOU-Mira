using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Roles;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

public static class RevealerEvents
{
    [RegisterEvent]
    public static void CompleteTaskEvent(CompleteTaskEvent @event)
    {
        if (@event.Player.AmOwner && @event.Player.Data.Role is RevealerRole &&
            OptionGroupSingleton<RevealerOptions>.Instance.TaskUses &&
            !OptionGroupSingleton<RevealerOptions>.Instance.TrapsRemoveOnNewRound)
        {
            var button = CustomButtonSingleton<RevealerTrapButton>.Instance;
            ++button.UsesLeft;
            ++button.ExtraUses;
            button.SetUses(button.UsesLeft);
        }
    }

    [RegisterEvent]
    public static void StartMeetingEventHandler(StartMeetingEvent @event)
    {
        CustomRoleUtils.GetActiveRolesOfType<RevealerRole>().Do(x => x.Report());
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (OptionGroupSingleton<RevealerOptions>.Instance.TrapsRemoveOnNewRound)
        {
            CustomRoleUtils.GetActiveRolesOfType<RevealerRole>().Do(x => x.Clear());

            if (PlayerControl.LocalPlayer.Data.Role is RevealerRole)
            {
                var uses = OptionGroupSingleton<RevealerOptions>.Instance.MaxTraps;
                CustomButtonSingleton<RevealerTrapButton>.Instance.SetUses((int)uses);
            }
        }
    }
}