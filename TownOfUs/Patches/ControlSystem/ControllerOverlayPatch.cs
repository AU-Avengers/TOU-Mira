using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using TownOfUs.Interfaces;
using TownOfUs.Modules.ControlSystem;
using TownOfUs.Utilities.ControlSystem;
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

        if (PlayerControl.LocalPlayer.GetRemoteControl<IRemoteControl>() is not { } control ||
            !control.State.IsUsingState(control.Player.PlayerId, out _, out _))
        {
            return;
        }

        if (!GetImageToOutline(__instance, control.State, out SpriteRenderer? image, out Color color, out float onVal))
        {
            return;
        }

        image.material.SetFloat(ShaderID.Outline, on ? onVal : 0);
        image.material.SetColor(ShaderID.OutlineColor, color);
        image.material.SetColor(ShaderID.AddColor, mainTarget ? color : Color.clear);
    }

    private static bool GetImageToOutline(MonoBehaviour __instance, ControlState controlState, [NotNullWhen(true)] out SpriteRenderer? image, out Color color, out float onVal)
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

        if (!controlState.HasControl(PlayerControl.LocalPlayer.PlayerId))
        {
            color = Palette.DisabledGrey;
            onVal = 0.3f;
        }

        return true;
    }

    [HarmonyPatch]
    public static class ClimbLadderPatch
    {
        public static MethodBase TargetMethod()
        {
            return Helpers.GetStateMachineMoveNext<PlayerPhysics>(nameof(PlayerPhysics.CoClimbLadder))!;
        }

        public static void Postfix(Il2CppObjectBase __instance, bool __result)
        {
            var wrapper = new StateMachineWrapper<PlayerPhysics>(__instance);

            if (__result)
            {
                return;
            }

            var instance = wrapper.Instance;
            var player = instance.myPlayer;
            var source = wrapper.GetParameter<Ladder>("source");

            if (player.TryGetModifierOfType<IControlledModifier>(out var mod, x => x.Controller.AmOwner) &&
                mod.State.IsControlled(player.PlayerId, out _))
            {
                source.SetDestinationCooldown();
            }
        }
    }

    [HarmonyPatch]
    public static class UseZiplinePatch
    {
        public static MethodBase TargetMethod()
        {
            return Helpers.GetStateMachineMoveNext<ZiplineBehaviour>(nameof(ZiplineBehaviour.CoUseZipline))!;
        }

        public static void Postfix(Il2CppObjectBase __instance, bool __result)
        {
            var wrapper = new StateMachineWrapper<ZiplineBehaviour>(__instance);

            if (__result)
            {
                return;
            }

            var instance = wrapper.Instance;
            var player = wrapper.GetParameter<PlayerControl>("player");

            if (player.TryGetModifierOfType<IControlledModifier>(out var mod, x => x.Controller.AmOwner) &&
                mod.State.IsControlled(player.PlayerId, out _) && instance.lastUsedConsole)
            {
                instance.lastUsedConsole.SetDestinationCooldown();
            }
        }
    }
}
