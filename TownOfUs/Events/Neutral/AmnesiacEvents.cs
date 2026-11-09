using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Utilities;
using System.Collections;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using UnityEngine;
using Color = UnityEngine.Color;
using Object = UnityEngine.Object;

namespace TownOfUs.Events.Neutral;

public static class AmnesiacEvents
{
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        if (!CustomRoleUtils.GetActiveRolesOfType<AmnesiacRole>().Any())
        {
            return;
        }

        if (!OptionGroupSingleton<AmnesiacOptions>.Instance.RememberArrows)
        {
            return;
        }

        Coroutines.Start(CoCreateArrow(@event.Target));
    }

    [RegisterEvent]
    public static void JanitorCleanEventHandler(TouAbilityEvent @event)
    {
        if (@event is not { AbilityType: AbilityType.JanitorClean, Target: DeadBody { ParentId: var parentId } })
        {
            return;
        }

        // Remove all AmnesiacArrowModifiers pointing to the cleaned body
        foreach (var amne in CustomRoleUtils.GetActiveRolesOfType<AmnesiacRole>().Select(x => x.Player))
        {
            if (amne == null)
            {
                continue;
            }

            var arrowModifiers = amne.GetModifiers<AmnesiacArrowModifier>(x => x.DeadBody?.ParentId == parentId);
            foreach (var arrowMod in arrowModifiers)
            {
                arrowMod.RemoveSelf();
            }
        }
    }

    private static IEnumerator CoCreateArrow(PlayerControl target)
    {
        yield return new WaitForSeconds(OptionGroupSingleton<AmnesiacOptions>.Instance.RememberArrowDelay.Value);

        var deadBody = Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x => x.ParentId == target.PlayerId);

        if (deadBody == null)
        {
            yield break;
        }

        foreach (var amne in CustomRoleUtils.GetActiveRolesOfType<AmnesiacRole>().Select(x => x.Player))
        {
            if (amne.AmOwner)
            {
                amne.AddModifier<AmnesiacArrowModifier>(deadBody, Color.white);
            }
        }
    }
}