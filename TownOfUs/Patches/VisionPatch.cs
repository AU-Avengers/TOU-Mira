using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Crewmate;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules;
using TownOfUs.Options;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Patches;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
public static class VisionPatch
{
    public static bool NerfMe { get; set; }

    public static void Postfix(ShipStatus __instance, NetworkedPlayerInfo player, ref float __result)
    {
        if (player == null || player.IsDead)
        {
            __result = __instance.MaxLightRadius;
            return;
        }

        var visionFactor = 1f;

        if (player.Object.HasModifier<EclipsalBlindModifier>())
        {
            var mod = player.Object.GetModifier<EclipsalBlindModifier>()!;

            visionFactor = mod.VisionPerc;
        }


        if (player.Role.IsImpostor || (player._object.Data.Role is ITownOfUsRole touRole && touRole.HasImpostorVision))
        {
            __result = __instance.MaxLightRadius *
                       GameOptionsManager.Instance.currentNormalGameOptions.ImpostorLightMod * visionFactor;
        }
        else
        {
            if (ModCompatibility.IsSubmerged())
            {
                if (player._object.HasModifier<TorchModifier>())
                {
                    __result = Mathf.Lerp(__instance.MinLightRadius, __instance.MaxLightRadius, 1) *
                               GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod * visionFactor;
                }
                else
                {
                    __result *= visionFactor;
                }
            }
            else
            {
                SwitchSystem? switchSystem = null;

                if (__instance.Systems != null &&
                    __instance.Systems.TryGetValue(SystemTypes.Electrical, out var system))
                {
                    switchSystem = system.TryCast<SwitchSystem>();
                }

                var t = switchSystem?.Level ?? 1;


                if (player._object.HasModifier<TorchModifier>() && !player._object.HasModifier<EclipsalBlindModifier>())
                {
                    t = 1;
                }

                __result = Mathf.Lerp(__instance.MinLightRadius, __instance.MaxLightRadius, t) *
                           GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod * visionFactor;
                var mapId = (MapNames)GameOptionsManager.Instance.currentNormalGameOptions.MapId;
                if (TutorialManager.InstanceExists)
                {
                    mapId = (MapNames)AmongUsClient.Instance.TutorialMapId;
                }

                if (!player.Role.IsImpostor && OptionGroupSingleton<TownOfUsMapOptions>.Instance.SmallMapHalfVision &&
                    mapId is MapNames.MiraHQ or MapNames.Skeld or MapNames.Dleks)
                {
                    __result /= 2;
                }

                if (player._object.HasModifier<ScoutModifier>())
                {
                    __result = t == 1 ? __result * 2f : __result / 2;
                }
            }
        }

        if (NerfMe && !PlayerControl.LocalPlayer.HasDied())
        {
            __result /= 2;
        }
    }
}