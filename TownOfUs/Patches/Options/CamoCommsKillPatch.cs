using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Options.Maps;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using Action = Il2CppSystem.Action;

namespace TownOfUs.Patches.Options;
[HarmonyPriority(Priority.Last)]
[HarmonyPatch(typeof(HorseWrangleOverlay), nameof(HorseWrangleOverlay.Initialize))]
[HarmonyPatch(typeof(OverlayKillAnimation), nameof(OverlayKillAnimation.Initialize))]
public static class KillOverlayPatch
{
    public static bool Prefix(OverlayKillAnimation __instance, KillOverlayInitData initData)
    {
        if (CustomRoleUtils.GetActiveRolesOfType<DeputyRole>().Any(x => x.ShotThisMeeting))
        {
            var outfit = new VisualAppearance(PlayerControl.LocalPlayer.GetDefaultAppearance(),
                TownOfUsAppearances.Camouflage)
            {
                ColorId = PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId,
                HatId = "hat_NoHat",
                SkinId = "skin_None",
                VisorId = "visor_EmptyVisor",
                PlayerName = string.Empty,
                PetId = "pet_EmptyPet",
                NameVisible = false,
                PlayerMaterialColor = new Color(0, 0, 0, 0),
            };
            initData.victimOutfit = outfit;
            __instance.initData = initData;
            var killerAction = (Action)(() => { __instance.LoadKillerSkin(initData.killerOutfit); });
            var victimAction = (Action)(() => { __instance.LoadVictimSkin(initData.victimOutfit); });
            if (__instance.killerParts)
            {
                __instance.killerParts.SetBodyType(initData.killerBodyType);
                __instance.killerParts.UpdateFromPlayerOutfit(initData.killerOutfit, PlayerMaterial.MaskType.None, false, false, killerAction, false);
                __instance.killerParts.ToggleName(false);
                __instance.LoadKillerPet(initData.killerOutfit);
            }
            if (initData.victimOutfit != null && __instance.victimParts)
            {
                __instance.victimHat = initData.victimOutfit.HatId;
                __instance.victimParts.SetBodyType(initData.victimBodyType);
                __instance.victimParts.UpdateFromPlayerOutfit(initData.victimOutfit, PlayerMaterial.MaskType.None, false, false, victimAction, false);
                __instance.victimParts.SetHatLeftFacingVictim(__instance.leftFacingVictim);
                __instance.victimParts.ToggleName(false);
                __instance.LoadVictimPet(initData.victimOutfit);
            }

            __instance.victimParts = null;

            var horse = __instance.TryCast<HorseWrangleOverlay>();
            if (horse != null)
            {
                Warning($"Adjusting visuals for the sprites...");
                foreach (var sprite in horse.victimSprites)
                {
                    sprite.enabled = false;
                }
            }
            return false;
        }
        if (HudManagerPatches.CamouflageCommsEnabled && OptionGroupSingleton<AdvancedSabotageOptions>.Instance.CamoKillScreens)
        {
            __instance.initData = initData;
            var outfit = new VisualAppearance(PlayerControl.LocalPlayer.GetDefaultAppearance(),
                TownOfUsAppearances.Camouflage)
            {
                ColorId = PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId,
                HatId = "hat_NoHat",
                SkinId = "skin_None",
                VisorId = "visor_EmptyVisor",
                PlayerName = string.Empty,
                PetId = "pet_EmptyPet",
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
                    // Ensure we're using instance materials to prevent shared material issues
                    var bodyMaterial = player.cosmetics.currentBodySprite.BodySprite.material;
                    bodyMaterial.SetColor(ShaderID.BackColor, (Color)outfit.PlayerMaterialBackColor);
                    if (player.cosmetics.GetLongBoi() != null)
                    {
                        var headMaterial = player.cosmetics.GetLongBoi().headSprite.material;
                        var neckMaterial = player.cosmetics.GetLongBoi().neckSprite.material;
                        var foregroundNeckMaterial = player.cosmetics.GetLongBoi().foregroundNeckSprite.material;
                        headMaterial.SetColor(ShaderID.BackColor, (Color)outfit.PlayerMaterialBackColor);
                        neckMaterial.SetColor(ShaderID.BackColor, (Color)outfit.PlayerMaterialBackColor);
                        foregroundNeckMaterial.SetColor(ShaderID.BackColor, (Color)outfit.PlayerMaterialBackColor);
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