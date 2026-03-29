using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Interfaces;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class OfficerRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITouCrewRole, IWikiDiscoverable, IDoomable, ILoyalCrewmate
{
    public bool CanBeTraitor => true;
    public bool CanBeCrewpostor => false;
    public bool CanBeEgotist => true;
    public bool CanBeOtherEvil => true;
    public int RoundsBeforeReset { get; set; }
    public int TotalBullets { get; set; } = -1;
    public int LoadedBullets { get; set; }
    public override bool IsAffectedByComms => false;
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string LocaleKey => "Officer";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Load", "Load"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}LoadWikiDescription"),
                    TouCrewAssets.OfficerLoadSprite),
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Shoot", "Shoot"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}ShootWikiDescription"),
                    TouCrewAssets.OfficerShootSprite)
            };
        }
    }

    public Color RoleColor => TownOfUsColors.Officer;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateKilling;
    public bool IsPowerCrew => TotalBullets == -1 || TotalBullets > 0 || LoadedBullets > 0;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Officer,
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        IntroSound = TouAudio.ImpostorIntroSound
    };
    public static string RoundTabWaitString = TouLocale.GetParsed("TouRoleOfficerTabAdditionCount");
    public static string RoundTabWaitNextString = TouLocale.GetParsed("TouRoleOfficerTabAdditionNext");
    public static string RoundTabBasicTabText = TouLocale.GetParsed("TouRoleOfficerTabAdditionKillBasedInno");

    public string RoundWaitString()
    {
        return RoundTabWaitString.Replace("<count>", $"{RoundsBeforeReset}");
    }
    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        RoundTabWaitString = TouLocale.GetParsed("TouRoleOfficerTabAdditionCount");
        RoundTabWaitNextString = TouLocale.GetParsed("TouRoleOfficerTabAdditionNext");
        var opts = OptionGroupSingleton<OfficerOptions>.Instance;
        if (opts.CanOnlyShootActiveKillers.Value)
        {
            RoundTabBasicTabText = TouLocale.GetParsed("TouRoleOfficerTabAdditionKillBasedInno");
        }
        else if (opts.NonKillingNeutralsAreInnocent.Value)
        {
            RoundTabBasicTabText = TouLocale.GetParsed("TouRoleOfficerTabAdditionMajorityInno");
        }
        else
        {
            RoundTabBasicTabText = TouLocale.GetParsed("TouRoleOfficerTabAdditionCrewmatesInno");
        }
        if (Player.AmOwner)
        {
            var shootButton = CustomButtonSingleton<OfficerShootButton>.Instance;
            if (shootButton.TotalBullets == -1)
            {
                shootButton.TotalBullets = (int)opts.MaxBulletsTotal.Value;
                shootButton.RoundsBeforeReset = 0;
                shootButton.LoadedBullets = 0;
            }

            RpcOfficerSyncBullets(Player, shootButton.RoundsBeforeReset, shootButton.TotalBullets, shootButton.LoadedBullets);
        }
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        stringB.AppendLine(TownOfUsPlugin.Culture, $"<b>{RoundTabBasicTabText}</b>");
        if (RoundsBeforeReset > 0)
        {
            var text = RoundsBeforeReset == 1 ? RoundTabWaitNextString : RoundWaitString();
            stringB.AppendLine(TownOfUsPlugin.Culture, $"\n<b>{text}</b>");
        }

        return stringB;
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);
        RoundsBeforeReset--;

        if (Player.AmOwner)
        {
            if (RoundsBeforeReset > 0)
            {
                RpcOfficerSyncBullets(Player, RoundsBeforeReset, TotalBullets, 0);
            }
            else
            {
                var freshBullet = CustomButtonSingleton<OfficerLoadButton>.Instance.RecentlyLoadedBullet;
                RpcOfficerSyncBullets(Player, 0, TotalBullets, freshBullet ? 1 : 0);
            }
        }
    }

    public void LobbyStart()
    {
        var shootButton = CustomButtonSingleton<OfficerShootButton>.Instance;
        shootButton.TotalBullets = -1;
        shootButton.RoundsBeforeReset = 0;
        shootButton.LoadedBullets = 0;
    }

    [MethodRpc((uint)TownOfUsRpc.OfficerMisfire)]
    public static void RpcOfficerMisfire(PlayerControl officer)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(officer);
            return;
        }
        if (officer.Data.Role is not OfficerRole role)
        {
            Error("RpcOfficerMisfire - Invalid officer");
            return;
        }

        role.LoadedBullets--;
        // We add one so that way we know that the current round is checked, as well as the next one that follows.
        OfficerSyncBullets(officer, (int)OptionGroupSingleton<OfficerOptions>.Instance.RoundsPunished.Value + 1, role.TotalBullets, role.LoadedBullets);

        if (GameHistory.PlayerStats.TryGetValue(officer.PlayerId, out var stats))
        {
            stats.IncorrectKills += 1;
        }
    }

    [MethodRpc((uint)TownOfUsRpc.OfficerSyncBullets)]
    public static void RpcOfficerSyncBullets(PlayerControl officer, int roundsBeforeReset, int totalBullets,
        int loadedBullets)
    {
        OfficerSyncBullets(officer, roundsBeforeReset, totalBullets, loadedBullets);
    }

    public static void OfficerSyncBullets(PlayerControl officer, int roundsBeforeReset, int totalBullets,
        int loadedBullets)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(officer);
            return;
        }
        if (officer.Data.Role is not OfficerRole role)
        {
            Error("RpcOfficerMisfire - Invalid officer");
            return;
        }
        role.RoundsBeforeReset = roundsBeforeReset;
        role.TotalBullets = totalBullets;
        role.LoadedBullets = loadedBullets;

        if (officer.AmOwner)
        {
            var button = CustomButtonSingleton<OfficerShootButton>.Instance;
            button.RoundsBeforeReset = roundsBeforeReset;
            button.TotalBullets = totalBullets;
            button.LoadedBullets = loadedBullets;
        }
    }
}