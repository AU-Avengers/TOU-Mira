using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfUs.AnimationSystem.Events;
using TownOfUs.Modules;
using TownOfUs.Options;
using TownOfUs.Options.Maps;
using TownOfUs.Patches;
using TownOfUs.Roles;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using Action = Il2CppSystem.Action;

namespace TownOfUs.AnimationSystem;

public static class KillAnimationEvents
{
    [RegisterEvent(1000000)]
    public static void AfterKillAnimationHandler(AfterKillAnimationEvent @event)
    {
        if (!@event.ShouldAlterFurther)
        {
            return;
        }

        var killer = @event.Killer.Object;
        var killAnim = @event.Animation;
        var initData = @event.InitData;
        var obscureAnim = HudManagerPatches.CamouflageCommsEnabled &&
                          OptionGroupSingleton<AdvancedSabotageOptions>.Instance.CamoKillScreens;
        if (killer.GetSignificantRole() is ICustomKillAnimRole customAnimRole &&
            customAnimRole.ShouldOverrideKillAnim(killAnim, initData, obscureAnim,
                OptionGroupSingleton<GameMechanicOptions>.Instance.ShowInfoSensitiveKillAnimations.Value))
        {
            @event.Cancel();
            return;
        }

        if (obscureAnim)
        {
            killAnim.initData = initData;
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
            var killerAction = (Action)(() => { killAnim.LoadKillerSkin(outfit); });
            var victimAction = (Action)(() => { killAnim.LoadVictimSkin(initData.victimOutfit); });
            if (killAnim.killerParts)
            {
                killAnim.killerParts.SetBodyType(initData.killerBodyType);
                killAnim.killerParts.UpdateFromPlayerOutfit(outfit, PlayerMaterial.MaskType.None, false, false,
                    killerAction, false);
                killAnim.killerParts.ToggleName(false);
                killAnim.LoadKillerPet(outfit);
                var player = killAnim.killerParts;
                killAnim.killerParts.cosmetics.currentBodySprite.BodySprite.color = outfit.RendererColor;
                if (killAnim.killerParts.cosmetics.GetLongBoi() != null)
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

            if (initData.victimOutfit != null && killAnim.victimParts)
            {
                killAnim.victimHat = initData.victimOutfit.HatId;
                killAnim.victimParts.SetBodyType(initData.victimBodyType);
                killAnim.victimParts.UpdateFromPlayerOutfit(initData.victimOutfit, PlayerMaterial.MaskType.None,
                    false, false, victimAction, false);
                killAnim.victimParts.SetHatLeftFacingVictim(killAnim.leftFacingVictim);
                killAnim.victimParts.ToggleName(false);
                killAnim.LoadVictimPet(initData.victimOutfit);
            }

            @event.Cancel();
        }
    }
}