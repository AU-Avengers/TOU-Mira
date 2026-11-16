using HarmonyLib;
using MiraAPI.Events;
using TownOfUs.AnimationSystem.Events;

namespace TownOfUs.Patches.Options;

[HarmonyPriority(Priority.Last)]
[HarmonyPatch]
public static class KillOverlayPatch
{
    private static NetworkedPlayerInfo _lastKiller;
    private static NetworkedPlayerInfo _lastVictim;
    private static bool _toAlterLater;
    [HarmonyPrefix, HarmonyPatch(typeof(KillOverlay), nameof(KillOverlay.ShowKillAnimation), typeof(NetworkedPlayerInfo), typeof(NetworkedPlayerInfo))]
    public static bool ShowKillAnimation(NetworkedPlayerInfo killer, NetworkedPlayerInfo victim)
    {
        if (!GameManager.Instance)
        {
            return true;
        }
        _lastKiller = killer;
        _lastVictim = victim;
        var beforeAnimEvent = new BeforeKillAnimationEvent(victim, killer);
        beforeAnimEvent.InvokeEvent();
        _toAlterLater = beforeAnimEvent.ShouldAlterAnimationAfter;
        return beforeAnimEvent.FinalAnimation == null;
    }
    
    [HarmonyPrefix, HarmonyPatch(typeof(OverlayKillAnimation), nameof(OverlayKillAnimation.Initialize))]
    public static bool KillAnimationPrefix(OverlayKillAnimation __instance, KillOverlayInitData initData)
    {
        if (!GameManager.Instance)
        {
            return true;
        }
        var afterAnimEvent = new AfterKillAnimationEvent(_lastVictim, _lastKiller, __instance, initData, _toAlterLater);
        afterAnimEvent.InvokeEvent();

        return !afterAnimEvent.IsCancelled;
    }
}