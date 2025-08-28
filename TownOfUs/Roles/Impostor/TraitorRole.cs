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
    [HideFromIl2Cpp]
    public RoleBehaviour? RandomRole { get; set; }
    [HideFromIl2Cpp]
    public RoleBehaviour? SelectedRole { get; set; }
    public DoomableType DoomHintType => DoomableType.Trickster;
    public bool NoSpawn => true;
    public string RoleName => TouLocale.Get(TouNames.Traitor, "Traitor");
    public string RoleDescription => "Betray The Crewmates!";
    public string RoleLongDescription => "Betray the Crewmates!";
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

    public string GetAdvancedDescription()
    {
        return
            $"The {RoleName} is an Impostor Killing role that spawns after a meeting, in which the spawn conditions are suitable. The Traitor will never be a {TouLocale.Get(TouNames.Mayor, "Mayor")}, and must be a crewmate. The Traitor sets out to win the game for the fallen Impostors, and kill off the crew. They are also able to change to a better role."
            + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Change Role",
            "The Traitor can change their role to one of the provided role cards, or gamble on the random. Once they select a role, they stay as that role until they die. However, they must still be guessed as Traitor.",
            TouImpAssets.TraitorSelect)
    ];

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