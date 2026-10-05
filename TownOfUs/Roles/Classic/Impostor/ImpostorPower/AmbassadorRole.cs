using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class AmbassadorRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<VigilanteRole>());
    public DoomableType DoomHintType => DoomableType.Insight;
    public string IdPart => "Ambassador";

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}RetrainWiki", "Retrain (Meeting)"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Retrain.WikiDescription"),
                    TouAssets.RetrainCleanSprite)
            ];
        }
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorPower;
    [HideFromIl2Cpp] public NetworkedPlayerInfo? SelectedPlr { get; private set; }
    [HideFromIl2Cpp] public RoleBehaviour? SelectedRole { get; private set; }
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
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Ambassador.LoadAsset(), "TouMira.Role.Impostor.Ambassador", 1.45f),
        MaxRoleCount = 1,
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        Icon = TouRoleIcons.Ambassador
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);

        stringB.AppendLine(TownOfUsPlugin.Culture,
            $"{RetrainsString()}");

        return stringB;
    }

    public static string AvailableRetrainsString = MiraLocaleManager.Get("TownOfUsMira.Role.AmbassadorRetrainsAvailable");
    public static string RetrainWaitString = MiraLocaleManager.Get("TownOfUsMira.Role.AmbassadorRetrainWaiting");
    public static string RetrainCooldownString = MiraLocaleManager.Get("TownOfUsMira.Role.AmbassadorRetrainCooldown");

    public string RetrainsString()
    {
        return AvailableRetrainsString.Replace("<retrainsLeft>", $"{RetrainsAvailable}").Replace("<retrainsTotal>",
            $"{OptionGroupSingleton<AmbassadorOptions>.Instance.MaxRetrains}");
    }

    public string RetrainCdString()
    {
        return RetrainCooldownString.Replace("<roundsLeft>", $"{RoundsCooldown}").Replace("<roundsTotal>",
            $"{(int)OptionGroupSingleton<AmbassadorOptions>.Instance.RoundCooldown.Value}");
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        AvailableRetrainsString = MiraLocaleManager.Get("TownOfUsMira.Role.AmbassadorRetrainsAvailable");
        RetrainWaitString = MiraLocaleManager.Get("TownOfUsMira.Role.AmbassadorRetrainWaiting");
        RetrainCooldownString = MiraLocaleManager.Get("TownOfUsMira.Role.AmbassadorRetrainCooldown");

        RetrainsAvailable = (int)OptionGroupSingleton<AmbassadorOptions>.Instance.MaxRetrains;

        SelectedPlr = null;
        SelectedRole = null;

        if (Player.AmOwner)
        {
            meetingMenu = new MeetingMenu(
                this,
                Click,
                MiraLocaleManager.Get("TownOfUsMira.Role.AmbassadorRetrain"),
                MeetingAbilityType.Toggle,
                TouAssets.RetrainCleanSprite,
                TouAssets.RetrainCleanSprite,
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

        if (HudManagerHelper.Instance.CurrentRound <
            (int)OptionGroupSingleton<AmbassadorOptions>.Instance.RoundWhenAvailable)
        {
            return;
        }

        if (RoundsCooldown > 0)
        {
            return;
        }

        var meeting = MeetingHud.Instance;
        if (Player.AmOwner && meeting != null)
        {
            meetingMenu?.GenButtons(meeting,
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
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        TouRoleUtils.ClearTaskHeader(Player);
        SelectedPlr = null;

        if (Player.AmOwner)
        {
            meetingMenu?.Dispose();
            meetingMenu = null!;
        }
    }

    public void Click(PlayerVoteArea voteArea, MeetingHud __)
    {
        var player = GameData.Instance.GetPlayerById(voteArea.PlayerId);

        if (SelectedPlr == player)
        {
            RpcRetrain(PlayerControl.LocalPlayer);
            meetingMenu.Actives[voteArea.PlayerId] = false;
            return;
        }

        if (SelectedPlr != null)
        {
            meetingMenu.Actives[voteArea.PlayerId] = false;
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
                    MiraLocaleManager.Get("TownOfUsMira.Role.AmbassadorNeedKills")
                        .Replace("<requiredKills>", $"{(int)opt.KillsNeeded}");
                var notif1 =
                    Helpers.CreateAndShowNotification(text, Color.white, new Vector3(0f, 1f, -20f),
                        spr: TouRoleIcons.Ambassador.LoadAsset());

                notif1.AdjustNotification();
                return;
            }
        }

        var excluded = MiscUtils.AllRegisteredRoles
            .Where(x => x is ISpawnChange { NoSpawn: true } || x.Role is RoleTypes.Impostor || x.IsDead || x is ITownOfUsRole
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
            .Where(role => impRoles.Contains((ushort)role.Role))
            .ToList();

        if (TutorialManager.InstanceExists)
        {
            impRoles = MiscUtils.GetRegisteredRoles(ModdedRoleTeams.Impostor)
                .Where(x => !excluded.Contains(x.Role))
                .Select(x => (ushort)x.Role).ToList();
            roleList = MiscUtils.AllRegisteredRoles
                .Where(role => impRoles.Contains((ushort)role.Role))
                .ToList();
        }

        if (!player._object.Is(RoleAlignment.ImpostorKilling) && !player._object.Is(RoleAlignment.ImpostorPower))
        {
            var curRoleList = MiscUtils.GetPotentialRoles()
                .Where(role => impRoles.Contains((ushort)role.Role))
                .ToList();

            if (TutorialManager.InstanceExists)
            {
                impRoles = MiscUtils.GetRegisteredRoles(ModdedRoleTeams.Impostor)
                    .Where(x => !excluded.Contains(x.Role))
                    .Select(x => (ushort)x.Role).ToList();
                curRoleList = MiscUtils.AllRegisteredRoles
                    .Where(role => impRoles.Contains((ushort)role.Role))
                    .ToList();
            }
            foreach (var roleBehaviour in curRoleList)
            {
                if (roleBehaviour.GetRoleAlignment() == RoleAlignment.ImpostorKilling)
                {
                    roleList.Remove(roleBehaviour);
                }
            }
        }

        if (!Minigame.Instance)
        {
            if (roleList.Count == 0)
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.ImpSoft.ToTextColor()}No roles are available for the player.</color></b>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Ambassador.LoadAsset());

                notif1.AdjustNotification();
                return;
            }
            var trainMenu = AmbassadorSelectionMinigame.Create();
            trainMenu.Open(
                roleList,
                role =>
                {
                    if (role != null)
                    {
                        meetingMenu.Actives[voteArea.PlayerId] = true;
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
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }
        if (ambassador.Data.Role is not AmbassadorRole ambassadorRole)
        {
            Error("RpcRetrainConfirm - Invalid ambassador");
            return;
        }

        if (player != ambassadorRole.SelectedPlr?._object)
        {
            Error("RpcRetrainConfirm - Retrainee is not valid!");
            return;
        }

        if (ambassadorRole.SelectedPlr == null || ambassadorRole.SelectedRole == null ||
            ambassadorRole.Player.Data.IsDead || ambassadorRole.SelectedPlr.IsDead)
        {
            ambassadorRole.Clear();
            Error("RpcRetrainConfirm - A player or role check failed");
            return;
        }

        if (MeetingHud.Instance || ExileController.Instance)
        {
            Error(
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

            if (PlayerControl.LocalPlayer.IsImpostorAligned() &&
                (!OptionGroupSingleton<GeneralOptions>.Instance.FFAImpostorMode || ambassador.AmOwner ||
                 player.AmOwner))
            {
                var text =
                    MiraLocaleManager.Get("TownOfUsMira.Role.AmbassadorPlayerHasBeenRetrained")
                        .Replace("<player>", player.Data.PlayerName);

                if (player.AmOwner)
                {
                    player.SetKillTimer(currentTime);
                    text =
                        MiraLocaleManager.Get("TownOfUsMira.Role.AmbassadorYouHaveAccepted");
                }

                text = text.Replace("<newRole>", newRole.GetRoleName());
                var notif1 = Helpers.CreateAndShowNotification(text, Color.white, new Vector3(0f, 1f, -20f),
                    spr: newRole.RoleIconWhite ?? TouRoleIcons.Ambassador.LoadAsset());

                notif1.AdjustNotification();
            }
        }
        else if (PlayerControl.LocalPlayer.IsImpostorAligned() &&
                 (!OptionGroupSingleton<GeneralOptions>.Instance.FFAImpostorMode || ambassador.AmOwner))
        {
            var text =
                MiraLocaleManager.Get("TownOfUsMira.Role.AmbassadorPlayerHasDenied").Replace("<player>", player.Data.PlayerName);

            if (player.AmOwner)
            {
                text =
                    MiraLocaleManager.Get("TownOfUsMira.Role.AmbassadorYouDeniedRetrain");
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
            Error("RpcRetrain - Invalid ambassador");
            return;
        }

        if (playerId == byte.MaxValue || role == 0)
        {
            if (PlayerControl.LocalPlayer.IsImpostorAligned() && ambassador.SelectedPlr != null &&
                (!OptionGroupSingleton<GeneralOptions>.Instance.FFAImpostorMode || player.AmOwner))
            {
                var text =
                    MiraLocaleManager.Get("TownOfUsMira.Role.AmbassadorRetrainCancelled")
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
        if (PlayerControl.LocalPlayer.IsImpostorAligned() &&
            (!OptionGroupSingleton<GeneralOptions>.Instance.FFAImpostorMode || player.AmOwner))
        {
            var text =
                MiraLocaleManager.Get("TownOfUsMira.Role.AmbassadorDecidedToRetrain")
                    .Replace("<player>", ambassador.SelectedPlr.PlayerName);
            if (ambassador.SelectedPlr.Object.AmOwner && player.AmOwner)
            {
                text =
                    MiraLocaleManager.Get("TownOfUsMira.Role.AmbassadorDecidedToRetrainYourself");
            }
            else if (ambassador.SelectedPlr.Object == player)
            {
                text =
                    MiraLocaleManager.Get("TownOfUsMira.Role.AmbassadorDecidedToRetrainSelf");
            }
            else if (ambassador.SelectedPlr.Object.AmOwner)
            {
                text =
                    MiraLocaleManager.Get("TownOfUsMira.Role.AmbassadorDecidedToRetrainYou");
            }

            text = text.Replace("<newRole>",
                $"{TownOfUsColors.ImpSoft.ToTextColor()}{ambassador.SelectedRole.GetRoleName()}</color>");
            var notif1 = Helpers.CreateAndShowNotification(text, Color.white, new Vector3(0f, 1f, -20f),
                spr: ambassador.SelectedRole.RoleIconWhite ?? TouRoleIcons.Ambassador.LoadAsset());

            notif1.AdjustNotification();
        }
    }
}