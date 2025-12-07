using HarmonyLib;
using UnityEngine;

namespace TownOfUs.Patches.Misc
{
    [HarmonyPatch(typeof(FreeChatInputField))]
    public static class FreeChatPatches
    {
        //  Update character count text on Awake
        [HarmonyPostfix]
        [HarmonyPatch(nameof(FreeChatInputField.Awake))]
        public static void AwakePostfix(FreeChatInputField __instance)
        {
            if (__instance.charCountText != null && __instance.textArea != null)
            {
                int length = __instance.textArea.text.Length;
                int limit = __instance.textArea.characterLimit;
                __instance.charCountText.text = $"{length}/{limit}";
            }
        }

        //  Char count color + limit update
        [HarmonyPostfix]
        [HarmonyPatch(nameof(FreeChatInputField.UpdateCharCount))]
        public static void UpdateCharCountPostfix(FreeChatInputField __instance)
        {
            int length = __instance.textArea.text.Length;
            int limit = __instance.textArea.characterLimit;

            __instance.charCountText.text = $"{length}/{limit}";

            int yellowThreshold = AmongUsClient.Instance.AmHost ? 750 : 225;
            int redThreshold = AmongUsClient.Instance.AmHost ? 950 : 250;

            if (length < yellowThreshold)
            {
                __instance.charCountText.color = Color.black;
                return;
            }

            if (length < redThreshold)
            {
                __instance.charCountText.color = new Color(1f, 1f, 0f, 1f); // yellow
                return;
            }

            __instance.charCountText.color = Color.red;
        }
    }
}
