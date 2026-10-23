using HarmonyLib;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using System.Diagnostics.CodeAnalysis;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Patches.ControlSystem;

[HarmonyPatch]
public static class ControllerOverlayPatch
{
    [HarmonyPatch(typeof(OpenDoorConsole), nameof(OpenDoorConsole.SetOutline))]
    [HarmonyPatch(typeof(PlatformConsole), nameof(PlatformConsole.SetOutline))]
    [HarmonyPatch(typeof(ZiplineConsole), nameof(ZiplineConsole.SetOutline))]
    [HarmonyPatch(typeof(DeconControl), nameof(DeconControl.SetOutline))]
    [HarmonyPatch(typeof(DoorConsole), nameof(DoorConsole.SetOutline))]
    [HarmonyPatch(typeof(Ladder), nameof(Ladder.SetOutline))]
    [HarmonyPatch(typeof(Vent), nameof(Vent.SetOutline))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    public static void SetOutlinePostfix(MonoBehaviour __instance, bool on, bool mainTarget)
    {
        if (__instance.TryCast<IUsable>() == null)
        {
            return;
        }

        if (PlayerControl.LocalPlayer.Data.Role is not PuppeteerRole { Controlled: not null } puppeteer ||
            !PuppeteerRole.ControlState.IsControlled(puppeteer.Controlled.PlayerId, out _) ||
            PlayerControl.LocalPlayer.Data.Role is not ParasiteRole { Controlled: not null } parasite ||
            !ParasiteRole.ControlState.IsControlled(parasite.Controlled.PlayerId, out _))
        {
            return;
        }

        if (!GetImageToOutline(__instance, out SpriteRenderer? image, out Color color, out float onVal))
        {
            return;
        }

        image.material.SetFloat(ShaderID.Outline, on ? onVal : 0);
        image.material.SetColor(ShaderID.OutlineColor, color);
        image.material.SetColor(ShaderID.AddColor, mainTarget ? color : Color.clear);
    }

    private static bool GetImageToOutline(MonoBehaviour __instance, [NotNullWhen(true)] out SpriteRenderer? image, out Color color, out float onVal)
    {
        onVal = 1f;
        color = Color.white;
        if (__instance.TryCast<Ladder>() is { } ladder)
        {
            image = ladder.Image;
        }
        else if (__instance.TryCast<ZiplineConsole>() is { } zipline)
        {
            image = zipline.image;
        }
        else if (__instance.TryCast<DeconControl>() is { } decon && decon.Image)
        {
            image = decon.Image;
        }
        else if (__instance.TryCast<DoorConsole>() is { } doorConsole && doorConsole.Image)
        {
            image = doorConsole.Image;
        }
        else if (__instance.TryCast<OpenDoorConsole>() is { } openDoorConsole && openDoorConsole.image)
        {
            image = openDoorConsole.image;
        }
        else if (__instance.TryCast<PlatformConsole>() is { } platform && platform.Image)
        {
            image = platform.Image;
        }
        else if (__instance.TryCast<Vent>() is { } vent)
        {
            image = vent.myRend;
            color = (PlayerControl.LocalPlayer.Data.Role as ICustomRole)?.RoleColor ?? TownOfUsColors.Impostor;
        }
        else
        {
            image = null;
            return false;
        }

        return true;
    }
}
