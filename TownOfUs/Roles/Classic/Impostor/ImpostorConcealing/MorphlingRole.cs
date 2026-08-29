using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class MorphlingRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    [HideFromIl2Cpp] public PlayerControl? Sampled { get; set; }
    public DoomableType DoomHintType => DoomableType.Perception;
    public string IdPart => "Morphling";
    public string RoleName => MiraLocaleManager.Get($"TouRole{IdPart}");
    public string RoleDescription => MiraLocaleManager.Get($"TouRole{IdPart}IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get($"TouRole{IdPart}TabDescription");
    public static string MorphedString = MiraLocaleManager.Get("TouRoleMorphlingTabMorphed");

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TouRole{IdPart}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorConcealing;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Morphling.LoadAsset(), "TouMira.Role.Impostor.Morphling", 1.45f),
        Icon = TouRoleIcons.Morphling,
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        CanUseVent = (MorphlingVent)OptionGroupSingleton<MorphlingOptions>.Instance.CanVent.Value is not MorphlingVent.Never,
        IntroSound = TouAudio.ShapeshifterIntroSound
    };

    public void LobbyStart()
    {
        Clear();
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);

        if (Sampled != null && Player.HasModifier<MorphlingMorphModifier>())
        {
            stringB.Append(TownOfUsPlugin.Culture,
                $"\n<b>{MorphedString.Replace("<player>", $"{Sampled.Data.Color.ToTextColor()}{Sampled.Data.PlayerName}</color>")}</b>");
        }

        return stringB;
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TouRole{IdPart}Sample", "Sample"),
                    MiraLocaleManager.Get($"TouRole{IdPart}SampleWikiDescription"),
                    TouImpAssets.SampleSprite),
                new(MiraLocaleManager.Get($"TouRole{IdPart}Morph", "Morph"),
                    MiraLocaleManager.Get($"TouRole{IdPart}MorphWikiDescription"),
                    TouImpAssets.MorphSprite)
            ];
        }
    }

    public override void OnVotingComplete()
    {
        RoleBehaviourStubs.OnVotingComplete(this);

        Clear();
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        MorphedString = MiraLocaleManager.Get("TouRoleMorphlingTabMorphed");
        CustomButtonSingleton<MorphlingMorphButton>.Instance.SetActive(false, this);
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);

        Clear();
    }

    public void Clear()
    {
        Sampled = null;
    }
}