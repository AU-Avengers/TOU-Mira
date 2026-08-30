using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class OracleRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;
    public DoomableType DoomHintType => DoomableType.Insight;
    public string IdPart => "Oracle";
    public string RoleName => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}");
    public string RoleDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.TabDescription");

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription").Replace("<revealAccuracy>",
                $"{OptionGroupSingleton<OracleOptions>.Instance.RevealAccuracyPercentage}") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Bless", "Bless"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Bless.WikiDescription"),
                    TouCrewAssets.BlessSprite),
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Confess", "Confess"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Confess.WikiDescription").Replace("<revealAccuracy>",
                        $"{OptionGroupSingleton<OracleOptions>.Instance.RevealAccuracyPercentage}"),
                    TouCrewAssets.ConfessSprite)
            ];
        }
    }

    public Color RoleColor => TownOfUsColors.Oracle;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateProtective;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Oracle.LoadAsset(), "TouMira.Role.Crewmate.Oracle", 1.45f),
        Icon = TouRoleIcons.Oracle,
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        IntroSound = TouAudio.GuardianAngelSound
    };



    public override void OnDeath(DeathReason reason)
    {
        RoleBehaviourStubs.OnDeath(this, reason);

        RpcOracleConfess(Player);
    }

    public void ReportOnConfession()
    {
        if (!Player.AmOwner)
        {
            return;
        }

        var confessing = ModifierUtils
            .GetPlayersWithModifier<OracleConfessModifier>([HideFromIl2Cpp](x) => x.Oracle == Player).FirstOrDefault();

        if (confessing == null)
        {
            return;
        }

        var report = BuildReport(confessing);

        var title = $"<color=#{TownOfUsColors.Oracle.ToHtmlStringRGBA()}>{MiraLocaleManager.Get("TownOfUsMira.Role.OracleConfessionTitle")}</color>";
        MiscUtils.AddFakeChat(confessing.Data, title, report, false, true);
    }

    public static string BuildReport(PlayerControl player)
    {
        if (player.HasDied())
        {
            return MiraLocaleManager.Get("TownOfUsMira.Role.OracleConfessorDied");
        }

        var allPlayers = PlayerControl.AllPlayerControls.ToArray()
            .Where(x => !x.HasDied() && !x.AmOwner && x != player).ToList();
        if (allPlayers.Count < 2)
        {
            return MiraLocaleManager.Get("TownOfUsMira.Role.OracleTooFew");
        }

        var options = OptionGroupSingleton<OracleOptions>.Instance;

        var evilPlayers = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.HasDied() &&
                                                                               (x.IsImpostor() ||
                                                                                   (x.Is(RoleAlignment.NeutralOutlier) &&
                                                                                       options.ShowNeutralOutlierAsEvil.Value) ||
                                                                                   (x.Is(RoleAlignment
                                                                                           .NeutralKilling) &&
                                                                                       options
                                                                                           .ShowNeutralKillingAsEvil.Value) ||
                                                                                   (x.Is(RoleAlignment.NeutralEvil) &&
                                                                                       options.ShowNeutralEvilAsEvil.Value) ||
                                                                                   (x.Is(RoleAlignment.NeutralBenign) &&
                                                                                       options
                                                                                           .ShowNeutralBenignAsEvil.Value)))
            .ToList();

        if (evilPlayers.Count == 0)
        {
            return MiraLocaleManager.Get("TownOfUsMira.Role.OracleNoMoreEvil")
                .Replace("<player>", player.GetDefaultAppearance().PlayerName);
        }

        allPlayers.Shuffle();
        evilPlayers.Shuffle();
        var secondPlayer = allPlayers[0];
        var firstTwoEvil = evilPlayers.Any(plr => plr == player || plr == secondPlayer);

        if (firstTwoEvil)
        {
            var thirdPlayer = allPlayers[1];

            return MiraLocaleManager.Get("TownOfUsMira.Role.OracleThreePlayers")
                .Replace("<player1>", player.GetDefaultAppearance().PlayerName)
                .Replace("<player2>", secondPlayer.GetDefaultAppearance().PlayerName)
                .Replace("<player3>", thirdPlayer.GetDefaultAppearance().PlayerName);
        }
        else
        {
            var thirdPlayer = evilPlayers[0];

            return MiraLocaleManager.Get("TownOfUsMira.Role.OracleThreePlayers")
                .Replace("<player1>", player.GetDefaultAppearance().PlayerName)
                .Replace("<player2>", secondPlayer.GetDefaultAppearance().PlayerName)
                .Replace("<player3>", thirdPlayer.GetDefaultAppearance().PlayerName);
        }
    }

    [MethodRpc((uint)TownOfUsRpc.OracleConfess)]
    public static void RpcOracleConfess(PlayerControl player)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }
        var mod = ModifierUtils.GetActiveModifiers<OracleConfessModifier>(x => x.Oracle == player).FirstOrDefault();

        mod?.ConfessToAll = true;
    }

    [MethodRpc((uint)TownOfUsRpc.OracleBless)]
    public static void RpcOracleBless(PlayerControl exiled)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(exiled);
            return;
        }
        // Message($"RpcOracleBless exiled '{exiled.Data.PlayerName}'");
        var mod = exiled.GetModifier<OracleBlessedModifier>();

        mod?.SavedFromExile = true;
    }

    [MethodRpc((uint)TownOfUsRpc.OracleBlessNotify)]
    public static void RpcOracleBlessNotify(PlayerControl source, PlayerControl oracle, PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(source);
            return;
        }
        if (oracle.Data.Role is not OracleRole || !source.AmOwner && !oracle.AmOwner)
        {
            Error("RpcOracleBlessNotify - Invalid oracle");
            return;
        }

        if (oracle.AmOwner)
        {
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Oracle));
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{MiraLocaleManager.Get("TownOfUsMira.Role.OracleBlessingMessageSelf").Replace("<player>", target.Data.PlayerName)}</b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Oracle.LoadAsset());
            notif1.AdjustNotification();
        }
        else if (source.AmOwner)
        {
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Oracle));
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{MiraLocaleManager.Get("TownOfUsMira.Role.OracleBlessingMessageOthers").Replace("<player>", target.Data.PlayerName)}</b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Oracle.LoadAsset());
            notif1.AdjustNotification();
        }
    }
}