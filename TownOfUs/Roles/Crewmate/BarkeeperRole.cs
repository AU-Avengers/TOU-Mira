using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using System.Globalization;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modifiers.Other;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Roles.Crewmate;

public sealed class BarkeeperRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string RoleName => "Barkeeper";
    public string RoleDescription => "Roleblock Evildoers to slow them down";
    public string RoleLongDescription => "Roleblock Evildoers to disable their abilities.";
    public Color RoleColor => TownOfUsColors.Barkeeper;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Barkeeper,
        // IntroSound = TouAudio.ToppatIntroSound,
        MaxRoleCount = 15
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var sb = ITownOfUsRole.SetNewTabText(this);
        var formatProvider = CultureInfo.InvariantCulture;
        var rbdur = OptionGroupSingleton<BarkeeperOptions>.Instance.RoleblockDuration;

        // Add a blank line before extra info for spacing
        sb.AppendLine();

        sb.AppendLine(formatProvider, $"Roleblocked players are roleblocked for {rbdur} second(s).");

        if (OptionGroupSingleton<BarkeeperOptions>.Instance.Hangover)
            sb.AppendLine("Your target will have a hangover when their roleblock expires.");

        return sb;
    }
    public string GetAdvancedDescription()
    {
        var rbdur = OptionGroupSingleton<BarkeeperOptions>.Instance.RoleblockDuration;
        var desc = $"The Barkeeper is a Crewmate Support role that can roleblock other players, roleblocking them for {rbdur} second(s).";

        if (OptionGroupSingleton<BarkeeperOptions>.Instance.Hangover)
            desc += "\n\nOnce the roleblock expires, the player will be hungover, preventing them from being roleblocked again too quickly.";

        return desc + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Drink",
            $"Drink with a player, roleblocking them for {OptionGroupSingleton<BarkeeperOptions>.Instance.RoleblockDuration} second(s)",
            TouCrewAssets.CleanseSprite)
    ];

    [MethodRpc((uint)TownOfUsRpc.Roleblock)]
    public static void RpcRoleblock(PlayerControl player, PlayerControl target)
    {
        var options = OptionGroupSingleton<BarkeeperOptions>.Instance;
        var roleblockDuration = options.RoleblockDuration;
        var hangoverDuration = options.HangoverDuration.Value;
        var applyHangover = options.Hangover;
        var invertControls = options.InvertControlsOfRoleblocked;
        var iconSelf = TouRoleIcons.Barkeeper.LoadAsset();
        var iconTarget = TouRoleIcons.Barkeeper.LoadAsset();
        if (player.Data.Role is BootleggerRole)
        {
            var options2 = OptionGroupSingleton<BootleggerOptions>.Instance;
            roleblockDuration = options2.RoleblockDuration;
            hangoverDuration = options2.HangoverDuration.Value;
            applyHangover = options2.Hangover;
            invertControls = options2.InvertControlsOfRoleblocked;
            iconSelf = TouRoleIcons.Bootlegger.LoadAsset();
        }
        var targetName = target.CachedPlayerData.PlayerName;

        var immune = true;
        if (!target.HasModifier<HangoverModifier>() && !target.HasModifier<DrunkModifier>() &&
            !target.HasModifier<RoleblockedModifier>() && target.Data.Role is not BootleggerRole &&
            target.Data.Role is not BarkeeperRole)
        {
            immune = false;
            target.AddModifier<RoleblockedModifier>(invertControls, applyHangover, roleblockDuration, hangoverDuration);
        }

        if (player.AmOwner)
        {
            ShowNotification($"{targetName} was roleblocked!", iconSelf);
        }

        if (target.AmOwner)
        {
            if (immune)
            {
                ShowNotification($"Someone gave you a drink, but you are too hungover!", iconTarget);
            }
            else
            {
                ShowNotification($"Someone gave you a drink, you were roleblocked!", iconTarget);
            }
        }


        static void ShowNotification(string message, Sprite icon)
        {
            var notif = Helpers.CreateAndShowNotification($"<b>{message}</b>", Color.white, new Vector3(0f, 1f, -20f), spr: icon);
            notif.AdjustNotification();
        }
    }

}