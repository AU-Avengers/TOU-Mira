using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Options.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class SurvivorRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, IGuessable
{
    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (!playerControl.AmOwner)
        {
            return;
        }
        ImportantTextTask orCreateTask = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        orCreateTask.Text = $"{TownOfUsColors.Neutral.ToTextColor()}{MiraLocaleManager.Get("NeutralBenignTaskHeader")}</color>";
        orCreateTask.name = "NeutralRoleText";
    }

    public DoomableType DoomHintType => DoomableType.Protective;
    public string IdPart => "Survivor";
    public string RoleName => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}");
    public string RoleDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.TabDescription");

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Safeguard", "Safeguard"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Safeguard.WikiDescription"),
                    TouNeutAssets.VestSprite)
            ];
        }
    }

    public Color RoleColor => TownOfUsColors.Survivor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;

    public RoleAlignment RoleAlignment => RoleAlignment.NeutralBenign;

    // This is so the role can be guessed without requiring it to be enabled normally
    public bool CanBeGuessed =>
        (MiscUtils.GetPotentialRoles()
             .Contains(RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<FairyRole>())) &&
         OptionGroupSingleton<FairyOptions>.Instance.OnTargetDeath is BecomeOptions.Survivor)
        || (MiscUtils.GetPotentialRoles()
                .Contains(RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<ExecutionerRole>())) &&
            OptionGroupSingleton<ExecutionerOptions>.Instance.OnTargetDeath is BecomeOptions.Survivor);

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Survivor.LoadAsset(), "TouMira.Role.Neutral.Survivor", 1.45f),
        IntroSound = TouAudio.ToppatIntroSound,
        Icon = TouRoleIcons.Survivor,
        OptionsScreenshot = TouBanners.NeutralRoleBanner,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };



    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        if (Player.AmOwner && OptionGroupSingleton<SurvivorOptions>.Instance.ScatterOn)
        {
            Player.AddModifier<ScatterModifier>(OptionGroupSingleton<SurvivorOptions>.Instance.ScatterTimer.Value);
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);

        if (Player.AmOwner && OptionGroupSingleton<SurvivorOptions>.Instance.ScatterOn)
        {
            Player.RemoveModifier<ScatterModifier>();
        }
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return !Player.HasDied();
    }

    public bool WinConditionMet()
    {
        var hasLivingHalters = MiscUtils.NKillersAliveCount > 0 ||
                               (MiscUtils.ImpAliveCount > 0 && MiscUtils.CrewKillersAliveCount > 0) ||
                               (MiscUtils.GameHaltersAliveCount > 0 && Helpers.GetAlivePlayers().Count > 1)
                               || Helpers.GetAlivePlayers().All(x =>
                                   (x.IsCrewmate() || x.Is(RoleAlignment.NeutralBenign)) && !x.IsImpostorAligned());
        var survCount = CustomRoleUtils.GetActiveRolesOfType<SurvivorRole>().Count(x => !x.Player.HasDied());

        if (survCount == 0 || MiscUtils.NonGameEndingNeutralCount == 0 || Helpers.GetAlivePlayers().Count > 3 ||
            hasLivingHalters)
        {
            return false;
        }

        return true;
    }
}