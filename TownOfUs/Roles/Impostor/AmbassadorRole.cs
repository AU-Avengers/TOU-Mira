using System.Globalization;
using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Events;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class AmbassadorRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public DoomableType DoomHintType => DoomableType.Insight;
    public string LocaleKey => "Ambassador";
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
                new(TouLocale.GetParsed($"TouRole{LocaleKey}RetrainWiki", "Retrain (Meeting)"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}RetrainWikiDescription"),
                    TouAssets.RetrainCleanSprite)
            };
        }
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorPower;
    public NetworkedPlayerInfo? SelectedPlr { get; private set; }
    public RoleBehaviour? SelectedRole { get; private set; }
    public int RetrainsAvailable { get; set; }
    public int RoundsCooldown { get; set; }
    private MeetingMenu meetingMenu;

    public void Clear()
    {
        SelectedPlr = null;
        SelectedRole = null;
    }

    public CustomRoleConfiguration Configuration => new(this)
    {
        MaxRoleCount = 1,
        Icon = TouRoleIcons.Ambassador
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);

        stringB.AppendLine(CultureInfo.InvariantCulture,
            $"{RetrainsString()}");

        return stringB;
    }

    public static string AvailableRetrainsString = TouLocale.GetParsed("TouRoleAmbassadorRetrainsAvailable");
    public static string RetrainWaitString = TouLocale.GetParsed("TouRoleAmbassadorRetrainWaiting");
    public static string RetrainCooldownString = TouLocale.GetParsed("TouRoleAmbassadorRetrainCooldown");

    public string RetrainsString()
    {
        return AvailableRetrainsString.Replace("<retrainsLeft>", $"{RetrainsAvailable}").Replace("<retrainsTotal>",
            $"{OptionGroupSingleton<AmbassadorOptions>.Instance.MaxRetrains}");
    }

    public string RetrainCdString()
    {
        return RetrainCooldownString.Replace("<roundsLeft>", $"{RoundsCooldown}").Replace("<roundsTotal>",
            $"{OptionGroupSingleton<AmbassadorOptions>.Instance.RoundCooldown}");
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        AvailableRetrainsString = TouLocale.GetParsed("TouRoleAmbassadorRetrainsAvailable");
        RetrainWaitString = TouLocale.GetParsed("TouRoleAmbassadorRetrainWaiting");
        RetrainCooldownString = TouLocale.GetParsed("TouRoleAmbassadorRetrainCooldown");

        RetrainsAvailable = (int)OptionGroupSingleton<AmbassadorOptions>.Instance.MaxRetrains;

        SelectedPlr = null;
        SelectedRole = null;

        if (Player.AmOwner)
        {
            meetingMenu = new MeetingMenu(
                this,
                Click,
                MeetingAbilityType.Toggle,
                TouAssets.RetrainSprite,
                TouAssets.RetrainSprite,
                IsExempt,
                activeColor: new Color32(200, 80, 80, 255))
            {
                Position = new Vector3(-0.40f, 0f, -3f),
            };
        }
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        if (RetrainsAvailable <= 0)
        {
            return;
        }

        if (DeathEventHandlers.CurrentRound <
            (int)OptionGroupSingleton<AmbassadorOptions>.Instance.RoundWhenAvailable)
        {
            return;
        }

        if (RoundsCooldown > 0)
        {
            return;
        }

        if (Player.AmOwner)
        {
            meetingMenu?.GenButtons(MeetingHud.Instance,
                Player.AmOwner && !Player.HasDied() && !Player.HasModifier<JailedModifier>());
        }
    }

    public override void OnVotingComplete()
    {
        RoleBehaviourStubs.OnVotingComplete(this);

        if (Player.AmOwner)
        {
            meetingMenu?.HideButtons();
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        SelectedPlr = null;

        if (Player.AmOwner)
        {
            meetingMenu?.Dispose();
            meetingMenu = null!;
        }

        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
    }

    public void Click(PlayerVoteArea voteArea, MeetingHud __)
    {
        var player = GameData.Instance.GetPlayerById(voteArea.TargetPlayerId);

        if (SelectedPlr == player)
        {
            RpcRetrain(PlayerControl.LocalPlayer);
            meetingMenu.Actives[voteArea.TargetPlayerId] = false;
            return;
        }

        if (SelectedPlr != null)
        {
            meetingMenu.Actives[voteArea.TargetPlayerId] = false;
            meetingMenu.Actives[SelectedPlr.PlayerId] = false;
            RpcRetrain(PlayerControl.LocalPlayer);
        }

        var opt = OptionGroupSingleton<AmbassadorOptions>.Instance;
        if ((int)opt.KillsNeeded > 0)
        {
            var killedAmbassPlayers = GameHistory.KilledPlayers.Count(x =>
                x.KillerId == Player.PlayerId && x.VictimId != Player.PlayerId);

            var killedPlayerPlayers = GameHistory.KilledPlayers.Count(x =>
                x.KillerId == voteArea.GetPlayer()?.PlayerId && x.VictimId != voteArea.GetPlayer()?.PlayerId);

            if (killedAmbassPlayers < (int)opt.KillsNeeded && killedPlayerPlayers < (int)opt.KillsNeeded)
            {
                var text =
                    TouLocale.GetParsed("TouRoleAmbassadorNeedKills")
                        .Replace("<requiredKills>", $"{(int)opt.KillsNeeded}");
                var notif1 =
                    Helpers.CreateAndShowNotification(text, Color.white, new Vector3(0f, 1f, -20f),
                        spr: TouRoleIcons.Ambassador.LoadAsset());

                notif1.AdjustNotification();
                return;
            }
        }

        var excluded = MiscUtils.AllRoles
            .Where(x => x is ISpawnChange { NoSpawn: true } || x is ITownOfUsRole
            {
                RoleAlignment: RoleAlignment.ImpostorPower
            }).Select(x => x.Role).ToList();
        var impRoles = MiscUtils.GetRolesToAssign(ModdedRoleTeams.Impostor, x => !excluded.Contains(x.Role))
            .Select(x => x.RoleType).ToList();

        foreach (var player2 in PlayerControl.AllPlayerControls)
        {
            if (player2.IsImpostor() && !player2.AmOwner)
            {
                var role = player2.GetRoleWhenAlive();
                if (role)
                {
                    impRoles.Remove((ushort)role!.Role);
                }

                if (player2.TryGetModifier<AmbassadorRetrainedModifier>(out var retrained))
                {
                    impRoles.Remove((ushort)retrained.PreviousRole.Role);
                }
            }
        }

        var roleList = MiscUtils.GetPotentialRoles()
            .Where(role => role is ICustomRole)
            .Where(role => impRoles.Contains(RoleId.Get(role.GetType())))
            .ToList();

        if (!player._object.Is(RoleAlignment.ImpostorKilling) && !player._object.Is(RoleAlignment.ImpostorPower))
        {
            var curRoleList = MiscUtils.GetPotentialRoles()
                .Where(role => role is ICustomRole)
                .Where(role => impRoles.Contains(RoleId.Get(role.GetType())))
                .ToList();
            foreach (var roleBehaviour in curRoleList)
            {
                if (roleBehaviour is ITownOfUsRole touRole && touRole.RoleAlignment == RoleAlignment.ImpostorKilling)
                {
                    roleList.Remove(roleBehaviour);
                }
            }
        }

        if (Minigame.Instance == null)
        {
            var trainMenu = AmbassadorSelectionMinigame.Create();
            trainMenu.Open(
                roleList,
                role =>
                {
                    if (role != null)
                    {
                        meetingMenu.Actives[voteArea.TargetPlayerId] = true;
                        RpcRetrain(PlayerControl.LocalPlayer, player.PlayerId, (ushort)role.Role);
                    }

                    trainMenu.Close();
                }
            );
        }
    }

    private bool IsExempt(PlayerVoteArea voteArea)
    {
        return Player.Data.IsDead || voteArea.AmDead || voteArea.GetPlayer()?.IsImpostor() == false ||
               voteArea.GetPlayer()?.HasModifier<AmbassadorRetrainedModifier>() == true
               || OptionGroupSingleton<GeneralOptions>.Instance.FFAImpostorMode && !Player.AmOwner;
    }

    [MethodRpc((uint)TownOfUsRpc.RetrainConfirm)]
    public static void RpcRetrainConfirm(PlayerControl ambassador, PlayerControl player, int cooldown, ushort role = 0,
        bool accepted = false)
    {
        if (ambassador.Data.Role is not AmbassadorRole ambassadorRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcRetrainConfirm - Invalid ambassador");
            return;
        }

        if (player != ambassadorRole.SelectedPlr?._object)
        {
            Logger<TownOfUsPlugin>.Error("RpcRetrainConfirm - Retrainee is not valid!");
            return;
        }

        if (ambassadorRole.SelectedPlr == null || ambassadorRole.SelectedRole == null ||
            ambassadorRole.Player.Data.IsDead || ambassadorRole.SelectedPlr.IsDead)
        {
            ambassadorRole.Clear();
            Logger<TownOfUsPlugin>.Error("RpcRetrainConfirm - A player or role check failed");
            return;
        }

        if (MeetingHud.Instance || ExileController.Instance)
        {
            Logger<TownOfUsPlugin>.Error(
                "RpcRetrainConfirm - You thought you were slick, huh? No, you can't retrain outside of rounds!");
            return;
        }

        ambassadorRole.Clear();
        var newRole = RoleManager.Instance.GetRole((RoleTypes)role)!;

        if (accepted)
        {
            --ambassadorRole.RetrainsAvailable;
            ambassadorRole.RoundsCooldown = cooldown;
            var currentTime = 0f;
            if (player.AmOwner)
            {
                currentTime = player.killTimer;
            }

            player.AddModifier<AmbassadorRetrainedModifier>((ushort)player.Data.Role.Role);
            player.ChangeRole(role);

            if (PlayerControl.LocalPlayer.IsImpostor() &&
                (!OptionGroupSingleton<GeneralOptions>.Instance.FFAImpostorMode || ambassador.AmOwner))
            {
                var text =
                    TouLocale.GetParsed("TouRoleAmbassadorPlayerHasBeenRetrained")
                        .Replace("<player>", player.Data.PlayerName);

                if (player.AmOwner)
                {
                    player.SetKillTimer(currentTime);
                    text =
                        TouLocale.GetParsed("TouRoleAmbassadorYouHaveAccepted");
                }

                text = text.Replace("<newRole>", newRole.GetRoleName());
                var notif1 = Helpers.CreateAndShowNotification(text, Color.white, new Vector3(0f, 1f, -20f),
                    spr: newRole.RoleIconWhite ?? TouRoleIcons.Ambassador.LoadAsset());

                notif1.AdjustNotification();
            }
        }
        else if (PlayerControl.LocalPlayer.IsImpostor() &&
                 (!OptionGroupSingleton<GeneralOptions>.Instance.FFAImpostorMode || ambassador.AmOwner))
        {
            var text =
                TouLocale.GetParsed("TouRoleAmbassadorPlayerHasDenied").Replace("<player>", player.Data.PlayerName);

            if (player.AmOwner)
            {
                text =
                    TouLocale.GetParsed("TouRoleAmbassadorYouDeniedRetrain");
            }

            text = text.Replace("<newRole>", newRole.GetRoleName());

            var notif1 = Helpers.CreateAndShowNotification(text, Color.white, new Vector3(0f, 1f, -20f),
                spr: newRole.RoleIconWhite ?? TouRoleIcons.Ambassador.LoadAsset());

            notif1.AdjustNotification();
        }
    }

    [MethodRpc((uint)TownOfUsRpc.RetrainImpostor)]
    private static void RpcRetrain(PlayerControl player, byte playerId = byte.MaxValue, ushort role = 0)
    {
        if (player.Data.Role is not AmbassadorRole ambassador)
        {
            Logger<TownOfUsPlugin>.Error("RpcRetrain - Invalid ambassador");
            return;
        }

        if (playerId == byte.MaxValue || role == 0)
        {
            if (PlayerControl.LocalPlayer.IsImpostor() && ambassador.SelectedPlr != null &&
                (!OptionGroupSingleton<GeneralOptions>.Instance.FFAImpostorMode || player.AmOwner))
            {
                var text =
                    TouLocale.GetParsed("TouRoleAmbassadorRetrainCancelled")
                        .Replace("<player>", ambassador.SelectedPlr.PlayerName);
                var notif1 =
                    Helpers.CreateAndShowNotification(text, Color.white, new Vector3(0f, 1f, -20f),
                        spr: TouRoleIcons.Ambassador.LoadAsset());

                notif1.AdjustNotification();
                if (ambassador.Player.AmOwner)
                {
                    ambassador.meetingMenu.Actives[ambassador.SelectedPlr.PlayerId] = false;
                }
            }

            ambassador.SelectedPlr = null;
            ambassador.SelectedRole = null;
            return;
        }

        ambassador.SelectedPlr = GameData.Instance.GetPlayerById(playerId);
        ambassador.SelectedRole = RoleManager.Instance.GetRole((RoleTypes)role);
        if (PlayerControl.LocalPlayer.IsImpostor() &&
            (!OptionGroupSingleton<GeneralOptions>.Instance.FFAImpostorMode || player.AmOwner))
        {
            var text =
                TouLocale.GetParsed("TouRoleAmbassadorDecidedToRetrain")
                    .Replace("<player>", ambassador.SelectedPlr.PlayerName);
            if (ambassador.SelectedPlr.Object.AmOwner && player.AmOwner)
            {
                text =
                    TouLocale.GetParsed("TouRoleAmbassadorDecidedToRetrainYourself");
            }
            else if (ambassador.SelectedPlr.Object == player)
            {
                text =
                    TouLocale.GetParsed("TouRoleAmbassadorDecidedToRetrainSelf");
            }
            else if (ambassador.SelectedPlr.Object.AmOwner)
            {
                text =
                    TouLocale.GetParsed("TouRoleAmbassadorDecidedToRetrainYou");
            }

            text = text.Replace("<newRole>",
                $"{TownOfUsColors.ImpSoft.ToTextColor()}{ambassador.SelectedRole.GetRoleName()}</color>");
            var notif1 = Helpers.CreateAndShowNotification(text, Color.white, new Vector3(0f, 1f, -20f),
                spr: ambassador.SelectedRole.RoleIconWhite != null
                    ? ambassador.SelectedRole.RoleIconWhite
                    : TouRoleIcons.Ambassador.LoadAsset());

            notif1.AdjustNotification();
        }
    }
}