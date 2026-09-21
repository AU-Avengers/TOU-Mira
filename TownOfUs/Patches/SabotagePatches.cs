using HarmonyLib;
using MiraAPI.GameOptions;
using TownOfUs.Modules.Components;
using TownOfUs.Options;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class SabotagePatches
{
    public static bool CanLocalPlayerSabotage()
    {
        var localPlayer = PlayerControl.LocalPlayer;
        if (!localPlayer || !localPlayer.Data || LobbyBehaviour.Instance)
        {
            return true;
        }

        if (MiscUtils.CurrentGamemode() is not TouGamemode.Normal)
        {
            return true;
        }

        var options = OptionGroupSingleton<GameMechanicOptions>.Instance;

        if (localPlayer.HasDied() && !options.CanSabotageWhenDead.Value)
        {
            return false;
        }

        var minimum = (int)options.PlayerCountWhenSabotagesDisable.Value;
        if (minimum > 0)
        {
            var aliveCount = PlayerControl.AllPlayerControls.ToArray().Count(x => !x.HasDied());
            if (aliveCount <= minimum)
            {
                return false;
            }
        }

        return true;
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.ToggleMapVisible))]
    [HarmonyPrefix]
    public static void ToggleMapVisiblePatch(HudManager __instance, MapOptions options)
    {
        if (options.Mode is MapOptions.Modes.Sabotage && !CanLocalPlayerSabotage())
        {
            options.Mode = MapOptions.Modes.Normal;
        }
    }

    [HarmonyPatch(typeof(NormalGameManager), nameof(NormalGameManager.GetMapOptions))]
    [HarmonyPostfix]
    public static void GetMapOptionsPatch(ref MapOptions __result)
    {
        if (__result == null || __result.Mode != MapOptions.Modes.Sabotage || CanLocalPlayerSabotage())
        {
            return;
        }

        __result = new MapOptions { Mode = MapOptions.Modes.Normal };
    }

    [HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.Refresh))]
    [HarmonyPostfix]
    public static void RefreshPostfix(SabotageButton __instance)
    {
        var player = PlayerControl.LocalPlayer;
        if (GameManager.Instance == null || player == null)
        {
            return;
        }

        if (__instance.gameObject.active && !CanLocalPlayerSabotage())
        {
            __instance.SetDisabled();
            HudManagerHelper.Instance.SabotageButtonDisabledSprite?.gameObject.SetActive(true);
        }
        else
        {
            HudManagerHelper.Instance.SabotageButtonDisabledSprite?.gameObject.SetActive(false);
        }
    }
}