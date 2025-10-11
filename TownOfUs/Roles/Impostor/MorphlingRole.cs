using System.Globalization;
using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class MorphlingRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    [HideFromIl2Cpp] public PlayerControl? Sampled { get; set; }
    public DoomableType DoomHintType => DoomableType.Perception;
    public string LocaleKey => "Morphling";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");
    public static string MorphedString = TouLocale.GetParsed("TouRoleMorphlingTabMorphed");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorConcealing;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Morphling,
        CanUseVent = OptionGroupSingleton<MorphlingOptions>.Instance.CanVent,
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Shapeshifter)
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
            stringB.Append(CultureInfo.InvariantCulture,
                $"\n<b>{MorphedString.Replace("<player>", $"{Sampled.Data.Color.ToTextColor()}{Sampled.Data.PlayerName}</color>")}</b>");
        }

        return stringB;
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Sample", "Sample"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}SampleWikiDescription"),
                    TouImpAssets.SampleSprite),
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Morph", "Morph"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}MorphWikiDescription"),
                    TouImpAssets.MorphSprite)
            };
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
        MorphedString = TouLocale.GetParsed("TouRoleMorphlingTabMorphed");
        CustomButtonSingleton<MorphlingMorphButton>.Instance.SetActive(false, this);
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        Clear();
    }

    public void Clear()
    {
        Sampled = null;
    }
}