using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class SurvivorRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, IGuessable
{
    public DoomableType DoomHintType => DoomableType.Protective;
    public string LocaleKey => "Survivor";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Safeguard", "Safeguard"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}SafeguardWikiDescription"),
                    TouNeutAssets.VestSprite)
            };
        }
    }

    public Color RoleColor => TownOfUsColors.Survivor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;

    public RoleAlignment RoleAlignment => RoleAlignment.NeutralBenign;

    // This is so the role can be guessed without requiring it to be enabled normally
    public bool CanBeGuessed =>
        (MiscUtils.GetPotentialRoles()
             .Contains(RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<GuardianAngelTouRole>())) &&
         OptionGroupSingleton<GuardianAngelOptions>.Instance.OnTargetDeath is BecomeOptions.Survivor)
        || (MiscUtils.GetPotentialRoles()
                .Contains(RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<ExecutionerRole>())) &&
            OptionGroupSingleton<ExecutionerOptions>.Instance.OnTargetDeath is BecomeOptions.Survivor);

    public CustomRoleConfiguration Configuration => new(this)
    {
        IntroSound = TouAudio.ToppatIntroSound,
        Icon = TouRoleIcons.Survivor,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        if (Player.AmOwner && OptionGroupSingleton<SurvivorOptions>.Instance.ScatterOn)
        {
            Player.AddModifier<ScatterModifier>(OptionGroupSingleton<SurvivorOptions>.Instance.ScatterTimer);
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        if (Player.AmOwner && OptionGroupSingleton<SurvivorOptions>.Instance.ScatterOn)
        {
            Player.RemoveModifier<ScatterModifier>();
        }
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return !Player.HasDied();
    }
}