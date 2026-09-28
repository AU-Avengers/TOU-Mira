using HarmonyLib;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Neutral.NeutralOutlier;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Patches.Roles;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Patches;

[HarmonyPatch(typeof(MiraAPI.Utilities.Helpers), nameof(MiraAPI.Utilities.Helpers.GetClosestPlayers),
    typeof(UnityEngine.Vector2), typeof(float), typeof(bool))]
public static class GetClosestPlayersDuelExemptPatch
{
    public static void Postfix(System.Collections.Generic.List<PlayerControl> __result)
    {
        __result?.RemoveAll((PlayerControl p) => p == null || p.HasModifier<DuelModifier>());
    }
}

[HarmonyPatch(typeof(SpyMapCountOverlayPatch), nameof(SpyMapCountOverlayPatch.UpdateBlips),
    typeof(CounterArea), typeof(List<int>), typeof(bool))]
public static class AdminDuelHidePatch
{
    public static void Prefix(List<int> colorMapping)
    {
        if (colorMapping == null || colorMapping.Count == 0)
        {
            return;
        }

        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p == null || p.Data == null || p.Data.IsDead || !p.HasModifier<DuelModifier>())
            {
                continue;
            }
            colorMapping.Remove(p.Data.DefaultOutfit.ColorId);
        }
    }
}

[HarmonyPatch(typeof(SecurityLogBehaviour), nameof(SecurityLogBehaviour.LogPlayer))]
public static class DoorLogDuelHidePatch
{
    public static bool Prefix([HarmonyArgument(0)] PlayerControl player)
    {
        return player == null || !player.HasModifier<DuelModifier>();
    }
}

[HarmonyPatch(typeof(ArrowTargetModifier), nameof(ArrowTargetModifier.FixedUpdate))]
public static class SonarDuelArrowHidePatch
{
    public static void Postfix(ArrowTargetModifier __instance)
    {
        if (__instance is not SonarArrowTargetModifier)
        {
            return;
        }

        var arrow = __instance.Arrow;
        if (arrow == null || arrow.gameObject == null)
        {
            return;
        }

        var hide = __instance.Player != null && __instance.Player.HasModifier<DuelModifier>();
        if (arrow.gameObject.activeSelf == hide)
        {
            arrow.gameObject.SetActive(!hide);
        }
    }
}

[HarmonyPatch(typeof(AurialRole), nameof(AurialRole.RpcSense))]
public static class AurialDuelSenseExemptPatch
{
    public static bool Prefix(PlayerControl player, PlayerControl source)
    {
        return source == null || !source.HasModifier<DuelModifier>();
    }
}

[HarmonyPatch(typeof(FootstepsModifier), nameof(FootstepsModifier.FixedUpdate))]
public static class FootstepsDuelHidePatch
{
    public static bool Prefix(FootstepsModifier __instance)
    {
        return __instance.Player == null || !__instance.Player.HasModifier<DuelModifier>();
    }
}

[HarmonyPatch(typeof(SystemConsole), nameof(SystemConsole.CanUse))]
public static class DuelEmergencyConsoleBlockPatch
{
    public static void Postfix(SystemConsole __instance, NetworkedPlayerInfo pc, ref bool canUse, ref bool couldUse)
    {
        if (pc?.Object == null || !pc.Object.HasModifier<DuelModifier>())
        {
            return;
        }

        if (__instance?.MinigamePrefab != null && __instance.MinigamePrefab.TryCast<EmergencyMinigame>() != null)
        {
            canUse = false;
            couldUse = false;
        }
    }
}

[HarmonyPatch(typeof(LookoutRole), nameof(LookoutRole.RpcSeePlayer))]
public static class DuelStrikeLookoutExemptPatch
{
    public static bool Prefix(PlayerControl source, PlayerControl target)
    {
        return !(source.HasModifier<DuelModifier>() && target.HasModifier<DuelModifier>());
    }
}

[HarmonyPatch(typeof(Trap), nameof(Trap.Update))]
public static class TrapDuelExemptPatch
{
    public static void Postfix(Trap __instance)
    {
        __instance._owner?.TrappedPlayers.RemoveAll(r => r?.Player != null && r.Player.HasModifier<DuelModifier>());
    }
}
