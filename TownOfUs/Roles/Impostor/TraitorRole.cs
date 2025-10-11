using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class TraitorRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ISpawnChange
{
    [HideFromIl2Cpp] public List<RoleBehaviour> ChosenRoles { get; } = [];
    [HideFromIl2Cpp] public RoleBehaviour? RandomRole { get; set; }
    [HideFromIl2Cpp] public RoleBehaviour? SelectedRole { get; set; }
    public DoomableType DoomHintType => DoomableType.Trickster;
    public bool NoSpawn => true;
    public string LocaleKey => "Traitor";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorPower;

    public CustomRoleConfiguration Configuration => new(this)
    {
        MaxRoleCount = 1,
        Icon = TouRoleIcons.Traitor
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"TouRole{LocaleKey}ChangeRole", "Change Role"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}ChangeRoleWikiDescription"),
                    TouImpAssets.TraitorSelect)
            };
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