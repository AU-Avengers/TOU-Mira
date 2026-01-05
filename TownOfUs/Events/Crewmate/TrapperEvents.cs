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

public static class TrapperEvents
{
    [RegisterEvent]
    public static void CompleteTaskEvent(CompleteTaskEvent @event)
    {
        if (@event.Player.AmOwner && @event.Player.Data.Role is TrapperRole &&
            OptionGroupSingleton<TrapperOptions>.Instance.TaskUses &&
            !OptionGroupSingleton<TrapperOptions>.Instance.TrapsRemoveOnNewRound)
        {
            var button = CustomButtonSingleton<TrapperTrapButton>.Instance;
            ++button.UsesLeft;
            ++button.ExtraUses;
            button.SetUses(button.UsesLeft);
        }
    }

    [RegisterEvent]
    public static void StartMeetingEventHandler(StartMeetingEvent @event)
    {
        CustomRoleUtils.GetActiveRolesOfType<TrapperRole>().Do(x => x.Report());
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (OptionGroupSingleton<TrapperOptions>.Instance.TrapsRemoveOnNewRound)
        {
            CustomRoleUtils.GetActiveRolesOfType<TrapperRole>().Do(x => x.Clear());

            if (PlayerControl.LocalPlayer.Data.Role is TrapperRole)
            {
                var uses = OptionGroupSingleton<TrapperOptions>.Instance.MaxTraps;
                CustomButtonSingleton<TrapperTrapButton>.Instance.SetUses((int)uses);
            }
        }
    }
}