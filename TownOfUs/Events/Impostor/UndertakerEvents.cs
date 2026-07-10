using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Events.Impostor;

public static class UndertakerEvents
{
    [RegisterEvent]
    public static void OnMeetingEventHandler(StartMeetingEvent _)
    {
        foreach (var undertaker in CustomRoleUtils.GetActiveRolesOfType<UndertakerRole>()
                     .Select(undertaker => undertaker.Player).ToList())
        {
            if (undertaker.HasModifier<DragModifier>())
            {
                undertaker.GetModifierComponent().RemoveModifier<DragModifier>();
            }

            if (undertaker.AmOwner)
            {
                CustomButtonSingleton<UndertakerDragDropButton>.Instance.SetDrag();
            }
        }
    }
}