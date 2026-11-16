using System.Collections;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Modules.ControlSystem;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Events.Impostor;

public static class PuppeteerEvents
{
    public static bool CanPuppeteerMove = true;
    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;
        var victim = @event.Target;
        var framed = @event.Framed;
        if (framed == null || source.Data.Role is not PuppeteerRole puppetRole || puppetRole.Controlled != framed)
        {
            return;
        }

        Coroutines.Start(CoSnapFramed(framed, victim));
    }

    private static IEnumerator CoSnapFramed(PlayerControl controlled, PlayerControl victim)
    {
        var anim = controlled.KillAnimations.RandomSnapshot();
        CanPuppeteerMove = false;
        PuppeteerControlState.ClearMovementState(controlled.PlayerId);
        yield return controlled.MyPhysics.Animations.CoPlayCustomAnimation(anim.BlurAnim);
        controlled.MyPhysics.Animations.PlayIdleAnimation();
        controlled.NetTransform.SnapTo(victim.transform.position);
        yield return new WaitForSeconds(0.1f);
        CanPuppeteerMove = true;
    }
}
