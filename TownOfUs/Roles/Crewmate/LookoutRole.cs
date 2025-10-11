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
    public string LocaleKey => "Lookout";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

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

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Watch", "Watch"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}WatchWikiDescription"),
                    TouCrewAssets.WatchSprite)
            };
        }
    }

    [MethodRpc((uint)TownOfUsRpc.LookoutSeePlayer)]
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