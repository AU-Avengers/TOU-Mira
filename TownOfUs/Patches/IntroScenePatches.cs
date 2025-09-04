
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfUs.Options;
using TownOfUs.Utilities;
using Object = Il2CppSystem.Object;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class IntroScenePatches
{
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    public static void ShowTeamPatchPostfix(IntroCutscene __instance)
    {
        if (PlayerControl.LocalPlayer.IsImpostor())
        {
            if (OptionGroupSingleton<GeneralOptions>.Instance.FFAImpostorMode)
            {
                __instance.TeamTitle.text =
                    DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Impostor,
                        Array.Empty<Object>());
                __instance.TeamTitle.color = Palette.ImpostorRed;

                var player = __instance.CreatePlayer(0, 1, PlayerControl.LocalPlayer.Data, true);
                __instance.ourCrewmate = player;
            }
        }
        else
        {
            SetHiddenImpostors(__instance);
        }
    }

    public static void SetHiddenImpostors(IntroCutscene __instance)
    {
        var amount = Helpers.GetAlivePlayers().Count(x => x.IsImpostor());
        __instance.ImpostorText.text =
            DestroyableSingleton<TranslationController>.Instance.GetString(
                amount == 1 ? StringNames.NumImpostorsS : StringNames.NumImpostorsP, amount);
        __instance.ImpostorText.text = __instance.ImpostorText.text.Replace("[FF1919FF]", "<color=#FF1919FF>");
        __instance.ImpostorText.text = __instance.ImpostorText.text.Replace("[]", "</color>");

        if (!OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled) return;

        var players = GameData.Instance.PlayerCount;

        if (players < 7)
        {
            return;
        }

        var list = OptionGroupSingleton<RoleOptions>.Instance;

        int maxSlots = players < 15 ? players : 15;

        List<RoleListOption> buckets = [];
        if (list.RoleListEnabled)
        {
            for (int i = 0; i < maxSlots; i++)
            {
                int slotValue = i switch
                {
                    0 => list.Slot1,
                    1 => list.Slot2,
                    2 => list.Slot3,
                    3 => list.Slot4,
                    4 => list.Slot5,
                    5 => list.Slot6,
                    6 => list.Slot7,
                    7 => list.Slot8,
                    8 => list.Slot9,
                    9 => list.Slot10,
                    10 => list.Slot11,
                    11 => list.Slot12,
                    12 => list.Slot13,
                    13 => list.Slot14,
                    14 => list.Slot15,
                    _ => -1
                };

                buckets.Add((RoleListOption)slotValue);
            }
        }

        if (!buckets.Any(x => x is RoleListOption.Any)) return;


        __instance.ImpostorText.text =
            DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NumImpostorsP, 256);
        __instance.ImpostorText.text = __instance.ImpostorText.text.Replace("[FF1919FF]", "<color=#FF1919FF>");
        __instance.ImpostorText.text = __instance.ImpostorText.text.Replace("[]", "</color>");
        __instance.ImpostorText.text = __instance.ImpostorText.text.Replace("256", "???");
    }
}
