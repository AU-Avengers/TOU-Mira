using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch]
public static class GhostRoleUsePatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(DeconControl), nameof(DeconControl.Use))]
    public static bool DeconControlUsePatch(DeconControl __instance)
    {
        // DeconControl.CanUse was inlined, so we "uninline" it here
        __instance.CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out _);
        if (!canUse)
        {
            return false;
        }

        __instance.cooldown = 6f;
        if (Constants.ShouldPlaySfx())
        {
            SoundManager.Instance.PlaySound(__instance.UseSound, false);
        }

        __instance.OnUse.Invoke();

        return false;
    }

    [HarmonyPatch(typeof(MovingPlatformBehaviour))]
    [HarmonyPatch(nameof(MovingPlatformBehaviour.Use), typeof(PlayerControl))]
    [HarmonyPrefix]
    public static bool MovingPlatformBehaviourUsePrefixPatch(MovingPlatformBehaviour __instance, PlayerControl player)
    {
        var vector = __instance.transform.position - player.transform.position;
        if (player.Data.IsGhostDead() || player.Data.Disconnected)
        {
            return false;
        }

        if (__instance.Target || vector.magnitude > 3f)
        {
            return false;
        }

        __instance.IsDirty = true;
        __instance.StartCoroutine(__instance.UsePlatform(player));
        return false;
    }

    [HarmonyPatch(typeof(ZiplineBehaviour), nameof(ZiplineBehaviour.Use), typeof(PlayerControl), typeof(bool))]
    [HarmonyPrefix]
    public static bool ZiplineBehaviourUsePrefixPatch(ZiplineBehaviour __instance, PlayerControl player, bool fromTop)
    {
        if (player.Data.IsGhostDead() || player.Data.Disconnected)
        {
            return false;
        }

        Transform transform;
        Transform transform2;
        Transform transform3;
        if (fromTop)
        {
            transform = __instance.handleTop;
            transform2 = __instance.handleBottom;
            transform3 = __instance.landingPositionBottom;
        }
        else
        {
            transform = __instance.handleBottom;
            transform2 = __instance.handleTop;
            transform3 = __instance.landingPositionTop;
        }

        __instance.StopAllCoroutinesForPlayer(player);
        __instance.playerIdUseZiplineCoroutines[player.PlayerId] =
            __instance.StartCoroutine(__instance.CoUseZipline(player, transform, transform2, transform3, fromTop));
        return false;
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckUseZipline))]
    [HarmonyPrefix]
    public static bool CheckUseZiplinePrefixPatch(PlayerControl __instance, PlayerControl target,
        ZiplineBehaviour ziplineBehaviour, bool fromTop)
    {
        __instance.logger.Debug($"Checking if {__instance.PlayerId} can use zipline");
        if (AmongUsClient.Instance.IsGameOver || !AmongUsClient.Instance.AmHost)
        {
            return false;
        }

        if (!target)
        {
            __instance.logger.Warning("Invalid zipline use, player is null");
            return false;
        }

        var data = target.Data;
        if (data == null || data.IsGhostDead() || target.inMovingPlat)
        {
            __instance.logger.Warning($"Invalid zipline use from {target.PlayerId}");
            return false;
        }

        if (MeetingHud.Instance)
        {
            __instance.logger.Warning("Tried to zipline while a meeting was starting");
            return false;
        }

        var vector = ziplineBehaviour.GetHandlePos(fromTop) - target.transform.position;
        if (vector.magnitude > 3f)
        {
            __instance.logger.Info($"{target} was denied the zipline: distance={vector.magnitude}");
            return false;
        }

        __instance.RpcUseZipline(target, ziplineBehaviour, fromTop);

        return false;
    }

    [HarmonyPatch(typeof(OpenDoorConsole), nameof(OpenDoorConsole.Use))]
    [HarmonyPrefix]
    public static bool OpenDoorConsoleUsePrefixPatch(OpenDoorConsole __instance)
    {
        __instance.CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out _);

        if (!canUse)
        {
            return false;
        }

        __instance.myDoor.SetDoorway(true);

        return false;
    }

    [HarmonyPatch(typeof(DoorConsole), nameof(DoorConsole.Use))]
    [HarmonyPrefix]
    public static bool DoorConsoleUsePrefixPatch(DoorConsole __instance)
    {
        __instance.CanUse(PlayerControl.LocalPlayer.Data, out var canUse, out _);

        if (!canUse)
        {
            return false;
        }

        PlayerControl.LocalPlayer.NetTransform.Halt();
        var minigame = Object.Instantiate(__instance.MinigamePrefab, Camera.main.transform);
        minigame.transform.localPosition = new Vector3(0f, 0f, -50f);

        try
        {
            minigame.Cast<IDoorMinigame>().SetDoor(__instance.MyDoor);
        }
        catch (InvalidCastException)
        {
            /* ignored */
        }

        minigame.Begin(null);

        return false;
    }
}