using HarmonyLib;
using Reactor.Utilities;
using TownOfUs.Modules;
using TownOfUs.Roles;
using System.Reflection;
using Il2CppInterop.Runtime.InteropTypes;
using MiraAPI.Utilities;
using UnityEngine;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class SubmergedStartPatch
{
    public static MethodBase TargetMethod()
    {
        return Helpers.GetStateMachineMoveNext<IntroCutscene>(nameof(IntroCutscene.ShowRole))!;
    }

    public static void Postfix(Il2CppObjectBase __instance)
    {
        var wrapper = new StateMachineWrapper<IntroCutscene>(__instance);
        // run before the first yield
        if (wrapper.GetState() != 1)
        {
            return;
        }

        if (ModCompatibility.IsSubmerged())
        {
            Coroutines.Start(ModCompatibility.WaitMeeting(ModCompatibility.ResetTimers));
        }
    }
}

public static class SubmergedHudPatch
{
    public static GameObject SubmergedFloorButton;
    public static void UpdateFloorButton(HudManager __instance, IGhostRole ghost)
    {
        if (ModCompatibility.IsSubmerged())
        {
            if (!SubmergedFloorButton)
            {
                SubmergedFloorButton = __instance.MapButton.transform.parent.Find(__instance.MapButton.name + "(Clone)")
                    ?.gameObject ?? null!;
            }

            if (SubmergedFloorButton)
            {
                SubmergedFloorButton.SetActive(!ghost.GhostActive);
            }
        }
    }
}

[HarmonyPatch(typeof(PlayerPhysics))]
[HarmonyPriority(Priority.Low)] // make sure it occurs after other patches
public static class SubmergedLateUpdatePhysicsPatch
{
    [HarmonyPatch(nameof(PlayerPhysics.HandleAnimation))]
    [HarmonyPatch(nameof(PlayerPhysics.LateUpdate))]
    public static void Postfix(PlayerPhysics __instance)
    {
        if (!ModCompatibility.IsSubmerged())
        {
            return;
        }

        ModCompatibility.GhostRoleFix(__instance);
    }
}