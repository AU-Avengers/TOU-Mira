using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Events.Modifiers;

public static class EgotistEvents
{
    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            return;
        }

        var ego = ModifierUtils.GetActiveModifiers<EgotistModifier>().FirstOrDefault(x => !x.Player.HasDied());
        if (ego != null && Helpers.GetAlivePlayers().Where(x =>
                    x.IsCrewmate() && !(x.TryGetModifier<AllianceGameModifier>(out var ally) && !ally.GetsPunished))
                .ToList().Count == 0)
        {
            if (ego.Player.AmOwner)
            {
                PlayerControl.LocalPlayer.RpcPlayerExile();
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>You have successfully won as the {TownOfUsColors.Egotist.ToTextColor()}Egotist</color>, as no more crewmates remain!</b>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouModifierIcons.Egotist.LoadAsset());

                notif1.AdjustNotification();
            }
            else
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>The {TownOfUsColors.Egotist.ToTextColor()}Egotist</color>, {ego.Player.Data.PlayerName}, has successfully won, as no more crewmates remain!</b>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouModifierIcons.Egotist.LoadAsset());

                notif1.AdjustNotification();
            }
        }
    }
}