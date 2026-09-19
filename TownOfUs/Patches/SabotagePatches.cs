using HarmonyLib;
using MiraAPI.GameOptions;
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

        var options = OptionGroupSingleton<GeneralOptions>.Instance;

        if (localPlayer.HasDied())
        {
            return options.CanSabotageWhenDead.Value;
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

    [HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.Refresh))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static bool SabotageButtonRefreshPatch(SabotageButton __instance)
    {
        if (CanLocalPlayerSabotage())
        {
            return true;
        }

        __instance.ToggleVisible(false);
        __instance.SetDisabled();
        return false;
    }

    [HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static bool SabotageButtonClickPatch()
    {
        return CanLocalPlayerSabotage();
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

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    [HarmonyPostfix]
    public static void HudManagerUpdatePatch(HudManager __instance)
    {
        if (!__instance.SabotageButton)
        {
            return;
        }

        var sabotageButton = __instance.SabotageButton;
        if (!sabotageButton.isActiveAndEnabled)
        {
            return;
        }

        if (CanLocalPlayerSabotage())
        {
            sabotageButton.SetEnabled();
            return;
        }

        sabotageButton.SetDisabled();
    }
}