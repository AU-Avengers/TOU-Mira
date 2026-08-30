using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Hud;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Modifiers;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class TraitorRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ISpawnChange, IGuessable
{

    // This is so the role can be guessed without requiring it to be enabled normally
    public bool CanBeGuessed =>
        RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<TraitorRole>()) is ICustomRole customRole &&
        (int)customRole.GetCount()! > 0 && (int)customRole.GetChance()! > 0 ||
        (int)OptionGroupSingleton<AllianceModifierOptions>.Instance.CrewpostorChance.Value > 0;
    public bool CanSpawnOnCurrentMode() => false;
    [HideFromIl2Cpp] public List<RoleBehaviour> ChosenRoles { get; } = [];
    [HideFromIl2Cpp] public RoleBehaviour? RandomRole { get; set; }
    [HideFromIl2Cpp] public RoleBehaviour? SelectedRole { get; set; }
    public DoomableType DoomHintType => DoomableType.Trickster;
    public bool NoSpawn => true;
    public bool IsDraftable => false;
    public string IdPart => "Traitor";
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
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorPower;

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (Player.AmOwner)
        {
            ButtonResetPatches.ResetCooldowns();
            Player.SetKillTimer(Player.GetKillCooldown());
        }
    }

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Traitor.LoadAsset(), "TouMira.Role.Impostor.Traitor", 1.45f),
        MaxRoleCount = 1,
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        Icon = TouRoleIcons.Traitor
    };

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}ChangeRole", "Change Role"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}ChangeRole.WikiDescription"),
                    TouImpAssets.TraitorSelect)
            ];
        }
    }

    public void Clear()
    {
        ChosenRoles.Clear();
        SelectedRole = null;
    }

    public void UpdateRole()
    {
        if (!SelectedRole)
        {
            return;
        }

        var currenttime = Player.killTimer;

        var roleType = RoleId.Get(SelectedRole!.GetType());
        Player.RpcChangeRole(roleType, false);
        Player.RpcAddModifier<TraitorCacheModifier>();
        SelectedRole = null;

        Player.SetKillTimer(currenttime);
    }
}