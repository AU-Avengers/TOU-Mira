using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Options;
using TownOfUs.Roles.Other;
using TownOfUs.Utilities;
using UnityEngine;
using Object = Il2CppSystem.Object;

namespace TownOfUs.Patches;

[HarmonyPatch]
public static class IntroScenePatches
{
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
    public static class IntroCutsceneSpectatorPatch
    {
        public static void Prefix(ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay, IntroCutscene __instance)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (SpectatorRole.TrackedSpectators.Contains(player.Data.PlayerName))
                {
                    teamToDisplay.Remove(player);
                }
            }
            if (PlayerControl.LocalPlayer.HasModifier<CrewpostorModifier>() && !OptionGroupSingleton<GeneralOptions>.Instance.FFAImpostorMode)
            {
                var impTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();

                impTeam.Add(PlayerControl.LocalPlayer);
                foreach (var impostor in Helpers.GetAlivePlayers().Where(x => x.IsImpostor()))
                {
                    impTeam.Add(impostor);
                }

                teamToDisplay = impTeam;
            }
        }
    }
    
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPrefix]
    public static bool ImpostorBeginPatch(IntroCutscene __instance)
    {
        if (!OptionGroupSingleton<GeneralOptions>.Instance.FFAImpostorMode)
        {
            var crewpostor = ModifierUtils.GetPlayersWithModifier<CrewpostorModifier>().FirstOrDefault();
            if (crewpostor != null && !OptionGroupSingleton<GeneralOptions>.Instance.FFAImpostorMode)
            {
                __instance.CreatePlayer(Helpers.GetAlivePlayers().Count(x => x.IsImpostor()), 1, crewpostor.Data, true);
            }
            return true;
        }

        __instance.TeamTitle.text =
            DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Impostor, Array.Empty<Object>());
        __instance.TeamTitle.color = Palette.ImpostorRed;

        var player = __instance.CreatePlayer(0, 1, PlayerControl.LocalPlayer.Data, true);
        __instance.ourCrewmate = player;

        return false;
    }
    
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
    [HarmonyPostfix]
    public static void ShowTeamPatchPostfix(IntroCutscene __instance)
    {
        SetHiddenImpostors(__instance);
        if (PlayerControl.LocalPlayer.HasModifier<CrewpostorModifier>())
        {
            __instance.TeamTitle.text =
                DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Impostor, Array.Empty<Object>());
            __instance.TeamTitle.color = Palette.ImpostorRed;
            __instance.ImpostorText.gameObject.SetActive(false);
        }

        foreach (var spec in PlayerControl.AllPlayerControls)
        {
            if (!spec || !SpectatorRole.TrackedSpectators.Contains(spec.Data.PlayerName))
            {
                continue;
            }

            spec!.Visible = false;
            spec.Die(DeathReason.Exile, false);

            if (spec.AmOwner)
            {
                HudManager.Instance.SetHudActive(false);
                HudManager.Instance.ShadowQuad.gameObject.SetActive(false);
            }
        }

        SpectatorRole.InitList();
    }
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CreatePlayer))]
    [HarmonyPrefix]
    public static bool CreatePlayerPrefix(IntroCutscene __instance, int i, int maxDepth, NetworkedPlayerInfo pData, bool impostorPositioning, ref PoolablePlayer __result)
    {
        int num = (i % 2 == 0) ? -1 : 1;
        int num2 = (i + 1) / 2;
        float num3 = (i == 0) ? -8 : -1;
        PoolablePlayer poolablePlayer = UnityEngine.Object.Instantiate<PoolablePlayer>(__instance.PlayerPrefab, __instance.transform);
        poolablePlayer.name = pData.PlayerName + "Dummy";
        poolablePlayer.SetFlipX(i % 2 == 0);
        if (impostorPositioning)
        {
            poolablePlayer.transform.localPosition = new Vector3((num * num2) * 1.5f, -1f + num2 * 0.15f, num3 + num2 * 0.01f) * 1f;
            float num4 = (1f - num2 * 0.075f) * 1f;
            var vector = new Vector3(num4, num4, 1f);
            poolablePlayer.transform.localScale = vector;
            poolablePlayer.SetNameScale(vector.Inv());
        }
        else
        {
            int num5 = num2 / maxDepth;
            float num6 = Mathf.Lerp(1f, 0.75f, num5);
            poolablePlayer.transform.localPosition = new Vector3(0.9f * num * num2 * num6, FloatRange.SpreadToEdges(-1.125f, 0f, num2, maxDepth), num3 + num2 * 0.01f) * 1f;
            float num7 = Mathf.Lerp((i == 0) ? 1.2f : 1f, 0.65f, num5) * 1f;
            poolablePlayer.transform.localScale = new Vector3(num7, num7, 1f);
        }
        poolablePlayer.UpdateFromEitherPlayerDataOrCache(pData, PlayerOutfitType.Default, PlayerMaterial.MaskType.None, true, null);
        if (impostorPositioning)
        {
            var namePosition = new Vector3(0f, -1.31f, -0.5f);
            poolablePlayer.SetNamePosition(namePosition);
            poolablePlayer.SetName(pData.PlayerName);
            poolablePlayer.SetNameColor(TownOfUsColors.Impostor);
        }
        poolablePlayer.ToggleName(impostorPositioning);
        __result = poolablePlayer;
        return false;
    }

    public static void SetHiddenImpostors(IntroCutscene __instance)
    {
        var amount = Helpers.GetAlivePlayers().Count(x => x.IsImpostor());
        __instance.ImpostorText.text =
            DestroyableSingleton<TranslationController>.Instance.GetString(
                amount == 1 ? StringNames.NumImpostorsS : StringNames.NumImpostorsP, amount);
        __instance.ImpostorText.text = __instance.ImpostorText.text.Replace("[FF1919FF]", "<color=#FF1919FF>");
        __instance.ImpostorText.text = __instance.ImpostorText.text.Replace("[]", "</color>");

        if (!OptionGroupSingleton<RoleOptions>.Instance.RoleListEnabled)
        {
            return;
        }

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

        if (!buckets.Any(x => x is RoleListOption.Any))
        {
            return;
        }


        __instance.ImpostorText.text =
            DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NumImpostorsP, 256);
        __instance.ImpostorText.text = __instance.ImpostorText.text.Replace("[FF1919FF]", "<color=#FF1919FF>");
        __instance.ImpostorText.text = __instance.ImpostorText.text.Replace("[]", "</color>");
        __instance.ImpostorText.text = __instance.ImpostorText.text.Replace("256", "???");
    }
}