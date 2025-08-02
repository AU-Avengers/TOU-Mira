using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Events.Crewmate;

public static class CatalystEvents
{
    [RegisterEvent]
    public static void EndMeetingEventHandler(EndMeetingEvent @event)
    {
        ModifierUtils.GetPlayersWithModifier<CatalystOverchargedModifier>()
            .Do(p => p.RemoveModifier<CatalystOverchargedModifier>());

        if (PlayerControl.LocalPlayer.Data.Role is CatalystRole)
        {
            CustomButtonSingleton<CatalystOverchargeButton>.Instance.SetUses((int)OptionGroupSingleton<CatalystOptions>.Instance.OverchargeUses);
        }
    }
}