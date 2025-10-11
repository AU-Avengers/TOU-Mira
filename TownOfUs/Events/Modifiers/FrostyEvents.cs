using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Crewmate;
using TownOfUs.Options.Modifiers.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Events.Modifiers;

public static class FrostyEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (!@event.Target.HasModifier<FrostyModifier>() || @event.Target == @event.Source ||
            MeetingHud.Instance)
        {
            return;
        }

        if (@event.Source.AmOwner)
        {
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.Frosty.ToTextColor()}{@event.Target.Data.PlayerName} was Frosty, causing you to be slower for {Math.Round(OptionGroupSingleton<FrostyOptions>.Instance.ChillDuration, 2)} seconds.</color></b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouModifierIcons.Frosty.LoadAsset());

            notif1.AdjustNotification();
        }

        @event.Source.AddModifier<FrozenModifier>();
    }
}