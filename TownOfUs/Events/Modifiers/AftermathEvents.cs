using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Buttons;
using TownOfUs.Modifiers.Game.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Events.Modifiers;

public static class AftermathEvents
{
    [RegisterEvent]
    public static void AftermathDeathEvent(AfterMurderEvent @event)
    {
        var source = @event.Source;

        if (!@event.Target.HasModifier<AftermathModifier>() || !source.AmOwner || MeetingHud.Instance)
        {
            return;
        }

        var button = CustomButtonManager.Buttons.Where(x => x.Enabled(source.Data.Role) && x.Timer <= 0)
            .OfType<IAftermathableButton>().FirstOrDefault();
        if (button == null)
        {
            return;
        }

        var text = TouLocale.GetParsed("TouModifierAftermathTriggeredNotif").Replace("<player>", @event.Target.Data.PlayerName);

        var notif1 = Helpers.CreateAndShowNotification(
            $"<b>{text.Replace("<modifier>", $"{TownOfUsColors.Aftermath.ToTextColor()}{TouLocale.Get("TouModifierAftermath")}</color>")}</b>",
            Color.white, new Vector3(0f, 1f, -20f), spr: TouModifierIcons.Aftermath.LoadAsset());

        notif1.AdjustNotification();

        switch (button)
        {
            case IAftermathablePlayerButton playerButton:
                playerButton.Target = source;
                break;
            case IAftermathableBodyButton bodyButton:
                bodyButton.Target =
                    source.GetNearestDeadBody(2f); // By logic, the closest body *should* be the one that just appeared
                break;
        }

        button.AftermathHandler();
    }
}