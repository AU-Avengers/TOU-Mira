using System.Text;
using HarmonyLib;
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
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class OracleRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;
    public DoomableType DoomHintType => DoomableType.Insight;
    public string LocaleKey => "Oracle";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription").Replace("<revealAccuracy>",
                $"{OptionGroupSingleton<OracleOptions>.Instance.RevealAccuracyPercentage}") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Bless", "Bless"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}BlessWikiDescription"),
                    TouCrewAssets.BlessSprite),
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Confess", "Confess"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}ConfessWikiDescription").Replace("<revealAccuracy>",
                        $"{OptionGroupSingleton<OracleOptions>.Instance.RevealAccuracyPercentage}"),
                    TouCrewAssets.ConfessSprite)
            };
        }
    }

    public Color RoleColor => TownOfUsColors.Oracle;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateProtective;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Oracle,
        IntroSound = TouAudio.GuardianAngelSound
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

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

        var title = $"<color=#{TownOfUsColors.Oracle.ToHtmlStringRGBA()}>Oracle Confession</color>";
        MiscUtils.AddFakeChat(confessing.Data, title, report, false, true);
    }

    public static string BuildReport(PlayerControl player)
    {
        if (player.HasDied())
        {
            return "Your confessor failed to survive so you received no confession";
        }

        var allPlayers = PlayerControl.AllPlayerControls.ToArray()
            .Where(x => !x.HasDied() && x != PlayerControl.LocalPlayer && x != player).ToList();
        if (allPlayers.Count < 2)
        {
            return "Too few people alive to receive a confessional";
        }

        var options = OptionGroupSingleton<OracleOptions>.Instance;

        var evilPlayers = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.HasDied() &&
                                                                               (x.IsImpostor() ||
                                                                                   (x.Is(RoleAlignment
                                                                                           .NeutralKilling) &&
                                                                                       options
                                                                                           .ShowNeutralKillingAsEvil) ||
                                                                                   (x.Is(RoleAlignment.NeutralEvil) &&
                                                                                       options.ShowNeutralEvilAsEvil) ||
                                                                                   (x.Is(RoleAlignment.NeutralBenign) &&
                                                                                       options
                                                                                           .ShowNeutralBenignAsEvil)))
            .ToList();

        if (evilPlayers.Count == 0)
        {
            return
                $"{player.GetDefaultAppearance().PlayerName} confesses to knowing that there are no more evil players!";
        }

        allPlayers.Shuffle();
        evilPlayers.Shuffle();
        var secondPlayer = allPlayers[0];
        var firstTwoEvil = evilPlayers.Any(plr => plr == player || plr == secondPlayer);

        if (firstTwoEvil)
        {
            var thirdPlayer = allPlayers[1];

            return
                $"{player.GetDefaultAppearance().PlayerName} confesses to knowing that they, {secondPlayer.GetDefaultAppearance().PlayerName} and/or {thirdPlayer.GetDefaultAppearance().PlayerName} is evil!";
        }
        else
        {
            var thirdPlayer = evilPlayers[0];

            return
                $"{player.GetDefaultAppearance().PlayerName} confesses to knowing that they, {secondPlayer.GetDefaultAppearance().PlayerName} and/or {thirdPlayer.GetDefaultAppearance().PlayerName} is evil!";
        }
    }

    [MethodRpc((uint)TownOfUsRpc.OracleConfess)]
    public static void RpcOracleConfess(PlayerControl player)
    {
        var mod = ModifierUtils.GetActiveModifiers<OracleConfessModifier>(x => x.Oracle == player).FirstOrDefault();

        if (mod != null)
        {
            mod.ConfessToAll = true;
        }
    }

    [MethodRpc((uint)TownOfUsRpc.OracleBless)]
    public static void RpcOracleBless(PlayerControl exiled)
    {
        // Logger<TownOfUsPlugin>.Message($"RpcOracleBless exiled '{exiled.Data.PlayerName}'");
        var mod = exiled.GetModifier<OracleBlessedModifier>();

        if (mod != null)
            // Logger<TownOfUsPlugin>.Message($"RpcOracleBless exiled '{exiled.Data.PlayerName}' SavedFromExile");
        {
            mod.SavedFromExile = true;
        }
    }

    [MethodRpc((uint)TownOfUsRpc.OracleBlessNotify)]
    public static void RpcOracleBlessNotify(PlayerControl oracle, PlayerControl source, PlayerControl target)
    {
        if (oracle.Data.Role is not OracleRole || !source.AmOwner && !oracle.AmOwner)
        {
            Logger<TownOfUsPlugin>.Error("RpcOracleBlessNotify - Invalid oracle");
            return;
        }

        if (oracle.AmOwner)
        {
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Oracle));
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>Your blessing has saved {TownOfUsColors.Oracle.ToTextColor()}{target.Data.PlayerName}</color> from getting guessed!</b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Oracle.LoadAsset());
            notif1.AdjustNotification();
        }
        else if (source.AmOwner)
        {
            Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Oracle));
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.ImpSoft.ToTextColor()}{target.Data.PlayerName}</color> survived due to being blessed by an {TownOfUsColors.Oracle.ToTextColor()}Oracle</color>!</b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Oracle.LoadAsset());
            notif1.AdjustNotification();
        }
    }
}