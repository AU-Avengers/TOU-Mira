using System.Collections;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Networking;
using TownOfUs.Options.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Events.Impostor;

public static class BootleggerEvents
{
    [RegisterEvent]
    public static void StartMeetingEventHandler(StartMeetingEvent @event)
    {
        var setting = (PoisonTrigger)OptionGroupSingleton<BootleggerOptions>.Instance.PoisonRoleblockTrigger.Value;
        if (!PlayerControl.LocalPlayer.IsHost() || setting is PoisonTrigger.OnMeetingEnd)
        {
            return;
        }

        Coroutines.Start(CoWaitForPois());
    }

    private static IEnumerator CoWaitForPois()
    {
        yield return new WaitForSeconds(8f);
        
        foreach (var poison in ModifierUtils.GetActiveModifiers<BootleggerPoisonModifier>())
        {
            if (poison.Player.HasDied())
            {
                continue;
            }
            if (poison.Poison == PoisonProgress.Poison)
            {
                Error($"{poison.Player.CachedPlayerData.PlayerName} is dying to poison after meeting began. ({poison.TimeRemaining} seconds on the timer)");
                poison.Bootlegger.RpcMeetingMurder(poison.Player, MeetingAnimation.PlayerNameplateAnimation, CustomTouMurderRpcs.GetRandomMeetingAnim(DeathAnimType.Nameplate),
                    didSucceed: !poison.Player.HasModifier<InvulnerabilityModifier>(), causeOfDeath: "Poison");
            }
        }
    }
    [RegisterEvent]
    public static void OnProcessVotesEventHandler(ProcessVotesEvent @event)
    {
        var setting = (PoisonTrigger)OptionGroupSingleton<BootleggerOptions>.Instance.PoisonRoleblockTrigger.Value;
        foreach (var poison in ModifierUtils.GetActiveModifiers<BootleggerPoisonModifier>())
        {
            if (poison.Player.HasDied())
            {
                continue;
            }
            if (poison.Poison == PoisonProgress.Poison && setting is PoisonTrigger.OnMeetingEnd)
            {
                Error($"{poison.Player.CachedPlayerData.PlayerName} is dying to poison after meting finished.");
                poison.Bootlegger.RpcMeetingMurder(poison.Player, MeetingAnimation.PlayerNameplateAnimation, CustomTouMurderRpcs.GetRandomMeetingAnim(DeathAnimType.Nameplate),
                    didSucceed: !poison.Player.HasModifier<InvulnerabilityModifier>(), causeOfDeath: "Poison");
            }
        }
    }

    [RegisterEvent(1000)]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        var victim = @event.Target;
        if ((@event.IsCancelled || victim.HasModifier<InvulnerabilityModifier>()) && victim.TryGetModifier<BootleggerPoisonModifier>(out var mod))
        {
            mod.Poison = PoisonProgress.Immune;
        }
    }
}