using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class LookoutRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;
    public DoomableType DoomHintType => DoomableType.Hunter;
    public string RoleName => TouLocale.Get(TouNames.Lookout, "Lookout");
    public string RoleDescription => "Keep Your Eyes Wide Open";
    public string RoleLongDescription => "Watch other crewmates to see what roles interact with them";
    public Color RoleColor => TownOfUsColors.Lookout;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Lookout,
        IntroSound = TouAudio.QuestionSound
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return
            $"The {RoleName} is a Crewmate Investigative role that can watch other players during rounds. During meetings they will see all roles who interact with each watched player."
            + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Watch",
            "Watch a player or multiple, the next meeting you will know which players interacted with the watched ones.",
            TouCrewAssets.WatchSprite)
    ];

    [MethodRpc((uint)TownOfUsRpc.LookoutSeePlayer, SendImmediately = true)]
    public static void RpcSeePlayer(PlayerControl target, PlayerControl source)
    {
        if (!target.TryGetModifier<LookoutWatchedModifier>(out var mod))
        {
            Logger<TownOfUsPlugin>.Error("Not a watched player");
            return;
        }

        var role = source.Data.Role;

        var cachedMod = source.GetModifiers<BaseModifier>().FirstOrDefault(x => x is ICachedRole) as ICachedRole;
        if (cachedMod != null)
        {
            role = cachedMod.CachedRole;
        }

        // Prevents duplicate role entries
        if (!mod.SeenPlayers.Contains(role))
        {
            mod.SeenPlayers.Add(role);
        }
    }
}