using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfUs.Options;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using Action = Il2CppSystem.Action;

namespace TownOfUs.Patches.Options;
[HarmonyPriority(Priority.Last)]
[HarmonyPatch(typeof(OverlayKillAnimation), nameof(OverlayKillAnimation.Initialize))]
public static class KillOverlayPatch
{
    public static bool Prefix(OverlayKillAnimation __instance, KillOverlayInitData initData)
    {
        if (HudManagerPatches.CamouflageCommsEnabled && OptionGroupSingleton<GeneralOptions>.Instance.CamoKillScreens)
        {
            __instance.initData = initData;
            var outfit = new VisualAppearance(PlayerControl.LocalPlayer.GetDefaultAppearance(),
                TownOfUsAppearances.Camouflage)
            {
                ColorId = PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId,
                HatId = string.Empty,
                SkinId = string.Empty,
                VisorId = string.Empty,
                PlayerName = string.Empty,
                PetId = string.Empty,
                NameVisible = false,
                PlayerMaterialColor = Color.grey,
            };
            var killerAction = (Action)(() => { __instance.LoadKillerSkin(outfit); });
            var victimAction = (Action)(() => { __instance.LoadVictimSkin(initData.victimOutfit); });
            if (__instance.killerParts)
            {
                __instance.killerParts.SetBodyType(initData.killerBodyType);
                __instance.killerParts.UpdateFromPlayerOutfit(outfit, PlayerMaterial.MaskType.None, false, false,
                    killerAction, false);
                __instance.killerParts.ToggleName(false);
                __instance.LoadKillerPet(outfit);
                var player = __instance.killerParts;
                __instance.killerParts.cosmetics.currentBodySprite.BodySprite.color = outfit.RendererColor;
                if (__instance.killerParts.cosmetics.GetLongBoi() != null)
                {
                    player.cosmetics.GetLongBoi().headSprite.color = outfit.RendererColor;
                    player.cosmetics.GetLongBoi().neckSprite.color = outfit.RendererColor;
                    player.cosmetics.GetLongBoi().foregroundNeckSprite.color = outfit.RendererColor;
                }

                if (outfit.PlayerMaterialColor != null)
                {
                    PlayerMaterial.SetColors((Color)outfit.PlayerMaterialColor,
                        player.cosmetics.currentBodySprite.BodySprite);
                    if (player.cosmetics.GetLongBoi() != null)
                    {
                        PlayerMaterial.SetColors((Color)outfit.PlayerMaterialColor,
                            player.cosmetics.GetLongBoi().headSprite);
                        PlayerMaterial.SetColors((Color)outfit.PlayerMaterialColor,
                            player.cosmetics.GetLongBoi().neckSprite);
                        PlayerMaterial.SetColors((Color)outfit.PlayerMaterialColor,
                            player.cosmetics.GetLongBoi().foregroundNeckSprite);
                    }
                }

                if (outfit.PlayerMaterialBackColor != null)
                {
                    player.cosmetics.currentBodySprite.BodySprite.material.SetColor(ShaderID.BackColor,
                        (Color)outfit.PlayerMaterialBackColor);
                    if (player.cosmetics.GetLongBoi() != null)
                    {
                        player.cosmetics.GetLongBoi().headSprite.material.SetColor(ShaderID.BackColor,
                            (Color)outfit.PlayerMaterialBackColor);
                        player.cosmetics.GetLongBoi().neckSprite.material.SetColor(ShaderID.BackColor,
                            (Color)outfit.PlayerMaterialBackColor);
                        player.cosmetics.GetLongBoi().foregroundNeckSprite.material.SetColor(ShaderID.BackColor,
                            (Color)outfit.PlayerMaterialBackColor);
                    }
                }

                if (outfit.PlayerMaterialVisorColor != null)
                {
                    player.cosmetics.currentBodySprite.BodySprite.material.SetColor(ShaderID.VisorColor,
                        (Color)outfit.PlayerMaterialVisorColor);
                    if (player.cosmetics.GetLongBoi() != null)
                    {
                        player.cosmetics.GetLongBoi().headSprite.material.SetColor(ShaderID.VisorColor,
                            (Color)outfit.PlayerMaterialVisorColor);
                        player.cosmetics.GetLongBoi().neckSprite.material.SetColor(ShaderID.VisorColor,
                            (Color)outfit.PlayerMaterialVisorColor);
                        player.cosmetics.GetLongBoi().foregroundNeckSprite.material.SetColor(ShaderID.VisorColor,
                            (Color)outfit.PlayerMaterialVisorColor);
                    }
                }
            }

            if (initData.victimOutfit != null && __instance.victimParts)
            {
                __instance.victimHat = initData.victimOutfit.HatId;
                __instance.victimParts.SetBodyType(initData.victimBodyType);
                __instance.victimParts.UpdateFromPlayerOutfit(initData.victimOutfit, PlayerMaterial.MaskType.None,
                    false, false, victimAction, false);
                __instance.victimParts.SetHatLeftFacingVictim(__instance.leftFacingVictim);
                __instance.victimParts.ToggleName(false);
                __instance.LoadVictimPet(initData.victimOutfit);
            }

            return false;
        }

        return true;
    }
}