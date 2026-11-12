using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities;
using Reactor.Utilities;
using System.Collections;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Events.TouEvents;
using TownOfUs.Interfaces;
using TownOfUs.Modules.ControlSystem;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities.ControlSystem;
using UnityEngine;

namespace TownOfUs.Events;

public static class ControlStateEvents
{
    public static int ActiveControlKillCount;
    [RegisterEvent]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if (!@event.TriggeredByIntro)
        {
            return; // Only run when game starts.
        }

        ActiveControlKillCount = 0;

        var hunterStalk = CustomButtonSingleton<PuppeteerControlButton>.Instance;
        hunterStalk.ExtraUses = 0;
        hunterStalk.SetUses(OptionGroupSingleton<PuppeteerOptions>.Instance.ControlUses);
        if (!hunterStalk.LimitedUses)
        {
            hunterStalk.Button?.usesRemainingText.gameObject.SetActive(false);
            hunterStalk.Button?.usesRemainingSprite.gameObject.SetActive(false);
        }
        else
        {
            hunterStalk.Button?.usesRemainingText.gameObject.SetActive(true);
            hunterStalk.Button?.usesRemainingSprite.gameObject.SetActive(true);
        }
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var opt = OptionGroupSingleton<PuppeteerOptions>.Instance;
        var controlButton = CustomButtonSingleton<PuppeteerControlButton>.Instance;
        if (@event.Source.AmOwner)
        {
            ++ActiveControlKillCount;
            if (@event.Source.Data.Role is not PuppeteerRole)
            {
                return;
            }

            if (!controlButton.EffectActive)
            {
                controlButton.ResetCooldownAndOrEffect();
            }

            if (controlButton.LimitedUses &&
                opt.ControlPerKills != 0 && opt.ControlPerKills <= ActiveControlKillCount)
            {
                ++controlButton.UsesLeft;
                ++controlButton.ExtraUses;
                controlButton.SetUses(controlButton.UsesLeft);
                ActiveControlKillCount = 0;
            }
        }
    }

    [RegisterEvent]
    public static void AfterFramedMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;
        var victim = @event.Target;
        var framed = @event.Framed;
        if (framed == null || source.GetRemoteControl<IController>() is not { } controller || controller.Controlled != framed)
        {
            return;
        }

        Coroutines.Start(CoSnapFramed(framed, victim, controller.State));
    }

    private static IEnumerator CoSnapFramed(PlayerControl controlled, PlayerControl victim, ControlState controlState)
    {
        var anim = controlled.KillAnimations.RandomSnapshot();
        controlState.FreezeControl = true;
        controlState.ClearMovementState(controlled.PlayerId);
        yield return controlled.MyPhysics.Animations.CoPlayCustomAnimation(anim.BlurAnim);
        controlled.MyPhysics.Animations.PlayIdleAnimation();
        controlled.NetTransform.SnapTo(victim.transform.position);
        yield return new WaitForSeconds(0.1f);
        controlState.FreezeControl = false;
    }

    [RegisterEvent]
    public static void RoundStartEventHandler(RoundStartEvent _)
    {
        ControlState.ClearAllInstances();

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            (player.Data.Role as ITownOfUsLocalControlRole)?.ClearControlLocal();

            player.TryRemoveModifier<TimedModifier>(x => x is IControlledModifier);
        }
    }

    [RegisterEvent]
    public static void ClientGameEndEventHandler(ClientGameEndEvent _)
    {
        ControlState.ClearAllInstances();

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            (player.Data.Role as ITownOfUsLocalControlRole)?.ClearControlLocal();

            player.TryRemoveModifier<TimedModifier>(x => x is IControlledModifier);
        }
    }
}