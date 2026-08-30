using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using TownOfUs.Modifiers;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;
using MiraAPI.Patches.Stubs;
using TownOfUs.Modifiers.Game.Alliance;

namespace TownOfUs.Roles.Crewmate;

public sealed class MonarchRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string IdPart => "Monarch";
    public string RoleName => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}");
    public string RoleDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.TabDescription");

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }
    public Color RoleColor => TownOfUsColors.Monarch;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmatePower;

    public static string VoteInfoString = MiraLocaleManager.Get("TownOfUsMira.Role.MonarchTabVoteInfo");
    public static string DefenseEgoString = MiraLocaleManager.Get("TownOfUsMira.Role.MonarchTabDefenseInfoEgo");
    public static string DefenseString = MiraLocaleManager.Get("TownOfUsMira.Role.MonarchTabDefenseInfo");
    public static string DeathInfoString = MiraLocaleManager.Get("TownOfUsMira.Role.MonarchTabDeathInfo");

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        VoteInfoString = MiraLocaleManager.Get("TownOfUsMira.Role.MonarchTabVoteInfo");
        DefenseEgoString = MiraLocaleManager.Get("TownOfUsMira.Role.MonarchTabDefenseInfoEgo");
        DefenseString = MiraLocaleManager.Get("TownOfUsMira.Role.MonarchTabDefenseInfo");
        DeathInfoString = MiraLocaleManager.Get("TownOfUsMira.Role.MonarchTabDeathInfo");
    }

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Monarch.LoadAsset(), "TouMira.Role.Crewmate.Monarch", 1.45f),
        Icon = TouRoleIcons.Monarch,
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        IntroSound = TouAudio.ToppatIntroSound,
        MaxRoleCount = 1
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var sb = ITownOfUsRole.SetNewTabText(this);
        var votes = (int)OptionGroupSingleton<MonarchOptions>.Instance.VotesPerKnight;

        // Add a blank line before extra info for spacing
        sb.AppendLine();

        sb.AppendLine(TownOfUsPlugin.Culture, $"{VoteInfoString.Replace("<amount>", votes.ToString(TownOfUsPlugin.Culture))}");

        var egoIsThriving = PlayerControl.LocalPlayer?.HasModifier<EgotistModifier>() ?? false;

        if (OptionGroupSingleton<MonarchOptions>.Instance.CrewKnightsGrantKillImmunity)
        {
            if (egoIsThriving)
                sb.AppendLine(TownOfUsPlugin.Culture, $"{DefenseEgoString}");
            else
                sb.AppendLine(TownOfUsPlugin.Culture, $"{DefenseString}");
        }

        if (OptionGroupSingleton<MonarchOptions>.Instance.InformWhenKnightDies)
            sb.AppendLine(TownOfUsPlugin.Culture, $"{DeathInfoString}");

        return sb;
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Knight", "Knight"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}KnightDescription").Replace("<amount>",
                        ((int)OptionGroupSingleton<MonarchOptions>.Instance.VotesPerKnight).ToString(TownOfUsPlugin.Culture)),
                    TouCrewAssets.KnightSprite)
            ];
        }
    }

    [MethodRpc((uint)TownOfUsRpc.Knight)]
    public static void RpcKnight(PlayerControl player, PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }
        if (player.Data.Role is not MonarchRole monarch)
        {
            Error("RpcKnight - Invalid monarch");
            return;
        }
        var targetName = target.CachedPlayerData.PlayerName;
        var icon = TouRoleIcons.Monarch.LoadAsset();

        if (target.HasDied())
        {
            if (player.AmOwner)
            {
                ShowNotification(MiraLocaleManager.Get("TownOfUsMira.Role.MonarchKnightTargetDied").Replace("<player>", targetName));
            }
            return;
        }

        target.AddModifier<KnightedModifier>();

        if (player.AmOwner)
        {
            ShowNotification(MiraLocaleManager.Get("TownOfUsMira.Role.MonarchKnightSuccess").Replace("<player>", targetName));
        }

        if (target.AmOwner && !OptionGroupSingleton<MonarchOptions>.Instance.RevealAtMeeting)
        {
            ShowNotification(MiraLocaleManager.Get("TownOfUsMira.Role.MonarchKnightedFeedback").Replace("<role>", $"{TownOfUsColors.Monarch.ToTextColor()}{monarch.RoleName}</color>").Replace("<votes>", ((int)OptionGroupSingleton<MonarchOptions>.Instance.VotesPerKnight).ToString(TownOfUsPlugin.Culture)));
        }


        void ShowNotification(string message)
        {
            var notif = Helpers.CreateAndShowNotification($"<b>{message}</b>", Color.white, new Vector3(0f, 1f, -20f), spr: icon);
            notif.Text.SetOutlineThickness(0.35f);
        }
    }
}