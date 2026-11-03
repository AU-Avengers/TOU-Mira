using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using Action = Il2CppSystem.Action;

namespace TownOfUs.Roles.Impostor;

public sealed class SwooperRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICustomKillAnimRole
{
    public bool ShouldOverrideKillAnim(OverlayKillAnimation killAnimation, KillOverlayInitData initData,
        bool isCamoCommsOn, bool sensitiveKillAnimsOn)
    {
        if (sensitiveKillAnimsOn && Player.HasModifier<SwoopModifier>())
        {
            killAnimation.initData = initData;
            var outfit = new VisualAppearance(PlayerControl.LocalPlayer.GetDefaultAppearance(),
                TownOfUsAppearances.Swooper)
            {
                ColorId = PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId,
                HatId = "hat_NoHat",
                SkinId = "skin_None",
                VisorId = "visor_EmptyVisor",
                PlayerName = string.Empty,
                PetId = "pet_EmptyPet",
                RendererColor = new Color(0f, 0f, 0f, 0.1f),
                NameColor = Color.clear,
                ColorBlindTextColor = Color.clear
            };
            var killerAction = (Action)(() => { killAnimation.LoadKillerSkin(outfit); });
            var victimAction = (Action)(() => { killAnimation.LoadVictimSkin(initData.victimOutfit); });
            if (killAnimation.killerParts)
            {
                killAnimation.killerParts.SetBodyType(initData.killerBodyType);
                killAnimation.killerParts.UpdateFromPlayerOutfit(outfit, PlayerMaterial.MaskType.None, false, false,
                    killerAction, false);
                killAnimation.killerParts.ToggleName(false);
                killAnimation.LoadKillerPet(outfit);
                var player = killAnimation.killerParts;
                killAnimation.killerParts.cosmetics.currentBodySprite.BodySprite.color = outfit.RendererColor;
                if (killAnimation.killerParts.cosmetics.GetLongBoi() != null)
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

            if (initData.victimOutfit != null && killAnimation.victimParts)
            {
                killAnimation.victimHat = initData.victimOutfit.HatId;
                killAnimation.victimParts.SetBodyType(initData.victimBodyType);
                killAnimation.victimParts.UpdateFromPlayerOutfit(initData.victimOutfit, PlayerMaterial.MaskType.None,
                    false, false, victimAction, false);
                killAnimation.victimParts.SetHatLeftFacingVictim(killAnimation.leftFacingVictim);
                killAnimation.victimParts.ToggleName(false);
                killAnimation.LoadVictimPet(initData.victimOutfit);
            }

            killAnimation.killerParts.RawSetAppearance(outfit);
            return true;
        }

        return false;
    }
    public DoomableType DoomHintType => DoomableType.Hunter;
    public string IdPart => "Swooper";
    public string RoleMedDescriptionLocale => $"TownOfUsMira.Role.{IdPart}.TabDescription";

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorConcealing;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Swooper, "TouMira.Role.Impostor.Swooper", 1.45f),
        CanUseVent = OptionGroupSingleton<SwooperOptions>.Instance.CanVent.Value is not SwooperVent.Never,
        Icon = TouRoleIcons.Swooper,
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        IntroSound = TouAudio.PhantomIntroSound
    };

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Swoop", "Swoop"),
                    MiraLocaleManager.Get("MiraApi.AbilityType.Basic"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Swoop.WikiDescription"),
                    TouImpAssets.SwoopSprite),
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Unswoop", "Unswoop"),
                    MiraLocaleManager.Get("MiraApi.AbilityType.Basic"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Unswoop.WikiDescription"),
                    TouImpAssets.UnswoopSprite)
            ];
        }
    }
}