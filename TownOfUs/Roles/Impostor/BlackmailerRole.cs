using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class BlackmailerRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public void FixedUpdate()
    {
        if (Player == null || Player.Data.Role is not JanitorRole || Player.HasDied() || !Player.AmOwner ||
            MeetingHud.Instance || (!HudManager.Instance.UseButton.isActiveAndEnabled &&
                                    !HudManager.Instance.PetButton.isActiveAndEnabled))
        {
            return;
        }

        HudManager.Instance.KillButton.ToggleVisible(
            OptionGroupSingleton<BlackmailerOptions>.Instance.BlackmailerKill ||
            (Player != null && Player.GetModifiers<BaseModifier>().Any(x => x is ICachedRole)) ||
            (Player != null && MiscUtils.ImpAliveCount == 1));
    }

    public DoomableType DoomHintType => DoomableType.Insight;
    public string RoleName => TouLocale.Get(TouNames.Blackmailer, "Blackmailer");
    public string RoleDescription => "Silence Crewmates During Meetings";
    public string RoleLongDescription => "Silence a crewmate for the next meeting";
    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        UseVanillaKillButton = true,
        Icon = TouRoleIcons.Blackmailer
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return
            $"The {RoleName} is an Impostor Support role that can stop a player from speaking (marked with <color=#2A1119>M</color>) in the next meeting" +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new("Blackmail",
            "Silence a player for the next meeting. They will be unable to speak." +
            "They also will not be able to vote until less or equal amount of people are alive than the blackmailer settings allow." +
            "The blackmail will be visible to other players only if the setting is toggled",
            TouImpAssets.BlackmailSprite)
    ];

    [MethodRpc((uint)TownOfUsRpc.Blackmail, LocalHandling = RpcLocalHandling.Before, SendImmediately = true)]
    public static void RpcBlackmail(PlayerControl source, PlayerControl target)
    {
        var existingBmed = PlayerControl.AllPlayerControls.ToArray()
            .FirstOrDefault(x => x.GetModifier<BlackmailedModifier>()?.BlackMailerId == source.PlayerId);

        existingBmed?.RemoveModifier<BlackmailedModifier>();

        var modifier = new BlackmailedModifier(source.PlayerId);
        target.AddModifier(modifier);
        var touAbilityEvent = new TouAbilityEvent(AbilityType.BlackmailerBlackmail, source, target);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }
}