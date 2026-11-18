using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Utilities;
using TownOfUs.AnimationSystem.Events;

namespace TownOfUs.Patches.Options;

[HarmonyPriority(Priority.Last)]
[HarmonyPatch]
public static class KillOverlayPatch
{
    private static NetworkedPlayerInfo _lastKiller;
    private static NetworkedPlayerInfo _lastVictim;
    private static NetworkedPlayerInfo _lastDisplayedKiller;
    private static NetworkedPlayerInfo _lastDisplayedVictim;
    private static bool _toAlterLater;

    [HarmonyPrefix,
     HarmonyPatch(typeof(KillOverlay), nameof(KillOverlay.ShowKillAnimation), typeof(NetworkedPlayerInfo),
         typeof(NetworkedPlayerInfo))]
    public static bool ShowKillAnimation(KillOverlay __instance, NetworkedPlayerInfo killer, NetworkedPlayerInfo victim)
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
        _lastDisplayedKiller = beforeAnimEvent.DisplayedKiller;
        _lastDisplayedVictim = beforeAnimEvent.DisplayedVictim;
        if (beforeAnimEvent.FinalAnimation == null)
        {
            if (_lastDisplayedKiller != killer || _lastDisplayedVictim != victim)
            {
                IEnumerable<OverlayKillAnimation> self = __instance.KillAnims;
                OverlayKillAnimation killAnimation;
                if (_lastDisplayedKiller.Object)
                {
                    OverlayKillAnimation[] killAnimations = _lastDisplayedKiller.Object.MyPhysics.Animations.GetKillAnimations();
                    if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == AmongUs.GameOptions.GameModes.HideNSeek &&
                        AprilFoolsMode.ShouldHorseAround())
                    {
                        self = __instance.HorseWrangleAnims;
                    }
                    else if (GameOptionsManager.Instance.CurrentGameOptions.GameMode != AmongUs.GameOptions.GameModes.HideNSeek &&
                             AprilFoolsMode.ShouldClassicMode())
                    {
                        self = __instance.ClassicKillAnims;
                        if (_lastDisplayedKiller.Role.CustomKillAnimations.Length != 0)
                        {
                            self = _lastDisplayedKiller.Role.CustomKillAnimations;
                        }
                    }
                    else if (killAnimations.Length != 0)
                    {
                        self = killAnimations;
                    }
                    else
                    {
                        SkinViewData skin =
                            ShipStatus.Instance.CosmeticsCache.GetSkin(_lastDisplayedKiller.Object.CurrentOutfit.SkinId);
                        if (skin && skin.CustomKillAnimID > 0)
                        {
                            if (!string.IsNullOrEmpty(skin.HatID_KillAnim) &&
                                skin.HatID_KillAnim != _lastDisplayedKiller.Object.CurrentOutfit.HatId)
                            {
                                if (_lastDisplayedKiller.Role.CustomKillAnimations.Length != 0)
                                {
                                    self = _lastDisplayedKiller.Role.CustomKillAnimations;
                                }

                                killAnimation = self.RandomSnapshot();
                                __instance.ShowKillAnimation(killAnimation, _lastDisplayedKiller, _lastDisplayedVictim);
                                return false;
                            }

                            if (!string.IsNullOrEmpty(skin.VisorID_KillAnim) &&
                                skin.VisorID_KillAnim != _lastDisplayedKiller.Object.CurrentOutfit.VisorId)
                            {
                                if (_lastDisplayedKiller.Role.CustomKillAnimations.Length != 0)
                                {
                                    self = _lastDisplayedKiller.Role.CustomKillAnimations;
                                }

                                killAnimation = self.RandomSnapshot();
                                __instance.ShowKillAnimation(killAnimation, _lastDisplayedKiller, _lastDisplayedVictim);
                                return false;
                            }

                            self = new []
                            {
                                __instance.CustomKillAnimations[skin.CustomKillAnimID - 1]
                            };
                        }
                        else if (_lastDisplayedKiller.Role.CustomKillAnimations.Length != 0)
                        {
                            self = _lastDisplayedKiller.Role.CustomKillAnimations;
                        }
                    }
                }

                killAnimation = self.RandomSnapshot();
                __instance.ShowKillAnimation(killAnimation, _lastDisplayedKiller, _lastDisplayedVictim);
                return false;
            }

            return true;
        }

        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(OverlayKillAnimation), nameof(OverlayKillAnimation.Initialize))]
    public static bool KillAnimationPrefix(OverlayKillAnimation __instance, KillOverlayInitData initData)
    {
        if (!GameManager.Instance)
        {
            return true;
        }
        var afterAnimEvent = new AfterKillAnimationEvent(_lastVictim, _lastKiller, _lastDisplayedVictim, _lastDisplayedKiller, __instance, initData, _toAlterLater);
        afterAnimEvent.InvokeEvent();

        return !afterAnimEvent.IsCancelled;
    }
}