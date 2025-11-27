using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.HnsGame.Crewmate;
using TownOfUs.Options.Modifiers.HnsCrewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Events.HnsModifiers;

public static class HnsFrostyEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (!@event.Target.HasModifier<HnsFrostyModifier>() || @event.Target == @event.Source ||
            MeetingHud.Instance)
        {
            return;
        }

        if (@event.Source.AmOwner)
        {
            var text = TouLocale.GetParsed("HnsModifierFrostyTriggeredNotif").Replace("<player>", @event.Target.Data.PlayerName);
            text = text.Replace("<modifier>", $"{TownOfUsColors.Frosty.ToTextColor()}{TouLocale.Get("HnsModifierFrosty")}</color>");

            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{text.Replace("<time>", Math.Round(OptionGroupSingleton<HnsFrostyOptions>.Instance.ChillDuration, 2).ToString(TownOfUsPlugin.Culture))}</b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouModifierIcons.Frosty.LoadAsset());

            notif1.AdjustNotification();
        }

        @event.Source.AddModifier<HnsFrozenModifier>();
    }
}