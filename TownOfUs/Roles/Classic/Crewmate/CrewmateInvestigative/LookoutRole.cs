using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
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
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Lookout.LoadAsset(), "TouMira.Role.Crewmate.Lookout", 1.45f),
        Icon = TouRoleIcons.Lookout,
        OptionsScreenshot = TouBanners.LookoutRoleBanner,
        IntroSound = TouAudio.SuspenseIntro,
    };



    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Watch", "Watch"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}WatchWikiDescription"),
                    TouCrewAssets.WatchSprite)
            ];
        }
    }

    [MethodRpc((uint)TownOfUsRpc.LookoutSeePlayer)]
    public static void RpcSeePlayer(PlayerControl source, PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(source);
            return;
        }
        if (!target.TryGetModifier<LookoutWatchedModifier>(out var mod))
        {
            Error("Not a watched player");
            return;
        }

        // Fixes desync for when a player dies while interacting.
        var role = source.GetRoleWhenAlive();

        if (source.GetModifiers<BaseModifier>().FirstOrDefault(x => x is ICachedRole) is ICachedRole cachedMod)
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