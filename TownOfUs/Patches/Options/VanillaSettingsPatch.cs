using AmongUs.GameOptions;
using HarmonyLib;

namespace TownOfUs.Patches.Options;

[HarmonyPatch]
public static class VanillaSettingsPatch
{
    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.CreateSettings))]
    [HarmonyPostfix]
    public static void Postfix(GameOptionsMenu __instance)
    {
        if (__instance.gameObject.name == "GAME SETTINGS TAB")
        {
            try
            {
                var impostorCount = __instance.Children.ToArray()
                    ?.FirstOrDefault(x => x.TryCast<NumberOption>()?.intOptionName == Int32OptionNames.NumImpostors)
                    ?.Cast<NumberOption>();
                if (impostorCount != null)
                {
                    impostorCount.ValidRange = new FloatRange(0f, 5f);
                }

                var impostorMaxCount = __instance.Children.ToArray()
                    ?.FirstOrDefault(x => x.TryCast<NumberOption>()?.intOptionName == Int32OptionNames.MaxImpostors)
                    ?.Cast<NumberOption>();
                if (impostorMaxCount != null)
                {
                    impostorMaxCount.ValidRange = new FloatRange(0f, 5f);
                }

                var commonTasks = __instance.Children.ToArray()
                    ?.FirstOrDefault(x => x.TryCast<NumberOption>()?.intOptionName == Int32OptionNames.NumCommonTasks)
                    ?.Cast<NumberOption>();
                if (commonTasks != null)
                {
                    commonTasks.ValidRange = new FloatRange(0f, 4f);
                }

                var shortTasks = __instance.Children.ToArray()
                    ?.FirstOrDefault(x => x.TryCast<NumberOption>()?.intOptionName == Int32OptionNames.NumShortTasks)
                    ?.Cast<NumberOption>();
                if (shortTasks != null)
                {
                    shortTasks.ValidRange = new FloatRange(0f, 8f);
                }

                var longTasks = __instance.Children.ToArray()
                    ?.FirstOrDefault(x => x.TryCast<NumberOption>()?.intOptionName == Int32OptionNames.NumLongTasks)
                    ?.Cast<NumberOption>();
                if (longTasks != null)
                {
                    longTasks.ValidRange = new FloatRange(0f, 4f);
                }
            }
            catch
            {
                // ignored
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
    public static bool Prefix(ShipStatus __instance)
    {
        var commonTask = Math.Min(__instance.CommonTasks.Count, 4);
        var normalTask = Math.Min(__instance.ShortTasks.Count, 8);
        var longTask = Math.Min(__instance.LongTasks.Count, 4);
        if (GameOptionsManager.Instance.currentNormalGameOptions.NumCommonTasks > commonTask) GameOptionsManager.Instance.currentNormalGameOptions.NumCommonTasks = commonTask;
        if (GameOptionsManager.Instance.currentNormalGameOptions.NumShortTasks > normalTask) GameOptionsManager.Instance.currentNormalGameOptions.NumShortTasks = normalTask;
        if (GameOptionsManager.Instance.currentNormalGameOptions.NumLongTasks > longTask) GameOptionsManager.Instance.currentNormalGameOptions.NumLongTasks = longTask;
        return true;
    }
}