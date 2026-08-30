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
    public static string MorphedString = MiraLocaleManager.Get("TownOfUsMira.Role.MorphlingTabMorphed");

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
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Sample", "Sample"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Sample.WikiDescription"),
                    TouImpAssets.SampleSprite),
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Morph", "Morph"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Morph.WikiDescription"),
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
        MorphedString = MiraLocaleManager.Get("TownOfUsMira.Role.MorphlingTabMorphed");
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