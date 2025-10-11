using System.Collections;
using System.Globalization;
using System.Text;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using Random = System.Random;

namespace TownOfUs.Roles.Neutral;

public sealed class PlaguebearerRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public void FixedUpdate()
    {
        if (Player == null || Player.Data.Role is not PlaguebearerRole || Player.HasDied() || !Player.AmOwner)
        {
            return;
        }

        var allInfected =
            ModifierUtils.GetPlayersWithModifier<PlaguebearerInfectedModifier>([HideFromIl2Cpp](x) =>
                x.PlagueBearerId == Player.PlayerId && !x.Player.HasDied());

        if (allInfected.Count() >= Helpers.GetAlivePlayers().Count - 1 &&
            (!MeetingHud.Instance || Helpers.GetAlivePlayers().Count > 2))
        {
            var players =
                ModifierUtils.GetPlayersWithModifier<PlaguebearerInfectedModifier>([HideFromIl2Cpp](x) =>
                    x.PlagueBearerId == Player.PlayerId);

            players.Do(x =>
                x.RpcRemoveModifier<PlaguebearerInfectedModifier>());

            Player.RpcChangeRole(RoleId.Get<PestilenceRole>());

            CustomButtonSingleton<PestilenceKillButton>.Instance.SetTimer(OptionGroupSingleton<PlaguebearerOptions>
                .Instance.PestKillCooldown);
        }
    }

    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<AurialRole>());
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string LocaleKey => "Plaguebearer";
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
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Infect", "Infect"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}InfectWikiDescription"),
                    TouNeutAssets.InfectSprite)
            };
        }
    }

    public Color RoleColor => TownOfUsColors.Plaguebearer;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Phantom),
        Icon = TouRoleIcons.Plaguebearer,
        MaxRoleCount = 1,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);

        var allInfected = PlayerControl.AllPlayerControls.ToArray().Where(x =>
            !x.HasDied() && x != Player &&
            x.GetModifier<PlaguebearerInfectedModifier>()?.PlagueBearerId == Player.PlayerId);

        if (allInfected.Any())
        {
            stringB.Append(CultureInfo.InvariantCulture, $"\n<b>{TouLocale.Get("TouRolePlaguebearerTabInfectedInfo")}</b>");
            foreach (var plr in allInfected)
            {
                stringB.Append(TownOfUsPlugin.Culture, $"\n{Color.white.ToTextColor()}{plr.Data.PlayerName}</color>");
            }
        }

        var notInfected = PlayerControl.AllPlayerControls.ToArray().Where(x =>
            !x.HasDied() && x != Player && !x.HasModifier<PlaguebearerInfectedModifier>());

        stringB.Append(CultureInfo.InvariantCulture, $"\n\n<b>{TouLocale.GetParsed("TouRolePlaguebearerTabInfectCounter").Replace("<count>", $"{notInfected.Count()}")}</b>");

        return stringB;
    }

    public bool WinConditionMet()
    {
        if (Player.HasDied())
        {
            return false;
        }

        var result = Helpers.GetAlivePlayers().Count <= 2 && MiscUtils.KillersAliveCount == 1;

        return result;
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        Player.AddModifier<PlaguebearerInfectedModifier>(Player.PlayerId);
        if (Player.AmOwner && (int)OptionGroupSingleton<PlaguebearerOptions>.Instance.PestChance != 0)
        {
            Coroutines.Start(CheckForPestChance(Player));
        }
    }

    private static IEnumerator CheckForPestChance(PlayerControl player)
    {
        yield return new WaitForSeconds(0.01f);

        Random rnd = new();
        var chance = rnd.Next(1, 101);

        if (chance <= OptionGroupSingleton<PlaguebearerOptions>.Instance.PestChance)
        {
            player.RpcChangeRole(RoleId.Get<PestilenceRole>());
            CustomButtonSingleton<PestilenceKillButton>.Instance.SetTimer(OptionGroupSingleton<PlaguebearerOptions>
                .Instance.PestKillCooldown);
        }
    }

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        var console = usable.TryCast<Console>()!;
        return console == null || console.AllowImpostor;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }

    public static void CheckInfected(PlayerControl source, PlayerControl target)
    {
        if (source.Data.Role is PlaguebearerRole)
        {
            target.AddModifier<PlaguebearerInfectedModifier>(source.PlayerId);
        }
        else if (target.Data.Role is PlaguebearerRole)
        {
            source.AddModifier<PlaguebearerInfectedModifier>(target.PlayerId);
        }
        else if (source.TryGetModifier<PlaguebearerInfectedModifier>(out var mod) &&
                 !target.HasModifier<PlaguebearerInfectedModifier>())
        {
            target.AddModifier<PlaguebearerInfectedModifier>(mod.PlagueBearerId);
        }
        else if (target.TryGetModifier<PlaguebearerInfectedModifier>(out var mod2) &&
                 !source.HasModifier<PlaguebearerInfectedModifier>())
        {
            source.AddModifier<PlaguebearerInfectedModifier>(mod2.PlagueBearerId);
        }
    }

    [MethodRpc((uint)TownOfUsRpc.CheckInfected)]
    public static void RpcCheckInfected(PlayerControl source, PlayerControl target)
    {
        CheckInfected(source, target);
    }
}