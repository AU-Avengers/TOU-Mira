using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Hud;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Buttons.Impostor;
using TownOfUs.GameModes;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Roles.KillFrenzy;

public sealed class FrenzyVenererRole(IntPtr cppPtr) : FrenzyRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    public bool WinConditionMet()
    {
        var wwCount = CustomRoleUtils.GetActiveRolesOfType<FrenzyVenererRole>().Count(x => !x.Player.HasDied());

        if (MiscUtils.KillersAliveCount > wwCount)
        {
            return false;
        }

        return wwCount >= Helpers.GetAlivePlayers().Count - wwCount;
    }

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        var console = usable.TryCast<Console>()!;
        return console == null || console.AllowImpostor;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }
    public string IdPart => "Venerer";
    public string RoleName => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}");
    public string RoleDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.TabDescription");

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.FrenzyKiller;

    public CustomRoleConfiguration Configuration => new(this)
    {
        AssociatedGameMode = typeof(KillFrenzyMode),
        GhostRole = (RoleTypes)RoleId.Get<FrenzyGhostRole>(),
        FreeplayFolder = "Kill Frenzy",
        CanUseVent = false,
        Icon = TouRoleIcons.Venerer
    };



    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Camouflage", "Camouflage"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Camouflage.WikiDescription"),
                    TouImpAssets.CamouflageSprite),
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Sprint", "Sprint"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Sprint.WikiDescription"),
                    TouImpAssets.SprintSprite),
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Freeze", "Freeze"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Freeze.WikiDescription"),
                    TouImpAssets.FreezeSprite)
            };
        }
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        CustomButtonSingleton<VenererAbilityButton>.Instance.UpdateAbility(VenererAbility.None);
    }
}