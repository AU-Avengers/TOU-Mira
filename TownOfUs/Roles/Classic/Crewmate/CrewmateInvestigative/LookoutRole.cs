using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class LookoutRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;
    public DoomableType DoomHintType => DoomableType.Hunter;
    public string IdPart => "Lookout";
    public string RoleName => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}");
    public string RoleDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.IntroBlurb");
    public static string ReworkString => (LookoutView)OptionGroupSingleton<LookoutOptions>.Instance.WatchType.Value is LookoutView.Players ? "Alt" : string.Empty;
    public string RoleLongDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}{ReworkString}.TabDescription");

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
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
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Watch", "Watch"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Watch.WikiDescription"),
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
        mod.SeenPlayers.TryAdd(source, role);
    }
}