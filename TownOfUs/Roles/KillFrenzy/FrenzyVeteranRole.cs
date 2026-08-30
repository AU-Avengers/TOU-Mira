using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.GameModes;
using UnityEngine;

namespace TownOfUs.Roles.KillFrenzy;

public sealed class FrenzyVeteranRole(IntPtr cppPtr) : FrenzyRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    public bool WinConditionMet()
    {
        var wwCount = CustomRoleUtils.GetActiveRolesOfType<FrenzyVeteranRole>().Count(x => !x.Player.HasDied());

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
    public override bool IsAffectedByComms => false;

    public string IdPart => "Veteran";

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
            return new List<CustomButtonWikiDescription>
            {
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Alert", "Alert"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Alert.WikiDescription"),
                    TouCrewAssets.AlertSprite)
            };
        }
    }

    public Color RoleColor => TownOfUsColors.Veteran;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.FrenzyKiller;

    public CustomRoleConfiguration Configuration => new(this)
    {
        AssociatedGameMode = typeof(KillFrenzyMode),
        GhostRole = (RoleTypes)RoleId.Get<FrenzyGhostRole>(),
        FreeplayFolder = "Kill Frenzy",
        CanUseVent = false,
        Icon = TouRoleIcons.Veteran,
        IntroSound = TouAudio.ImpostorIntroSound
    };
}