using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Modifiers.Impostor.Venerer;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class VenererRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant, ICustomKillAnimRole
{
    public bool ShouldOverrideKillAnim(OverlayKillAnimation killAnimation, KillOverlayInitData initData,
        bool isCamoCommsOn, bool sensitiveKillAnimsOn)
    {
        if (Player.HasModifier<VenererCamouflageModifier>())
        {
            killAnimation.initData = initData;
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
            return true;
        }

        return false;
    }
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole(RoleId.GetType<HunterRole>());
    public DoomableType DoomHintType => DoomableType.Trickster;
    public string IdPart => "Venerer";

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
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Venerer, "TouMira.Role.Impostor.Venerer", 1.45f),
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        Icon = TouRoleIcons.Venerer
    };

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Camouflage", "Camouflage"),
                    MiraLocaleManager.Get("MiraApi.AbilityType.Basic"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Camouflage.WikiDescription"),
                    TouImpAssets.CamouflageSprite),
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Sprint", "Sprint"),
                    MiraLocaleManager.Get("MiraApi.AbilityType.Basic"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Sprint.WikiDescription"),
                    TouImpAssets.SprintSprite),
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Freeze", "Freeze"),
                    MiraLocaleManager.Get("MiraApi.AbilityType.Basic"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Freeze.WikiDescription"),
                    TouImpAssets.FreezeSprite)
            ];
        }
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        CustomButtonSingleton<VenererAbilityButton>.Instance.UpdateAbility(VenererAbility.None);
    }
}

public enum VenererAbility
{
    None,
    Camouflage,
    Sprint,
    Freeze
}