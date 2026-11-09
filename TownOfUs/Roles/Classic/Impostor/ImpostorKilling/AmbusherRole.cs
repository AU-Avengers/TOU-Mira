using System.Collections;
using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class AmbusherRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant, ICustomKillAnimRole
{
    public bool ShouldOverrideKillAnim(OverlayKillAnimation killAnimation, KillOverlayInitData initData,
        bool isCamoCommsOn, bool sensitiveKillAnimsOn)
    {
        if (Player.HasModifier<AmbusherConcealedModifier>())
        {
            killAnimation.initData = initData;
            var outfit = new VisualAppearance(PlayerControl.LocalPlayer.GetDefaultAppearance(),
                TownOfUsAppearances.Camouflage)
            {
                ColorId = 6,
                HatId = "hat_NoHat",
                SkinId = "skin_None",
                VisorId = "visor_EmptyVisor",
                PlayerName = string.Empty,
                PetId = "pet_EmptyPet",
                NameVisible = false,
                PlayerMaterialBackColor = Color.black,
                PlayerMaterialVisorColor = new Color32(70, 70, 70, 255),
                PlayerMaterialColor = new Color32(70, 70, 70, 255),
                RendererColor = new Color(1f, 1f, 1f, 0.4f)
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
    public RoleBehaviour CrewVariant => RoleId.GetRole<SonarRole>();
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string IdPart => "Ambusher";

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }
    public static string PursuingString = MiraLocaleManager.Get("TownOfUsMira.Role.AmbusherTabPursuingPlayer");

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorKilling;
    [HideFromIl2Cpp] public PlayerControl? Pursued { get; set; }

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Ambusher, "TouMira.Role.Impostor.Ambusher", 1.45f),
        Icon = TouRoleIcons.Ambusher,
        IntroSound = TouAudio.SneakyIntro,
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        CanUseVent = OptionGroupSingleton<AmbusherOptions>.Instance.CanVent
    };

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Pursue", "Pursue"),
                    MiraLocaleManager.Get("MiraApi.AbilityType.Interaction"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Pursue.WikiDescription"),
                    TouImpAssets.PursueSprite),
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Ambush", "Ambush"),
                    MiraLocaleManager.Get("MiraApi.AbilityType.Indirect"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Ambush.WikiDescription"),
                    TouImpAssets.AmbushSprite)
            ];
        }
    }

    public void LobbyStart()
    {
        Clear();
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);

        if (Pursued && Pursued != null)
        {
            stringB.Append(TownOfUsPlugin.Culture,
                $"\n<b>{PursuingString.Replace("<player>", $"{Pursued.Data.Color.ToTextColor()}{Pursued.Data.PlayerName}</color>")}</b>");
        }

        return stringB;
    }

    public override void OnVotingComplete()
    {
        RoleBehaviourStubs.OnVotingComplete(this);

        Clear();
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        PursuingString = MiraLocaleManager.Get("TownOfUsMira.Role.AmbusherTabPursuingPlayer");
        CustomButtonSingleton<AmbusherAmbushButton>.Instance.SetActive(false, this);
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);

        Clear();
    }

    public void Clear()
    {
        Pursued = null;
    }

    public void CheckDeadPursued()
    {
        if (Pursued != null && Pursued.HasDied())
        {
            Pursued = null;
        }
    }

    [MethodRpc((uint)TownOfUsRpc.AmbushPlayer)]
    public static void RpcAmbushPlayer(PlayerControl ambusher, PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(ambusher);
            return;
        }
        if (ambusher.Data.Role is not AmbusherRole)
        {
            Error("RpcAmbushPlayer - Invalid ambusher");
            return;
        }
        var murderResultFlags = MurderResultFlags.Succeeded;

        var beforeMurderEvent = new BeforeMurderEvent(ambusher, target, true);
        beforeMurderEvent.InvokeEvent();

        if (beforeMurderEvent.IsCancelled)
        {
            murderResultFlags = MurderResultFlags.FailedError;
        }

        var murderResultFlags2 = MurderResultFlags.DecisionByHost | murderResultFlags;

        if (murderResultFlags2.HasFlag(MurderResultFlags.Succeeded) &&
            murderResultFlags2.HasFlag(MurderResultFlags.DecisionByHost))
        {
            GameHistory.UpdatePlayerDeathData(target, MiraLocaleManager.Get("DiedToAmbusherAmbush"), 0, HudManagerHelper.Instance.CurrentRound,
            DeathHandlerOverride.SetTrue,
            MiraLocaleManager.Get("DiedByStringBasic").Replace("<player>", ambusher.Data.PlayerName),
            lockInfo: DeathHandlerOverride.SetTrue, playerState: StoredPlayerState.Dead);
        }

        Coroutines.Start(CoAmbushPlayer(ambusher, target, murderResultFlags2));
    }

    private static IEnumerator CoAmbushPlayer(PlayerControl ambusher, PlayerControl target, MurderResultFlags murderResultFlags2)
    {
        ambusher.AddModifier<AmbusherConcealedModifier>(target);
        yield return new WaitForSeconds(0.1f);
        ambusher.CustomMurder(
            target,
            null,
            true,
            false,
            murderResultFlags2,
            true,
            true,
            false);
    }
}