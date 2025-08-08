using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;
using System.Globalization;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modifiers.Crewmate;
using AmongUs.GameOptions;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Modifiers.Game.Universal;

namespace TownOfUs.Roles.Impostor;

public sealed class BootleggerRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<BarkeeperRole>());
    public override bool IsAffectedByComms => false;
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string RoleName => "Bootlegger";
    public string RoleDescription => "Roleblock Impostors To Stop Them";
    public string RoleLongDescription => "Roleblock the crew to slow down their progress";
    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Bootlegger,
        // IntroSound = TouAudio.ToppatIntroSound,
        MaxRoleCount = 15
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var sb = ITownOfUsRole.SetNewTabText(this);
        var formatProvider = CultureInfo.InvariantCulture;
        var rbdur = OptionGroupSingleton<BootleggerOptions>.Instance.RoleblockDuration;

        // Add a blank line before extra info for spacing
        sb.AppendLine();

        sb.AppendLine(formatProvider, $"Roleblocked players are roleblocked for {rbdur} second(s).");

        if (OptionGroupSingleton<BootleggerOptions>.Instance.Hangover)
            sb.AppendLine("Your target will have a hangover when their roleblock expires.");

        return sb;
    }
    public string GetAdvancedDescription()
    {
        var rbdur = OptionGroupSingleton<BootleggerOptions>.Instance.RoleblockDuration;
        var desc = $"The Bootlegger is an Impostor Support role that can roleblock other players, roleblocking them for {rbdur} second(s).";

        if (OptionGroupSingleton<BootleggerOptions>.Instance.Hangover)
            desc += "\n\nOnce the roleblock expires, the player will be hungover, preventing them from being roleblocked again too quickly.";

        return desc + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Drink",
            $"Drink with a player, roleblocking them for {OptionGroupSingleton<BootleggerOptions>.Instance.RoleblockDuration} second(s)",
            TouCrewAssets.RoleblockButtonSprite)
    ];

    [MethodRpc((uint)TownOfUsRpc.BootleggerRoleblock, SendImmediately = true)]
    public static void RpcRoleblock(PlayerControl player, PlayerControl target)
    {
        var targetName = target.CachedPlayerData.PlayerName;
        var iconSelf = TouRoleIcons.Bootlegger.LoadAsset();
        var iconTarget = TouRoleIcons.Barkeeper.LoadAsset();

        if (!(target.HasModifier<BarkeeperHangoverModifier>() || target.HasModifier<BootleggerHangoverModifier>() || target.HasModifier<DrunkModifier>())) target.AddModifier<BootleggerRoleblockedModifier>();

        if (player.AmOwner)
        {
            ShowNotification($"{targetName} was roleblocked!", iconSelf);
        }

        if (target.AmOwner)
        {
            if (target.HasModifier<BarkeeperHangoverModifier>() || target.HasModifier<BootleggerHangoverModifier>() || target.HasModifier<DrunkModifier>()) ShowNotification($"Someone gave you a drink, but you are too hungover!", iconTarget);
            else ShowNotification($"Someone gave you a drink, you were roleblocked!", iconTarget);
        }

        static void ShowNotification(string message, Sprite icon)
        {
            var notif = Helpers.CreateAndShowNotification($"<b>{message}</b>", Color.white, new Vector3(0f, 1f, -20f), spr: icon);
            notif.Text.SetOutlineThickness(0.35f);
        }
    }

}